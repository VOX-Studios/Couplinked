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

    private NodeColors[] _nodeColors;

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

        //if multiplayer
        if (_gameManager.GameSetupInfo.Teams.Count > 1 || _gameManager.GameSetupInfo.Teams[0].PlayerInputs.Count > 1)
        {
            _nodeColors = _gameManager.ColorManager.DefaultPlayerColors.Take(2)
                .Select(defaultPlayerColors => new NodeColors(defaultPlayerColors.NodeColors[0]))
                .ToArray();
        }
        else //singleplayer
        {
            _nodeColors = _gameManager.DataManager.PlayerColors[0].NodeColors.Select(nodeColorData => new NodeColors(nodeColorData)).ToArray();
        }
    }


    public void Spawn()
    {
        //1-9
        int rando = Random.Range(1, 10);

        int teamId = 0;

        switch (rando)
        {
            case 1:
                _spawnHit(0, teamId, _nodeColors[0]);
                break;
            case 2:
                _spawnHit(1, teamId, _nodeColors[1]);
                break;
            case 3:
                _spawnBothHits(teamId, _nodeColors[0], _nodeColors[1]);
                break;
            case 4:
                _hitSplitManager.SpawnHitSplit(
                    hitSplitFirstType: 0,
                    hitSplitSecondType: 1,
                    firstHitTeamId: teamId,
                    secondHitTeamId: teamId,
                    firstHitNodeColors: _nodeColors[0],
                    secondHitNodeColors: _nodeColors[1],
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
                   firstHitNodeColors: _nodeColors[1],
                   secondHitNodeColors: _nodeColors[0],
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

    private void _spawnHit(int nodeId, int teamId, NodeColors nodeColors)
    {
        Vector3 pos = _getRandomSpawnPosition();
        _hitManager.SpawnHit(nodeId, teamId, nodeColors, pos, _scale);
        _survivalHandler.PotentialMaxScore += 10;
    }

    private void _spawnNoHit()
    {
        Vector3 pos = _getRandomSpawnPosition();
        _noHitManager.SpawnNoHit(pos, _scale);
    }

    private void _spawnBothHits(int teamId, NodeColors node1Colors, NodeColors node2Colors)
    {
        //add all potential positions
        List<float> availablePositions = new List<float>(_rowPositions);

        //0 to length - 1
        int rando = Random.Range(0, availablePositions.Count);

        //Pull a position out
        float y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit1
        _hitManager.SpawnHit(0, teamId, node1Colors, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        //0 to length-1
        rando = Random.Range(0, availablePositions.Count);

        y = availablePositions[rando];
        availablePositions.RemoveAt(rando);

        //spawn Hit2
        _hitManager.SpawnHit(1, teamId, node2Colors, new Vector3(GameManager.RightX + 5, y, 0), _scale);
        _survivalHandler.PotentialMaxScore += 20;

        availablePositions.Clear();
    }
}
