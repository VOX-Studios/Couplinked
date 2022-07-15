using System.Collections.Generic;

public class NodePairing
{
    public List<Node> Nodes;

    public List<LaserManager> LaserManagers;

    public NodePairing(List<Node> nodes, List<LaserManager> laserManagers)
    {
        Nodes = nodes;
        LaserManagers = laserManagers;
    }

    public void SetScale(float scale)
    {
        foreach (Node node in Nodes)
        {
            node.SetScale(scale);
        }

        if(LaserManagers != null)
        {
            foreach (LaserManager laserManager in LaserManagers)
            {
                laserManager.SetScale(scale);
            }
        }
    }
}
