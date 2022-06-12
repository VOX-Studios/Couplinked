using System.Collections.Generic;
using UnityEngine;

class SurvivalSpawnHandler : ISurvivalSpawnHandler
{
    private GameManager _gameManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;
    private SurvivalHandler _survivalHandler;

    private float[] _rowPositions;

    public SurvivalSpawnHandler(
        SurvivalHandler survivalHandler,
        GameManager gameManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        float[] rowPositions
        )
    {
        _survivalHandler = survivalHandler;
        _gameManager = gameManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;
        _rowPositions = rowPositions;
    }


    public void Spawn()
    {
        //1-9
        int rando = Random.Range(1, 10);

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

        switch (rando)
        {
            case 1:
                _spawnHit(HitTypeEnum.Hit1, teamId, nodeColors);
                _survivalHandler.PotentialMaxScore += 10;
                break;
            case 2:
                _spawnHit(HitTypeEnum.Hit2, teamId, nodeColors);
                _survivalHandler.PotentialMaxScore += 10;
                break;
            case 3:
                _spawnBothHits(teamId, nodeColors);
                break;
            case 4:
                _hitSplitManager.SpawnHitSplit(
                    hitSplitFirstType: HitTypeEnum.Hit1,
                    hitSplitSecondType: HitTypeEnum.Hit2,
                    firstHitTeamId: teamId,
                    secondHitTeamId: teamId,
                    firstHitNodeColors: nodeColors,
                    secondHitNodeColors: nodeColors,
                    spawnPosition: _getRandomSpawnPosition()
                    );
                _survivalHandler.PotentialMaxScore += 20;
                break;
            case 5:
                _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: HitTypeEnum.Hit2,
                   hitSplitSecondType: HitTypeEnum.Hit1,
                   firstHitTeamId: teamId,
                   secondHitTeamId: teamId,
                   firstHitNodeColors: nodeColors,
                   secondHitNodeColors: nodeColors,
                   spawnPosition: _getRandomSpawnPosition()
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

    private void _spawnHit(HitTypeEnum hitType, int teamId, PlayerNodeColors nodeColors)
    {
        Vector3 pos = _getRandomSpawnPosition();
        _hitManager.SpawnHit(hitType, teamId, nodeColors, pos);
    }

    private void _spawnNoHit()
    {
        Vector3 pos = _getRandomSpawnPosition();
        _noHitManager.SpawnNoHit(pos);
    }

    private void _spawnBothHits(int teamId, PlayerNodeColors nodeColors)
    {
        //add all potential positions
        List<float> availablePositions = new List<float>(_rowPositions);

        //0 to length - 1
        int rando = Random.Range(0, availablePositions.Count);

        //Pull a position out
        float y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit1
        _hitManager.SpawnHit(HitTypeEnum.Hit1, teamId, nodeColors, new Vector3(GameManager.RightX + 5, y, 0));
        _survivalHandler.PotentialMaxScore += 20;

        //0 to length-1
        rando = Random.Range(0, availablePositions.Count);

        y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit2
        _hitManager.SpawnHit(HitTypeEnum.Hit2, teamId, nodeColors, new Vector3(GameManager.RightX + 5, y, 0));
        _survivalHandler.PotentialMaxScore += 20;

        availablePositions.Clear();
    }
}
