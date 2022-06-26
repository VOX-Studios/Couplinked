using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class SurvivalCoOpSpawnHandler : ISurvivalSpawnHandler
{
    private GameManager _gameManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;
    private SurvivalHandler _survivalHandler;
    private int _lastTeamId;

    private float[] _rowPositions;
    private float _scale;

    private PlayerColors[] _playerColors;

    public SurvivalCoOpSpawnHandler(
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
        _lastTeamId = -1;

        _playerColors = _gameManager.ColorManager.DefaultPlayerColors.Select(defaultPlayerColors => new PlayerColors(defaultPlayerColors)).ToArray();
    }

    public void Spawn()
    {
        //1-9
        int rando = Random.Range(1, 10);

        int teamId = 0;

        switch (_lastTeamId)
        {
            default:
            case -1:
                //0-1
                teamId = Random.Range(0, 2);
                break;
            case 0:
                teamId = 1;
                break;
            case 1:
                teamId = 0;
                break;

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
            case 5:
                _spawnHitSplit(teamId, playerColors);
                break;
            default:
                teamId = -1;
                _spawnNoHit();
                break;
        }

        _lastTeamId = teamId;
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

    private void _spawnBothHits(int firstTeamId, PlayerColors firstNodePlayerColors)
    {
        HitTypeEnum firstHitType = _getRandomHitType();

        //0-1
        int secondTeamId = Random.Range(0, 2);
        PlayerColors secondHitPlayerColors = _playerColors[secondTeamId];

        HitTypeEnum secondHitType;
        if (firstTeamId == secondTeamId)
        {
            if (firstHitType == HitTypeEnum.Hit1)
                secondHitType = HitTypeEnum.Hit2;
            else
                secondHitType = HitTypeEnum.Hit1;
        }
        else //different teams
        {
            secondHitType = _getRandomHitType();
        }

        //add all potential positions
        List<float> availablePositions = new List<float>(_rowPositions);

        //0 to length - 1
        int rando = Random.Range(0, availablePositions.Count);

        //Pull a position out
        float y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit1
        _hitManager.SpawnHit(firstHitType, firstTeamId, firstNodePlayerColors, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        //0 to length-1
        rando = Random.Range(0, availablePositions.Count);

        y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit2
        _hitManager.SpawnHit(secondHitType, secondTeamId, secondHitPlayerColors, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        availablePositions.Clear();
    }

    private void _spawnHitSplit(int firstTeamId, PlayerColors firstHitPlayerColors)
    {
        HitTypeEnum firstHitType = _getRandomHitType();

        //0-1
        int secondTeamId = Random.Range(0, 2);
        PlayerColors secondHitPlayerColors = _playerColors[secondTeamId];

        HitTypeEnum secondHitType;
        if (firstTeamId == secondTeamId)
        {
            if (firstHitType == HitTypeEnum.Hit1)
                secondHitType = HitTypeEnum.Hit2;
            else
                secondHitType = HitTypeEnum.Hit1;
        }
        else //different teams
        {
            secondHitType = _getRandomHitType();
        }

        Vector3 pos = _getRandomSpawnPosition();

        _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: firstHitType,
                   hitSplitSecondType: secondHitType,
                   firstHitTeamId: firstTeamId,
                   secondHitTeamId: secondTeamId,
                   firstHitPlayerColors: firstHitPlayerColors,
                   secondHitPlayerColors: secondHitPlayerColors,
                   spawnPosition: pos,
                   scale: _scale
                   );

        _survivalHandler.PotentialMaxScore += 20;
    }

    private HitTypeEnum _getRandomHitType()
    {
        //1-2
        int random = Random.Range(1, 3);

        if (random == 1)
            return HitTypeEnum.Hit1;
        else
            return HitTypeEnum.Hit2;
    }
}
