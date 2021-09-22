using System.Collections.Generic;
using UnityEngine;

class SurvivalCoOpSpawnHandler : ISurvivalSpawnHandler
{
    private GameManager _gameManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;
    private SurvivalHandler _survivalHandler;
    private int _lastTeamId;

    public SurvivalCoOpSpawnHandler(
        SurvivalHandler survivalHandler,
        GameManager gameManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager
        )
    {
        _survivalHandler = survivalHandler;
        _gameManager = gameManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;

        _lastTeamId = -1;
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

        //TODO: fix color bullshit, include particle and lightning shit
        PlayerNodeColors nodeColors = _gameManager.ColorManager.NodeColors[teamId];
        PlayerColorData colorData = _gameManager.DataManager.PlayerColors[teamId];
        nodeColors.InsideColor1 = colorData.Node1InsideColor.Get();
        nodeColors.OutsideColor1 = colorData.Node1OutsideColor.Get();
        nodeColors.ParticleColor1 = colorData.Node1ParticlesColor.Get();
        nodeColors.InsideColor2 = colorData.Node2InsideColor.Get();
        nodeColors.OutsideColor2 = colorData.Node2OutsideColor.Get();
        nodeColors.ParticleColor2 = colorData.Node2ParticlesColor.Get();

        switch (rando)
        {
            case 1:
                _hitManager.SpawnHit(HitTypeEnum.Hit1, teamId, nodeColors);
                _survivalHandler.PotentialMaxScore += 10;
                break;
            case 2:
                _hitManager.SpawnHit(HitTypeEnum.Hit2, teamId, nodeColors);
                _survivalHandler.PotentialMaxScore += 10;
                break;
            case 3:
                _spawnBothHits(teamId, nodeColors);
                break;
            case 4:
            case 5:
                _spawnHitSplit(teamId, nodeColors);
                break;
            default:
                teamId = -1;
                _noHitManager.SpawnNoHit();
                break;
        }

        _lastTeamId = teamId;
    }

    private void _spawnBothHits(int firstTeamId, PlayerNodeColors firstNodeColors)
    {
        HitTypeEnum firstHitType = _getRandomHitType();

        //0-1
        int secondTeamId = Random.Range(0, 2);
        PlayerNodeColors secondHitNodeColors = _gameManager.ColorManager.NodeColors[secondTeamId];

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
        List<float> availablePositions = new List<float>();
        availablePositions.Add(GameManager.TopLaneY);
        availablePositions.Add(GameManager.MidLaneY);
        availablePositions.Add(GameManager.BotLaneY);

        //0 to length - 1
        int rando = Random.Range(0, availablePositions.Count);

        //Pull a position out
        float y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit1
        _hitManager.SpawnHit(firstHitType, firstTeamId, firstNodeColors, new Vector3(GameManager.RightX + 5, y, 0));
        _survivalHandler.PotentialMaxScore += 20;

        //0 to length-1
        rando = Random.Range(0, availablePositions.Count);

        y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit2
        _hitManager.SpawnHit(secondHitType, secondTeamId, secondHitNodeColors, new Vector3(GameManager.RightX + 5, y, 0));
        _survivalHandler.PotentialMaxScore += 20;

        availablePositions.Clear();
    }

    private void _spawnHitSplit(int firstTeamId, PlayerNodeColors firstHitNodeColors)
    {
        HitTypeEnum firstHitType = _getRandomHitType();

        //0-1
        int secondTeamId = Random.Range(0, 2);
        PlayerNodeColors secondHitNodeColors = _gameManager.ColorManager.NodeColors[secondTeamId];

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

        _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: firstHitType,
                   hitSplitSecondType: secondHitType,
                   firstHitTeamId: firstTeamId,
                   secondHitTeamId: secondTeamId,
                   firstHitNodeColors: firstHitNodeColors,
                   secondHitNodeColors: secondHitNodeColors
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
