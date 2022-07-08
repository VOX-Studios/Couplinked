using Assets.Scripts.SceneManagers;
using System.Linq;
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
    private NodePairing[] _nodePairs;

    private RegularGameService _gameService;

    private NodeColors[] _nodeColors;


    public LevelHandler(
        GameManager gameManager, 
        GameSceneManager gameSceneManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        GameInput[][] gameInputs,
        NodePairing[] nodePairs
        )
    {
        _gameManager = gameManager;
        _gameSceneManager = gameSceneManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;

        _gameInputs = gameInputs;
        _nodePairs = nodePairs;

        _gameService = new RegularGameService(
            gameManager: _gameManager,
            gameSceneManager: _gameSceneManager,
            hitManager: _hitManager,
            hitSplitManager: _hitSplitManager,
            gameInputs: _gameInputs,
            nodePairs: _nodePairs
            );
    }

    public void Initialize()
    {
        //if multiplayer
        if (_gameManager.GameSetupInfo.Teams.Count > 1 || _gameManager.GameSetupInfo.Teams[0].PlayerInputs.Count > 1)
        {
            _nodeColors = _gameManager.ColorManager.DefaultPlayerColors.Take(2)
                .Select(defaultPlayerColors=> new NodeColors(defaultPlayerColors.NodeColors[0]))
                .ToArray();
        }
        else //singleplayer
        {
            _nodeColors = _gameManager.DataManager.PlayerColors[0].NodeColors.Select(nodeColorData => new NodeColors(nodeColorData)).ToArray();
        }

        _rowPositions = _calculateRowPositions(_gameManager.CurrentLevel.NumberOfRows);
        _handleScaling(_gameManager.CurrentLevel.NumberOfRows, _nodePairs);
        _gameService.Start();

        _timeLengthOfScreen = GameManager.WorldWidth / _gameManager.GameDifficultyManager.ObjectSpeed;

        _hasTimerStartedForEverythingInactive = false;
        _timeSinceLastActiveObject = 0;

        _gameTimeAtRightScreen = -_gameManager.GameDifficultyManager.LevelStartDelay;
        _nextSpawn = 0;
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

    private void _handleScaling(int numRows, NodePairing[] nodePairs)
    {
        if(numRows <= 5)
        {
            _scale = 1f;
        }
        else
        {
            _scale = 5f / numRows;
        }

        foreach(NodePairing nodePair in nodePairs)
        {
            nodePair.SetScale(_scale);
        }

        _gameSceneManager.CameraShake.Scale = _scale;
    }


    public void Run(float deltaTime)
    {
        _gameTimeAtRightScreen += deltaTime * _gameManager.GameDifficultyManager.GameTimeModifier;

        _handleSpawners();

        _gameService.RunInput(deltaTime);
    }

    private void _handleSpawners()
    {
        while (_nextSpawn < _gameManager.CurrentLevel.Data.Count && _gameManager.CurrentLevel.Data[_nextSpawn].Time <= _gameTimeAtRightScreen + .5f) //TODO: the + .5f should be a calculated value based on object length
        {
            ObjectData objectData = _gameManager.CurrentLevel.Data[_nextSpawn];

            float x = GameManager.RightX + ((objectData.Time - _gameTimeAtRightScreen) / _timeLengthOfScreen) * GameManager.WorldWidth;

            Vector3 pos = new Vector3(x, 0, 0);

            if(objectData.ObjectRow >= _rowPositions.Length)
            {
                //TODO: error? Might not need to...can just not spawn
                _nextSpawn++;
                continue;
            }

            pos.y = _rowPositions[objectData.ObjectRow];

            int teamId = 0;

            switch (objectData.ObjectType)
            {
                case ObjectTypeEnum.NoHit:
                    _noHitManager.SpawnNoHit(pos, _scale);
                    break;
                case ObjectTypeEnum.Hit1:
                    _hitManager.SpawnHit(0, teamId, _nodeColors[0], pos, _scale);
                    break;
                case ObjectTypeEnum.Hit2:
                    _hitManager.SpawnHit(1, teamId, _nodeColors[1], pos, _scale);
                    break;
                case ObjectTypeEnum.HitSplit1:
                    _hitSplitManager.SpawnHitSplit(
                        hitSplitFirstType: 0,
                        hitSplitSecondType: 1,
                        firstHitTeamId: teamId,
                        secondHitTeamId: teamId,
                        firstHitNodeColors: _nodeColors[0],
                        secondHitNodeColors: _nodeColors[1],
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
                       firstHitNodeColors: _nodeColors[1],
                       secondHitNodeColors: _nodeColors[0],
                       spawnPosition: pos,
                       scale: _scale
                       );
                    break;
            }

            _nextSpawn++;
        }

        if (_nextSpawn >= _gameManager.CurrentLevel.Data.Count)
        {
            //will always be > 0....redundant here
            if (_gameManager.CurrentLevel.Data.Count > 0)
            {
                if (!_hasTimerStartedForEverythingInactive && _noHitManager.activeNoHits.Count + _hitManager.activeHits.Count + _hitSplitManager.activeHitSplits.Count == 0)
                {
                    _hasTimerStartedForEverythingInactive = true;
                }

                if (_hasTimerStartedForEverythingInactive)
                    _timeSinceLastActiveObject += Time.deltaTime;

                const float timeBeforeGameEnds = 5f;
                if (_timeSinceLastActiveObject > timeBeforeGameEnds || _gameTimeAtRightScreen - _timeLengthOfScreen > _gameManager.CurrentLevel.Data[_gameManager.CurrentLevel.Data.Count - 1].Time)
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
}
