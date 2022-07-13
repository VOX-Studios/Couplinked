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

    private SpawnPlanner _spawnPlanner;
    private NodePairing[] _nodePairings;

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
        _nodePairings = nodePairings;
        _rowPositions = rowPositions;
        _scale = scale;

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
                        nodePairings: _nodePairings
                        );
                    break;
                case SpawnRowTypeEnum.HitSplit:
                    _spawnHitSplit(
                        rowIndex: i,
                        firstNodeId: spawnPlanRow.Ids[0].NodeId,
                        firstTeamId: spawnPlanRow.Ids[0].TeamId,
                        secondNodeId: spawnPlanRow.Ids[1].NodeId,
                        secondTeamId: spawnPlanRow.Ids[1].TeamId,
                        nodePairings: _nodePairings
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

    private void _spawnHit(int rowIndex, int nodeId, int teamId, NodePairing[] nodePairings)
    {
        Color hitColor = nodePairings[teamId].Nodes[nodeId].OutsideColor;
        Vector3 pos = _getSpawnPosition(rowIndex);
        _hitManager.SpawnHit(nodeId, teamId, hitColor, pos, _scale);
        _survivalHandler.PotentialMaxScore += 10;
    }

    private void _spawnNoHit(int rowIndex)
    {
        Vector3 pos = _getSpawnPosition(rowIndex);
        _noHitManager.SpawnNoHit(pos, _scale);
    }

    private void _spawnHitSplit(int rowIndex, int firstNodeId, int firstTeamId, int secondNodeId, int secondTeamId, NodePairing[] nodePairings)
    {
        Vector3 pos = _getSpawnPosition(rowIndex);

        _hitSplitManager.SpawnHitSplit(
                   hitSplitFirstType: firstNodeId,
                   hitSplitSecondType: secondNodeId,
                   firstHitTeamId: firstTeamId,
                   secondHitTeamId: secondTeamId,
                   firstHitColor: nodePairings[firstTeamId].Nodes[firstNodeId].OutsideColor,
                   secondHitColor: nodePairings[secondTeamId].Nodes[secondNodeId].OutsideColor,
                   spawnPosition: pos,
                   scale: _scale
                   );

        _survivalHandler.PotentialMaxScore += 20;
    }
}
