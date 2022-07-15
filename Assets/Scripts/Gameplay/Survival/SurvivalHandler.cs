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

    private byte _numberOfRows = 5; //TODO: handle new lighting system scaling to match
    private float[] _rowPositions;
    private float _scale;

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
            hitManager: _hitManager,
            hitSplitManager: _hitSplitManager,
            gameInputs: _gameInputs,
            nodePairs: _nodePairings
            );
    }

    private float[] _calculateRowPositions(int numRows)
    {
        float[] rowPositions = new float[numRows];

        float borderPadding = 2.5f; //TODO: share this with clamp
        rowPositions[0] = GameManager.TopY - borderPadding;
        rowPositions[rowPositions.Length - 1] = GameManager.BotY + borderPadding;

        //take top and bot and divide by remaining positions
        float spacing = (rowPositions[0] - rowPositions[rowPositions.Length - 1]) / (rowPositions.Length - 1);

        for (int i = 1; i < rowPositions.Length - 1; i++)
        {
            rowPositions[i] = rowPositions[0] - (spacing * i);
        }

        return rowPositions;
    }

    private void _handleScaling(int numRows, NodePairing[] nodePairs, ExplosionManager explosionManager)
    {
        if (numRows <= 3)
        {
            _scale = 1f;
        }
        else
        {
            _scale = 3f / numRows;
        }

        foreach (NodePairing nodePair in nodePairs)
        {
            nodePair.SetScale(_scale);
        }

        _gameManager.LightingManager.SetScale(_scale);
        explosionManager.SetScale(_scale);
        _gameSceneManager.CameraShake.Scale = _scale;
    }

    public void Initialize()
    {
        _rowPositions = _calculateRowPositions(_numberOfRows);
        _handleScaling(_numberOfRows, _nodePairings, _explosionManager);

        if (_gameManager.GameSetupInfo.GameMode == GameModeEnum.Survival)
        {
            _spawnHandler = new SurvivalSpawnHandler(
                survivalHandler: this,
                gameManager: _gameManager,
                hitManager: _hitManager,
                hitSplitManager: _hitSplitManager,
                noHitManager: _noHitManager,
                nodePairings: _nodePairings,
                rowPositions: _rowPositions,
                scale: _scale
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
                rowPositions: _rowPositions,
                scale: _scale
                );
        }

        _originalSpawnInterval = 1;
        _minSpawnInterval = 0.5;

        _spawnInterval = _originalSpawnInterval;
        _gameService.Start();
    }

    public void Run(float deltaTime)
    {
        _handleSpawners(deltaTime);
        _gameService.RunInput(deltaTime);
    }

    private void _handleSpawners(float deltaTime)
    {
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

    public void OnHitCollision(Hit hit, Collider2D other)
    {
        _gameService.OnHitCollision(hit, other);
    }

    public void OnHitSplitCollision(HitSplit hitSplit, Collider2D other)
    {
        _gameService.OnHitSplitCollision(hitSplit, other);
    }

    public void OnNoHitCollision(NoHit noHit, Collider2D other)
    {
        _gameService.OnNoHitCollision(noHit, other);
    }

    public void OnGameEnd()
    {
        _spawnInterval = _originalSpawnInterval;
        _spawnTimer = 0;
    }
}
