public class NodeColorData
{
	public readonly ColorData InsideColor;
	public readonly ColorData OutsideColor;
	public readonly ColorData ParticleColor;

	public NodeColorData(int playerIndex, int nodeIndex)
    {
		InsideColor = new ColorData($"P{playerIndex} Node {nodeIndex} Inside Color");
		OutsideColor = new ColorData($"P{playerIndex} Node {nodeIndex} Outside Color");
		ParticleColor = new ColorData($"P{playerIndex} Node {nodeIndex} Particles Color");
	}
}