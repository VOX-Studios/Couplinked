namespace Assets.Scripts.Gameplay
{
    static class ScaleUtility
    {
        public static float CalculateScale(int numRows)
        {
            if (numRows <= 3)
            {
                return 1f;
            }

            return 3f / numRows;
        }
    }
}
