public class CustomPlayerColorData
{
	public readonly NodeColorData[] NodeColors;
	public readonly ColorData GridColor;

	public CustomPlayerColorData(int playerIndex, int numberOfNodeColors)
    {
		NodeColors = new NodeColorData[numberOfNodeColors];

		for(int i = 0; i < NodeColors.Length; i++)
        {
			NodeColors[i] = new NodeColorData(playerIndex, i);
        }

		GridColor = new ColorData($"P{playerIndex} Grid Color");
	}
}