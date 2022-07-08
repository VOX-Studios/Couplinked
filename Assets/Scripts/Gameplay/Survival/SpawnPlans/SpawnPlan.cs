namespace Assets.Scripts.Gameplay.Survival.SpawnPlans
{
    class SpawnPlan
    {
        public SpawnPlanRow[] Rows { get; private set; }
        public SpawnPlan(int numRows)
        {
            Rows = new SpawnPlanRow[numRows];

            for(int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new SpawnPlanRow();
            }
        }
    }
}
