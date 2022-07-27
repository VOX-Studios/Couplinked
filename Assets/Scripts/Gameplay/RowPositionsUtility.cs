namespace Assets.Scripts.Gameplay
{
    static class RowPositionsUtility
    {
        public static float[] CalculateRowPositions(int numRows)
        {
            float[] rowPositions = new float[numRows];

            float borderPadding = 2.5f; //TODO: share this with clamp
            rowPositions[0] = GameManager.TopY - borderPadding;
            rowPositions[rowPositions.Length - 1] = GameManager.BotY + borderPadding;

            //take top and bot and divide by remaining positions
            float spacing = (rowPositions[0] - rowPositions[rowPositions.Length - 1]) / (rowPositions.Length - 1);

            for (int i = 1; i < rowPositions.Length - 1; i++)
            {
                rowPositions[i] = rowPositions[0] - (spacing * i);
            }

            return rowPositions;
        }
    }
}
