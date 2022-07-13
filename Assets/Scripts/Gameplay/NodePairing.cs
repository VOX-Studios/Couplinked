using System.Collections.Generic;

public class NodePairing
{
    public List<Node> Nodes;

    public List<LaserManager> LightningManagers;

    public NodePairing(List<Node> nodes, List<LaserManager> lightningManagers)
    {
        Nodes = nodes;
        LightningManagers = lightningManagers;
    }

    public void SetScale(float scale)
    {
        foreach (Node node in Nodes)
        {
            node.SetScale(scale);
        }

        foreach(LaserManager lightningManager in LightningManagers)
        {
            lightningManager.SetScale(scale);
        }
    }
}
