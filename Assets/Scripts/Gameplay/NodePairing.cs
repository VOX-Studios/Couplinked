using System.Collections.Generic;

public class NodePairing
{
    public List<Node> Nodes;

    public LightningManager LightningManager;

    public NodePairing(List<Node> nodes, LightningManager lightningManager)
    {
        Nodes = nodes;
        LightningManager = lightningManager;
    }

    public void SetScale(float scale)
    {
        foreach (Node node in Nodes)
        {
            node.SetScale(scale);
        }

        LightningManager.SetScale(scale);
    }
}
