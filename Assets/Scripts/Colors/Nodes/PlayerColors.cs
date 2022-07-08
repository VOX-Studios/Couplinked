using System.Linq;

public class PlayerColors
{
    public NodeColors[] NodeColors { get; private set; }

    public PlayerColors(DefaultPlayerColors defaultPlayerColors)
    {
        NodeColors = defaultPlayerColors.NodeColors.Select(defaultNodeColors => new NodeColors(defaultNodeColors))
            .ToArray();
    }

    public PlayerColors(CustomPlayerColorData customPlayerColorData)
    {
        NodeColors = customPlayerColorData.NodeColors.Select(nodeColorData => new NodeColors(nodeColorData))
            .ToArray();
    }
}