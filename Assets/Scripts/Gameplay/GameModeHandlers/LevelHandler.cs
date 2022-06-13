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

    private GameInput[] _gameInputs;
    private NodePair[] _nodePairs;
    private TeamManager _teamManager;

    private RegularGameService _gameService;

    public LevelHandler(
        GameManager gameManager, 
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
        _gameSceneManager = gameSceneManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;

        _gameInputs = gameInputs;
        _nodePairs = nodePairs;
        _teamManager = teamManager;

        _gameService = new RegularGameService(
            gameManager: _gameManager,
            gameSceneManager: _gameSceneManager,
            hitManager: _hitManager,
            hitSplitManager: _hitSplitManager,
            gameInputs: _gameInputs,
            nodePairs: _nodePairs,
            teamManager: _teamManager
            );
    }

    public void Initialize()
    {
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

    private void _handleScaling(int numRows, NodePair[] nodePairs)
    {
        if(numRows <= 5)
        {
            _scale = 1f;
        }
        else
        {
            _scale = 5f / numRows;
        }

        foreach(NodePair nodePair in nodePairs)
        {
            nodePair.Node1.transform.localScale = new Vector3(_scale, _scale, 1);
            nodePair.Node2.transform.localScale = new Vector3(_scale, _scale, 1);

            nodePair.LightningManager.SetScale(_scale);
        }
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

            //TODO: fix color bullshit, include particle and lightning shit
            PlayerNodeColors nodeColors = _gameManager.ColorManager.NodeColors[teamId];
            PlayerColorData colorData = _gameManager.DataManager.PlayerColors[teamId];
            nodeColors.InsideColor1 = colorData.Node1InsideColor.Get();
            nodeColors.OutsideColor1 = colorData.Node1OutsideColor.Get();
            nodeColors.ParticleColor1 = colorData.Node1ParticlesColor.Get();
            nodeColors.InsideColor2 = colorData.Node2InsideColor.Get();
            nodeColors.OutsideColor2 = colorData.Node2OutsideColor.Get();
            nodeColors.ParticleColor2 = colorData.Node2ParticlesColor.Get();

            switch (objectData.ObjectType)
            {
                case ObjectTypeEnum.NoHit:
                    _noHitManager.SpawnNoHit(pos, _scale);
                    break;
                case ObjectTypeEnum.Hit1:
                    _hitManager.SpawnHit(HitTypeEnum.Hit1, teamId, nodeColors, pos, _scale);
                    break;
                case ObjectTypeEnum.Hit2:
                    _hitManager.SpawnHit(HitTypeEnum.Hit2, teamId, nodeColors, pos, _scale);
                    break;
                case ObjectTypeEnum.HitSplit1:
                    _hitSplitManager.SpawnHitSplit(
                        hitSplitFirstType: HitTypeEnum.Hit1,
                        hitSplitSecondType: HitTypeEnum.Hit2,
                        firstHitTeamId: teamId,
                        secondHitTeamId: teamId,
                        firstHitNodeColors: nodeColors,
                        secondHitNodeColors: nodeColors,
                        spawnPosition: pos,
                        scale: _scale
                        );
                    break;
                case ObjectTypeEnum.HitSplit2:
                    _hitSplitManager.SpawnHitSplit(
                       hitSplitFirstType: HitTypeEnum.Hit2,
                       hitSplitSecondType: HitTypeEnum.Hit1,
                       firstHitTeamId: teamId,
                       secondHitTeamId: teamId,
                       firstHitNodeColors: nodeColors,
                       secondHitNodeColors: nodeColors,
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
