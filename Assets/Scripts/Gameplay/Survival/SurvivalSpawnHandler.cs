using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class SurvivalSpawnHandler : ISurvivalSpawnHandler
{
    private GameManager _gameManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;
    private SurvivalHandler _survivalHandler;

    private float[] _rowPositions;
    private float _scale;

    private PlayerColors[] _playerColors;

    public SurvivalSpawnHandler(
        SurvivalHandler survivalHandler,
        GameManager gameManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        float[] rowPositions,
        float scale
        )
    {
        _survivalHandler = survivalHandler;
        _gameManager = gameManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;
        _rowPositions = rowPositions;
        _scale = scale;
    }


    public void Spawn()
    {
        //1-9
        int rando = Random.Range(1, 10);

        int teamId = 0;

        //if multiplayer
        if (_gameManager.GameSetupInfo.Teams.Count > 1 || _gameManager.GameSetupInfo.Teams[0].PlayerInputs.Count > 1)
        {
            _playerColors = _gameManager.ColorManager.DefaultPlayerColors.Select(defaultPlayerColors => new PlayerColors(defaultPlayerColors)).ToArray();
        }
        else
        {
            _playerColors = _gameManager.DataManager.PlayerColors.Select(customPlayerColors => new PlayerColors(customPlayerColors)).ToArray();
        }

        PlayerColors playerColors = _playerColors[teamId];

        switch (rando)
        {
            case 1:
                _spawnHit(HitTypeEnum.Hit1, teamId, playerColors);
                _survivalHandler.PotentialMaxScore += 10;
                break;
            case 2:
                _spawnHit(HitTypeEnum.Hit2, teamId, playerColors);
                _survivalHandler.PotentialMaxScore += 10;
                break;
            case 3:
                _spawnBothHits(teamId, playerColors);
                break;
            case 4:
                _hitSplitManager.SpawnHitSplit(
                    hitSplitFirstType: HitTypeEnum.Hit1,
                    hitSplitSecondType: HitTypeEnum.Hit2,
                    firstHitTeamId: teamId,
                    secondHitTeamId: teamId,
                    firstHitPlayerColors: playerColors,
                    secondHitPlayerColors: playerColors,
                    spawnPosition: _getRandomSpawnPosition(),
                    scale: _scale
                    );
                _survivalHandler.PotentialMaxScore += 20;
                break;
            case 5:
                _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: HitTypeEnum.Hit2,
                   hitSplitSecondType: HitTypeEnum.Hit1,
                   firstHitTeamId: teamId,
                   secondHitTeamId: teamId,
                   firstHitPlayerColors: playerColors,
                   secondHitPlayerColors: playerColors,
                   spawnPosition: _getRandomSpawnPosition(),
                   scale: _scale
                   );
                _survivalHandler.PotentialMaxScore += 20;
                break;
            default:
                _spawnNoHit();
                break;
        }
    }

    private Vector3 _getRandomSpawnPosition()
    {
        //0 to Number of Rows - 1
        int i = Random.Range(0, _rowPositions.Length);

        return new Vector3(GameManager.RightX + 5, _rowPositions[i], 0);
    }

    private void _spawnHit(HitTypeEnum hitType, int teamId, PlayerColors playerColors)
    {
        Vector3 pos = _getRandomSpawnPosition();
        _hitManager.SpawnHit(hitType, teamId, playerColors, pos, _scale);
    }

    private void _spawnNoHit()
    {
        Vector3 pos = _getRandomSpawnPosition();
        _noHitManager.SpawnNoHit(pos, _scale);
    }

    private void _spawnBothHits(int teamId, PlayerColors playerColors)
    {
        //add all potential positions
        List<float> availablePositions = new List<float>(_rowPositions);

        //0 to length - 1
        int rando = Random.Range(0, availablePositions.Count);

        //Pull a position out
        float y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit1
        _hitManager.SpawnHit(HitTypeEnum.Hit1, teamId, playerColors, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        //0 to length-1
        rando = Random.Range(0, availablePositions.Count);

        y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit2
        _hitManager.SpawnHit(HitTypeEnum.Hit2, teamId, playerColors, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        availablePositions.Clear();
    }
}
