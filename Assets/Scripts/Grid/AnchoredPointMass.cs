using UnityEngine;

public class AnchoredPointMass : IPointMass
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public AnchoredPointMass(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.zero;
    }

    public void ApplyForce(Vector2 force)
    {

    }

    public void Update()
    {
    
    }
}
