using System.Collections.Generic;
using System.Linq;
using UnityEngine;




namespace Assets.Scripts.Gameplay.Survival.SpawnPlans
{
    class SpawnPlanner
    {
        Dictionary<int, int> _teamLookup = new Dictionary<int, int>();
        Dictionary<int, int> _nodeLookup = new Dictionary<int, int>();

        private int _numRows;
        private NodePairing[] _nodePairings;

        private int _totalNodes;
        private List<int> _availableNodes = new List<int>();
        private List<int> _allNodes = new List<int>();
        public SpawnPlanner(int numRows, NodePairing[] nodePairings)
        {
            _numRows = numRows;
            _nodePairings = nodePairings;

            //build lookup info
            int nodeLookupId = 0;
            for (int teamIndex = 0; teamIndex < nodePairings.Length; teamIndex++)
            {
                NodePairing nodePairing = nodePairings[teamIndex];

                for (int nodeIndex = 0; nodeIndex < nodePairing.Nodes.Count; nodeIndex++)
                {
                    Node node = nodePairing.Nodes[nodeIndex];
                    _allNodes.Add(nodeLookupId);
                    _nodeLookup[nodeLookupId] = nodeIndex;
                    _teamLookup[nodeLookupId] = teamIndex;

                    nodeLookupId++;
                }
            }

            _totalNodes = _allNodes.Count;
            _makeAllAvailable();
        }

        private void _makeAllAvailable()
        {
            //shuffle
            for(int i = 0; i < _allNodes.Count; i++)
            {
                int swapWith = Random.Range(0, _allNodes.Count);

                int current = _allNodes[i];
                _allNodes[i] = _allNodes[swapWith];
                _allNodes[swapWith] = current;
            }

            //add all to list of available
            _availableNodes.AddRange(_allNodes);
        }

        private int _getNextAvailableNode()
        {
            if(_availableNodes.Count == 0)
            {
                _makeAllAvailable();
            }

            int lookupId = _availableNodes[0];
            _availableNodes.RemoveAt(0);

            return lookupId;
        }


        public SpawnPlan GetPlan()
        {
            SpawnPlan spawnPlan = new SpawnPlan(_numRows);

            //1-10
            int randomNoHits = Random.Range(1, 11);

            switch(randomNoHits)
            {
                
                case 1:
                    //10% chance to spawn a number of hits equal to half the number of nodes
                    _spawnHits(spawnPlan, Mathf.CeilToInt(_totalNodes / 2f));
                    break;
                case 2:
                case 3:
                    //20% chance to spawn a single hit for half the teams
                    _spawnHits(spawnPlan, (int)(_nodePairings.Length/ 2f));
                    break;
                case 4:
                case 5:
                    //20% chance to spawn enough hit splits hits to require 50% the number of nodes
                    _spawnHits(spawnPlan, Mathf.CeilToInt(_totalNodes / 2f), true);
                    break;
                default:
                    //50% chance to spawn 1/3rd the number of rows as NoHits
                    _spawnNoHits(spawnPlan, (int)(_numRows / 3f));
                    break;
            }

            return spawnPlan;

            //int hitsToSpawn = Random.Range(0, _totalNodes + 1);
            //int noHitsToSpawn;
            //switch (hitsToSpawn)
            //{
            //    case 0:
            //        //spawn NoHits only
            //        noHitsToSpawn = Random.Range(1, (int)(_numRows/3f));
            //        _spawnNoHits(spawnPlan, noHitsToSpawn);
            //        break;
            //    default:
            //        //spawn hits and nohits
            //        _spawnHits(spawnPlan, hitsToSpawn);

            //        return spawnPlan;

            //        int maxNoHitsToSpawn = spawnPlan.Rows.Count(row => row.SpawnableType == SpawnRowTypeEnum.Empty);
            //        noHitsToSpawn = Random.Range(0, maxNoHitsToSpawn + 1);
            //        _spawnNoHits(spawnPlan, noHitsToSpawn);
            //        break;
            //}

            //return spawnPlan;
        }

