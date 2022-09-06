using UnityEngine;

public class PointMass : IPointMass
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    private float damping = 0.95f;

    public PointMass(Vector2 position)
    {
        Position = position;
    }

    public void ApplyForce(Vector2 force)
    {
        Velocity += force;
    }

    public void Update()
    {
        Position += Velocity;

        if (Velocity.sqrMagnitude < 0.001f * 0.001f) // float at worst has 6 digits of precision
        {
            Velocity = Vector2.zero;
            return;
        }

        Velocity *= damping;
    }
}
