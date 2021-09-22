public class PlayerColorData
{
	public readonly ColorData Node1InsideColor;
	public readonly ColorData Node1OutsideColor;
	public readonly ColorData Node1ParticlesColor;
	public readonly ColorData Node2InsideColor;
	public readonly ColorData Node2OutsideColor;
	public readonly ColorData Node2ParticlesColor;
	public readonly ColorData LightningColor;
	public readonly ColorData GridColor;

	public PlayerColorData(int playerIndex)
    {
		Node1InsideColor = new ColorData($"P{playerIndex} Node 1 Inside Color");
		Node1OutsideColor = new ColorData($"P{playerIndex} Node 1 Outside Color");
		Node1ParticlesColor = new ColorData($"P{playerIndex} Node 1 Particles Color");
		Node2InsideColor = new ColorData($"P{playerIndex} Node 2 Inside Color");
		Node2OutsideColor = new ColorData($"P{playerIndex} Node 2 Outside Color");
		Node2ParticlesColor = new ColorData($"P{playerIndex} Node 2 Particles Color");
		LightningColor = new ColorData($"P{playerIndex} Lightning Color");
		GridColor = new ColorData($"P{playerIndex} Grid Color");
	}
}