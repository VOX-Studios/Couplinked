using Assets.Scripts.Gameplay.Survival.SpawnPlans;
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

    private float[] _rowPositions;
    private float _scale;

    private PlayerColors[] _playerColors;
    private SpawnPlanner _spawnPlanner;

    public SurvivalCoOpSpawnHandler(
        SurvivalHandler survivalHandler,
        GameManager gameManager, 
        HitManager hitManager, 
        HitSplitManager hitSplitManager, 
        NoHitManager noHitManager,
        NodePairing[] nodePairings,
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

        _playerColors = _gameManager.ColorManager.DefaultPlayerColors.Select(defaultPlayerColors => new PlayerColors(defaultPlayerColors)).ToArray();

        _spawnPlanner = new SpawnPlanner(_rowPositions.Length, nodePairings);
    }

    public void Spawn()
    {
        SpawnPlan spawnPlan = _spawnPlanner.GetPlan();

        for(int i = 0; i < spawnPlan.Rows.Length; i++)
        {
            SpawnPlanRow spawnPlanRow = spawnPlan.Rows[i];
            switch (spawnPlanRow.SpawnableType)
            {
                case SpawnRowTypeEnum.Hit:
                    _spawnHit(
                        rowIndex: i,
                        nodeId: spawnPlanRow.Ids[0].NodeId,
                        teamId: spawnPlanRow.Ids[0].TeamId,
                        playerColors: _playerColors
                        );
                    break;
                case SpawnRowTypeEnum.HitSplit:
                    _spawnHitSplit(
                        rowIndex: i,
                        firstNodeId: spawnPlanRow.Ids[0].NodeId,
                        firstTeamId: spawnPlanRow.Ids[0].TeamId,
                        secondNodeId: spawnPlanRow.Ids[1].NodeId,
                        secondTeamId: spawnPlanRow.Ids[1].TeamId,
                        playerColors: _playerColors
                        );
                    break;
                case SpawnRowTypeEnum.NoHit:
                    _spawnNoHit(i);
                    break;
            }
        }
    }

    private Vector3 _getSpawnPosition(int rowIndex)
    {
        return new Vector3(GameManager.RightX + 5, _rowPositions[rowIndex], 0);
    }

    private void _spawnHit(int rowIndex, int nodeId, int teamId, PlayerColors[] playerColors)
    {
        NodeColors nodeColors = playerColors[teamId].NodeColors[nodeId];
        Vector3 pos = _getSpawnPosition(rowIndex);
        _hitManager.SpawnHit(nodeId, teamId, nodeColors, pos, _scale);
        _survivalHandler.PotentialMaxScore += 10;
    }

    private void _spawnNoHit(int rowIndex)
    {
        Vector3 pos = _getSpawnPosition(rowIndex);
        _noHitManager.SpawnNoHit(pos, _scale);
    }

    private void _spawnHitSplit(int rowIndex, int firstNodeId, int firstTeamId, int secondNodeId, int secondTeamId, PlayerColors[] playerColors)
    {
        Vector3 pos = _getSpawnPosition(rowIndex);

        _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: firstNodeId,
                   hitSplitSecondType: secondNodeId,
                   firstHitTeamId: firstTeamId,
                   secondHitTeamId: secondTeamId,
                   firstHitNodeColors: playerColors[firstTeamId].NodeColors[firstNodeId],
                   secondHitNodeColors: playerColors[secondTeamId].NodeColors[secondNodeId],
                   spawnPosition: pos,
                   scale: _scale
                   );

        _survivalHandler.PotentialMaxScore += 20;
    }
}
