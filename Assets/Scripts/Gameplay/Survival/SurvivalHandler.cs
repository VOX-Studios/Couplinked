using Assets.Scripts.SceneManagers;
using UnityEngine;

class SurvivalHandler : IGameModeHandler
{
    private double _originalSpawnInterval = 1; //2 = easy; 1 = normal; .5 = hard;
    private double _spawnTimer;
    private double _spawnInterval;
    private double _minSpawnInterval = .5;

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

    private GameplayUtility _gameplayUtility;

    private GameInput[] _gameInputs;
    private NodePair[] _nodePairs;
    private TeamManager _teamManager;

    private RegularGameService _gameService;

    private ISurvivalSpawnHandler _spawnHandler;

    public SurvivalHandler(
        GameManager gameManager, 
        GameplayUtility gameplayUtility, 
        GameSceneManager gameSceneManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        GameInput[] gameInputs,
        NodePair[] nodePairs,
        TeamManager teamManager
        )
    {
        _gameManager = gameManager;
        _gameplayUtility = gameplayUtility;
        _gameSceneManager = gameSceneManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;

        _gameInputs = gameInputs;
        _nodePairs = nodePairs;
        _teamManager = teamManager;

        _gameService = new RegularGameService(
            gameManager: _gameManager,
            gameplayUtility: _gameplayUtility,
            gameSceneManager: _gameSceneManager,
            hitManager: _hitManager,
            hitSplitManager: _hitSplitManager,
            gameInputs: _gameInputs,
            nodePairs: _nodePairs,
            teamManager: _teamManager
            );
    }

    public void Start()
    {
        if (_gameManager.GameSetupInfo.GameMode == GameModeEnum.Survival)
        {
            _spawnHandler = new SurvivalSpawnHandler(
                survivalHandler: this,
                gameManager: _gameManager,
                hitManager: _hitManager,
                hitSplitManager: _hitSplitManager,
                noHitManager: _noHitManager
                );
        }
        else
        {
            _spawnHandler = new SurvivalCoOpSpawnHandler(
                survivalHandler: this,
                gameManager: _gameManager,
                hitManager: _hitManager,
                hitSplitManager: _hitSplitManager,
                noHitManager: _noHitManager
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
