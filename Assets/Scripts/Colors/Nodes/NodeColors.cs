using UnityEngine;

public class NodeColors
{
    public Color InsideColor { get; private set; }
    public Color OutsideColor { get; private set; }
    public Color ParticleColor { get; private set; }

    public NodeColors(DefaultNodeColors defaultNodeColors)
    {
        InsideColor = defaultNodeColors.InsideColor;
        OutsideColor = defaultNodeColors.OutsideColor;
        ParticleColor = defaultNodeColors.ParticleColor;
    }

    public NodeColors(NodeColorData nodeColorData)
    {
        InsideColor = nodeColorData.InsideColor.Get();
        OutsideColor = nodeColorData.OutsideColor.Get();
        ParticleColor = nodeColorData.ParticleColor.Get();
    }
}