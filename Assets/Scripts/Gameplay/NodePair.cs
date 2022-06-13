using UnityEngine;

public class NodePair
{
    public Node Node1;

    public Node Node2;

    public LightningManager LightningManager;

    public NodePair(Node node1, Node node2, LightningManager lightningManager)
    {
        Node1 = node1;
        Node2 = node2;
        LightningManager = lightningManager;
    }

    public void SetScale(float scale)
    {
        Node1.SetScale(scale);
        Node2.SetScale(scale);

        LightningManager.SetScale(scale);
    }
}
