using System.Collections.Generic;
using UnityEngine;

class SurvivalSpawnHandler : ISurvivalSpawnHandler
{
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;
    private NoHitManager _noHitManager;
    private SurvivalHandler _survivalHandler;

    private float[] _rowPositions;
    private float _scale;

    private NodePairing[] _nodePairings;

    public SurvivalSpawnHandler(
        SurvivalHandler survivalHandler,
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        NodePairing[] nodePairings,
        float[] rowPositions,
        float scale
        )
    {
        _survivalHandler = survivalHandler;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;
        _noHitManager = noHitManager;
        _nodePairings = nodePairings;
        _rowPositions = rowPositions;
        _scale = scale;
    }


    public void Spawn()
    {
        //1-9
        int rando = Random.Range(1, 10);

        int teamId = 0;

        switch (rando)
        {
            case 1:
                _spawnHit(0, teamId, _nodePairings);
                break;
            case 2:
                _spawnHit(1, teamId, _nodePairings);
                break;
            case 3:
                _spawnBothHits(teamId, _nodePairings);
                break;
            case 4:
                _hitSplitManager.SpawnHitSplit(
                    hitSplitFirstType: 0,
                    hitSplitSecondType: 1,
                    firstHitTeamId: teamId,
                    secondHitTeamId: teamId,
                    firstHitColor: _nodePairings[teamId].Nodes[0].OutsideColor,
                    secondHitColor: _nodePairings[teamId].Nodes[1].OutsideColor,
                    spawnPosition: _getRandomSpawnPosition(),
                    scale: _scale
                    );
                _survivalHandler.PotentialMaxScore += 20;
                break;
            case 5:
                _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: 1,
                   hitSplitSecondType: 0,
                   firstHitTeamId: teamId,
                   secondHitTeamId: teamId,
                   firstHitColor: _nodePairings[teamId].Nodes[1].OutsideColor,
                   secondHitColor: _nodePairings[teamId].Nodes[0].OutsideColor,
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

    private void _spawnHit(int nodeId, int teamId, NodePairing[] nodePairings)
    {
        Color hitColor = nodePairings[teamId].Nodes[nodeId].OutsideColor;
        Vector3 pos = _getRandomSpawnPosition();
        _hitManager.SpawnHit(nodeId, teamId, hitColor, pos, _scale);
        _survivalHandler.PotentialMaxScore += 10;
    }

    private void _spawnNoHit()
    {
        Vector3 pos = _getRandomSpawnPosition();
        _noHitManager.SpawnNoHit(pos, _scale);
    }

    private void _spawnBothHits(int teamId, NodePairing[] nodePairings)
    {
        //add all potential positions
        List<float> availablePositions = new List<float>(_rowPositions);

        //0 to length - 1
        int rando = Random.Range(0, availablePositions.Count);

        //Pull a position out
        float y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit1
        _hitManager.SpawnHit(0, teamId, nodePairings[teamId].Nodes[0].OutsideColor, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        //0 to length-1
        rando = Random.Range(0, availablePositions.Count);

        y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit2
        _hitManager.SpawnHit(1, teamId, nodePairings[teamId].Nodes[1].OutsideColor, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        availablePositions.Clear();
    }
}
