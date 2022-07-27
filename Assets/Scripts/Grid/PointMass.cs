using UnityEngine;

public class PointMass
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float InverseMass;

    private Vector3 _acceleration;
    private float damping = 0.95f;

    public PointMass(Vector3 position, float invMass)
    {
        Position = position;
        InverseMass = invMass;
    }

    public void ApplyForce(Vector3 force)
    {
        _acceleration += force * InverseMass;
    }

    public void Update()
    {
        Velocity += _acceleration;
        Position += Velocity;
        _acceleration = Vector3.zero;

        if (Velocity.sqrMagnitude < 0.001f * 0.001f) // float at worst has 6 digits of precision
        {
            Velocity = Vector3.zero;
            return;
        }

        Velocity *= damping;
    }
}
