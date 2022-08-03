using Assets.Scripts.GameEntities;
using Assets.Scripts.Gameplay;
using Assets.Scripts.SceneManagers;
using UnityEngine;

class LevelHandler : IGameModeHandler
{
    private int _nextSpawn;
    private float _gameTimeAtRightScreen;

    private bool _hasTimerStartedForEverythingInactive;
    private float _timeSinceLastActiveObject;

    private float[] _rowPositions;
    private float _scale;

    /// <summary>
    /// How many seconds the length of the screen represents
    /// </summary>
    private float _timeLengthOfScreen;

    private GameManager _gameManager;
    private GameSceneManager _gameSceneManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;

    public bool IsPaused = false;
    public bool IsResuming = false;

    public int Score = 0;
    public int RingsCollected = 0;

    private GameInput[][] _gameInputs;
    private NodePairing[] _nodePairings;
    private ExplosionManager _explosionManager;

    private RegularGameService _gameService;


    public LevelHandler(
        GameManager gameManager, 
        GameSceneManager gameSceneManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        GameInput[][] gameInputs,
        NodePairing[] nodePairings,
        ExplosionManager explosionManager
        )
    {
        _gameManager = gameManager;
        _gameSceneManager = gameSceneManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;

        _gameInputs = gameInputs;
        _nodePairings = nodePairings;
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
        _rowPositions = RowPositionsUtility.CalculateRowPositions(_gameManager.CurrentLevel.NumberOfRows);
        _scale = ScaleUtility.CalculateScale(_gameManager.CurrentLevel.NumberOfRows);
        _gameService.SetScale(_scale, _nodePairings, _explosionManager);

        _gameService.Start();

        _timeLengthOfScreen = GameManager.WorldWidth / (_gameManager.GameDifficultyManager.ObjectSpeed * _scale);

        _hasTimerStartedForEverythingInactive = false;
        _timeSinceLastActiveObject = 0;

        _gameTimeAtRightScreen = -_gameManager.GameDifficultyManager.LevelStartDelay;
        _nextSpawn = 0;
    }

    public void Run(bool isPaused, float deltaTime)
    {
        if (isPaused)
        {
            return;
        }

        _gameTimeAtRightScreen += deltaTime * _gameManager.GameDifficultyManager.GameTimeModifier;

        _handleSpawners();

        _gameService.Run(isPaused, deltaTime);
    }

    private void _handleSpawners()
    {
        //starting from our last spawn, loop through the next objects to spawn as long as they're within the screen length
        while (_nextSpawn < _gameManager.CurrentLevel.Data.Count && _gameManager.CurrentLevel.Data[_nextSpawn].Time <= _gameTimeAtRightScreen + .5f) //TODO: the + .5f should be a calculated value based on object length
        {
            //get the data for the next object to spawn
            ObjectData objectData = _gameManager.CurrentLevel.Data[_nextSpawn];

            //get the x coordinate of the spawn position
            float x = GameManager.RightX + ((objectData.Time - _gameTimeAtRightScreen) / _timeLengthOfScreen) * GameManager.WorldWidth;

            Vector3 pos = new Vector3(x, 0, 0);

            //if the object row listed isn't supported
            if(objectData.ObjectRow < 0 || objectData.ObjectRow >= _rowPositions.Length)
            {
                //TODO: bad data...maybe throw an error?

                //skip to next spawn
                _nextSpawn++;
                continue;
            }

            //set the y coordinate of the spawn position
            pos.y = _rowPositions[objectData.ObjectRow];

            int teamId = 0;

            switch (objectData.ObjectType)
            {
                case ObjectTypeEnum.NoHit:
                    _noHitManager.SpawnNoHit(pos, _scale);
                    break;
                case ObjectTypeEnum.Hit1:
                    _hitManager.SpawnHit(0, teamId, _nodePairings[teamId].Nodes[0].OutsideColor, pos, _scale);
                    break;
                case ObjectTypeEnum.Hit2:
                    _hitManager.SpawnHit(1, teamId, _nodePairings[teamId].Nodes[1].OutsideColor, pos, _scale);
                    break;
                case ObjectTypeEnum.HitSplit1:
                    _hitSplitManager.SpawnHitSplit(
                        hitSplitFirstType: 0,
                        hitSplitSecondType: 1,
                        firstHitTeamId: teamId,
                        secondHitTeamId: teamId,
                        firstHitColor: _nodePairings[teamId].Nodes[0].OutsideColor,
                        secondHitColor: _nodePairings[teamId].Nodes[1].OutsideColor,
                        spawnPosition: pos,
                        scale: _scale
                        );
                    break;
                case ObjectTypeEnum.HitSplit2:
                    _hitSplitManager.SpawnHitSplit(
                       hitSplitFirstType: 1,
                       hitSplitSecondType: 0,
                       firstHitTeamId: teamId,
                       secondHitTeamId: teamId,
                       firstHitColor: _nodePairings[teamId].Nodes[1].OutsideColor,
                       secondHitColor: _nodePairings[teamId].Nodes[0].OutsideColor,
                       spawnPosition: pos,
                       scale: _scale
                       );
                    break;
            }

            _nextSpawn++;
        }

        //if we've spawned everything already
        if (_nextSpawn >= _gameManager.CurrentLevel.Data.Count)
        {
            //if we actually had things to spawn in the level
            if (_gameManager.CurrentLevel.Data.Count > 0)
            {
                //if we haven't started the timer yet and everything has despawned
                if (!_hasTimerStartedForEverythingInactive && _noHitManager.ActiveGameEntities.Count + _hitManager.ActiveGameEntities.Count + _hitSplitManager.ActiveGameEntities.Count == 0)
                {
                    //activate the timer
                    _hasTimerStartedForEverythingInactive = true;
                }

                //if the timer is active
                if (_hasTimerStartedForEverythingInactive)
                {
                    //increment the timer
                    _timeSinceLastActiveObject += Time.deltaTime;
                }

                //this needs to be greater than or equal to how long our effects last (like score juice)
                const float timeBeforeGameEnds = 1.5f;

                //if our timer has be active long enough
                if (_timeSinceLastActiveObject >= timeBeforeGameEnds)
                {
                    _gameManager.ReasonForGameEnd = ReasonForGameEndEnum.Win;
                }
            }
            else
            {
                _gameManager.ReasonForGameEnd = ReasonForGameEndEnum.Win;
            }

        }
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
        //nothing to do here
    }
}
