using System.Linq;

public class PlayerColors
{
    public NodeColors[] NodeColors { get; private set; }

    public PlayerColors(DefaultPlayerColors defaultPlayerColors)
    {
        NodeColors = defaultPlayerColors.NodeColors.Select(nodeColors => new NodeColors()
        {
            InsideColor = nodeColors.InsideColor,
            OutsideColor = nodeColors.OutsideColor,
            ParticleColor = nodeColors.ParticleColor,
        }).ToArray();
    }

    public PlayerColors(CustomPlayerColorData customPlayerColorData)
    {
        NodeColors = customPlayerColorData.NodeColors.Select(nodeColors => new NodeColors()
        {
            InsideColor = nodeColors.InsideColor.Get(),
            OutsideColor = nodeColors.OutsideColor.Get(),
            ParticleColor = nodeColors.ParticleColor.Get(),
        }).ToArray();
    }
}