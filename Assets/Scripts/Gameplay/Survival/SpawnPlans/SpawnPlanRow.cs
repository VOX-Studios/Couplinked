using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Survival.SpawnPlans
{
    class SpawnPlanRow
    {
        public SpawnRowTypeEnum SpawnableType;
        public List<SpawnableIds> Ids;

        public SpawnPlanRow()
        {
            Ids = new List<SpawnableIds>();
        }
    }
}
