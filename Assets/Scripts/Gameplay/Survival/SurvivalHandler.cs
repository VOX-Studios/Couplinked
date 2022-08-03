using Assets.Scripts.GameEntities;
using Assets.Scripts.Gameplay;
using Assets.Scripts.SceneManagers;
using UnityEngine;

class SurvivalHandler : IGameModeHandler
{
    private double _originalSpawnInterval = 1;

    /// <summary>
    /// The time remaining until the next spawn.
    /// </summary>
    private double _spawnTimer;
    
    /// <summary>
    /// The amount of time between each spawn.
    /// </summary>
    private double _spawnInterval;

    /// <summary>
    /// The smallest amount of time allowed between each spawn (aka spawn interval can't be less than this number).
    /// </summary>
    private double _minSpawnInterval = .5;

    private byte _numberOfRows = 5; //TODO: make new lighting system scale to match changes to number of rows

    private GameManager _gameManager;
    private GameSceneManager _gameSceneManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;

    public int PotentialMaxScore;

    public bool IsPaused = false;
    public bool IsResuming = false;

    public int Score = 0;
    public int RingsCollected = 0;

    private GameInput[][] _gameInputs;
    private NodePairing[] _nodePairings;
    private ExplosionManager _explosionManager;

    private RegularGameService _gameService;

    private ISurvivalSpawnHandler _spawnHandler;

    public SurvivalHandler(
        GameManager gameManager,
        GameSceneManager gameSceneManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        GameInput[][] gameInputs,
        NodePairing[] nodePairs,
        ExplosionManager explosionManager
        )
    {
        _gameManager = gameManager;
        _gameSceneManager = gameSceneManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;

        _gameInputs = gameInputs;
        _nodePairings = nodePairs;
        _explosionManager = explosionManager;

        _gameService = new RegularGameService(
            gameManager: _gameManager,
            gameSceneManager: _gameSceneManager,
            noHitManager: _noHitManager,
            hitManager: _hitManager,
            hitSplitManager: _hitSplitManager,
            gameInputs: _gameInputs,
            nodePairs: _nodePairings,
            gameModeHandler: this
            );
    }

    public void Initialize()
    {
        float[] rowPositions = RowPositionsUtility.CalculateRowPositions(_numberOfRows);
        float scale = ScaleUtility.CalculateScale(_numberOfRows);
        _gameService.SetScale(scale, _nodePairings, _explosionManager);

        if (_gameManager.GameSetupInfo.GameMode == GameModeEnum.Survival)
        {
            _spawnHandler = new SurvivalSpawnHandler(
                survivalHandler: this,
                gameManager: _gameManager,
                hitManager: _hitManager,
                hitSplitManager: _hitSplitManager,
                noHitManager: _noHitManager,
                nodePairings: _nodePairings,
                rowPositions: rowPositions,
                scale: scale
                );
        }
        else
        {
            _spawnHandler = new SurvivalCoOpSpawnHandler(
                survivalHandler: this,
                gameManager: _gameManager,
                hitManager: _hitManager,
                hitSplitManager: _hitSplitManager,
                noHitManager: _noHitManager,
                nodePairings: _nodePairings,
                rowPositions: rowPositions,
                scale: scale
                );
        }

        _originalSpawnInterval = 1;
        _minSpawnInterval = 0.5;

        _spawnInterval = _originalSpawnInterval;
        _gameService.Start();
    }

    public void Run(bool isPaused, float deltaTime)
    {
        _handleSpawners(isPaused, deltaTime);
        _gameService.Run(isPaused, deltaTime);
    }

    private void _handleSpawners(bool isPaused, float deltaTime)
    {
        if(isPaused)
        {
            return;
        }
        _spawnTimer -= deltaTime;

        if (_spawnTimer > 0)
            return;

        if (_spawnInterval > _minSpawnInterval)
        {
            _spawnInterval -= .01;
        }
        else if (_spawnInterval < _minSpawnInterval)
        {
            _spawnInterval = _minSpawnInterval;

            //TODO: make this pop up INSTANTLY with small window
            string unlockMessage = "";
            if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_SurviveToMaxSpeed, out unlockMessage))
            {
                _gameManager.NotificationManager.QueueNotification(unlockMessage);
            }
        }

        _spawnHandler.Spawn();

        _spawnTimer = _spawnInterval;
    }

    public void OnCollision(Hit hit, Collider2D other)
    {
        _gameService.OnHitCollision(hit, other);
    }

    public void OnCollision(HitSplit hitSplit, Collider2D other)
    {
        _gameService.OnHitSplitCollision(hitSplit, other);
    }

    public void OnCollision(NoHit noHit, Collider2D other)
    {
        _gameService.OnNoHitCollision(noHit, other);
    }

    public void OnGameEntityOffScreen(IGameEntity gameEntity)
    {
        ReasonForGameEndEnum reasonForGameEnd;
        Color color;
        if (gameEntity.GameEntityType == GameEntityTypeEnum.Hit)
        {
            reasonForGameEnd = ReasonForGameEndEnum.HitOffScreen;
            color = (gameEntity as Hit).Color;
        }
        else if (gameEntity.GameEntityType == GameEntityTypeEnum.HitSplit)
        {
            reasonForGameEnd = ReasonForGameEndEnum.HitSplitOffScreen;
            color = (gameEntity as HitSplit).OutsideColor;
        }
        else
        {
            return;
        }

        _gameSceneManager.CameraShake.StartShake(.34f);
        _gameSceneManager.SideExplosionManager.ActivateExplosion(gameEntity.Transform.position.y, color);

        //end the game
        _gameSceneManager.EndGame(reasonForGameEnd);

        Vector2 vignettePosition = new Vector2(GameManager.LeftX, gameEntity.Transform.position.y);
        _gameSceneManager.VignetteManager.StartClosePhase1(vignettePosition, .1f);
    }

    public void OnGameEnd()
    {
        _spawnInterval = _originalSpawnInterval;
        _spawnTimer = 0;
    }
}