        private void _spawnHits(SpawnPlan spawnPlan, int hitsToSpawn, bool shouldSpawnHitSplits = false)
        {
            //make sure we're not attempting to spawn more nodes than we'd be able to (at least within our "ruleset")
            if(hitsToSpawn > _totalNodes)
            {
                hitsToSpawn = _totalNodes;
            }    

            HashSet<int> spawnedNodes = new HashSet<int>();

            //create array to represent where we can spawn
            List<int> possibleSpawnIndexes = new List<int>();
            for (int i = 0; i < spawnPlan.Rows.Length; i++)
            {
                possibleSpawnIndexes.Add(i);
            }

            //TODO: prevent spawning multiple of the same hits in a single column
            while (possibleSpawnIndexes.Count > 0 && hitsToSpawn > 0)
            {
                //get IDs
                int lookupId = _getNextAvailableNode();

                while(spawnedNodes.Contains(lookupId))
                {
                    _availableNodes.Add(lookupId);
                    lookupId = _getNextAvailableNode();
                }

                int nodeId = _nodeLookup[lookupId];
                int teamId = _teamLookup[lookupId];
 
                //get a random index
                int randomIndex = Random.Range(0, possibleSpawnIndexes.Count);
                int spawnIndex = possibleSpawnIndexes[randomIndex];

                spawnPlan.Rows[spawnIndex].SpawnableType = SpawnRowTypeEnum.Hit;
                spawnPlan.Rows[spawnIndex].Ids.Add(new SpawnableIds()
                {
                    NodeId = nodeId,
                    TeamId = teamId
                });

                spawnedNodes.Add(lookupId);
                possibleSpawnIndexes.RemoveAt(randomIndex);
                hitsToSpawn--;

                if (hitsToSpawn == 0 || !shouldSpawnHitSplits)
                {
                    continue;
                }

                //change the type to a hit split
                spawnPlan.Rows[spawnIndex].SpawnableType = SpawnRowTypeEnum.HitSplit;

                //get IDs for second hit
                int secondLookupId = _getNextAvailableNode();

                while (spawnedNodes.Contains(secondLookupId))
                {
                    _availableNodes.Add(secondLookupId);
                    secondLookupId = _getNextAvailableNode();
                }

                int secondTeamId = _teamLookup[secondLookupId];
                int secondNodeId = _nodeLookup[secondLookupId];

                //add IDs for second hit
                spawnPlan.Rows[spawnIndex].Ids.Add(new SpawnableIds()
                {
                    TeamId = secondTeamId,
                    NodeId = secondNodeId
                });

                spawnedNodes.Add(secondLookupId);
                hitsToSpawn--;
            }
        }

        private void _spawnNoHits(SpawnPlan spawnPlan, int maxNumberToSpawn)
        {
            if (maxNumberToSpawn == 0)
            {
                return;
            }

            //create array to mark where we can spawn
            bool[] possibleSpawns = new bool[spawnPlan.Rows.Length];

            //go through every row and mark the empty spaces as possible spawns
            for (int i = 0; i < spawnPlan.Rows.Length; i++)
            {
                SpawnPlanRow row = spawnPlan.Rows[i];

                if (row.SpawnableType == SpawnRowTypeEnum.Empty)
                {
                    possibleSpawns[i] = true;
                }
            }

            //go through every row
            for (int i = 0; i < spawnPlan.Rows.Length; i++)
            {
                SpawnPlanRow row = spawnPlan.Rows[i];

                //we only care about hits
                if (row.SpawnableType != SpawnRowTypeEnum.Hit && row.SpawnableType != SpawnRowTypeEnum.HitSplit)
                {
                    continue;
                }

                //search for a node on the same team
                for (int j = i + 1; j < spawnPlan.Rows.Length; j++)
                {
                    SpawnPlanRow searchRow = spawnPlan.Rows[j];

                    //we only care about hits
                    if (searchRow.SpawnableType != SpawnRowTypeEnum.Hit && searchRow.SpawnableType != SpawnRowTypeEnum.HitSplit)
                    {
                        continue;
                    }

                    //if we find a node on the same team
                    if (searchRow.Ids.Any(searchId => row.Ids.Any(rowId => rowId.TeamId == searchId.TeamId)))
                    {
                        //everything between the two is not a possible spawn
                        for (int k = i + 1; k < j; k++)
                        {
                            possibleSpawns[k] = false;
                        }

                        //break out of the search loop
                        break;
                    }
                }
            }

            //get the indexes where we can spawn
            List<int> possibleSpawnIndexes = new List<int>();
            for (int i = 0; i < possibleSpawns.Length; i++)
            {
                if (possibleSpawns[i])
                {
                    possibleSpawnIndexes.Add(i);
                }
            }

            //spawn until we run out of spaces or we reach our max
            while(possibleSpawnIndexes.Count > 0 && maxNumberToSpawn > 0)
            {
                //get a random index
                int randomIndex = Random.Range(0, possibleSpawnIndexes.Count);
                int spawnIndex = possibleSpawnIndexes[randomIndex];

                spawnPlan.Rows[spawnIndex].SpawnableType = SpawnRowTypeEnum.NoHit;
                possibleSpawnIndexes.RemoveAt(randomIndex);
                maxNumberToSpawn--;
            }

        }

    }
}
