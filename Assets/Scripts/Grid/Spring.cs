using UnityEngine;

struct Spring
{
    public IPointMass End1;
    public IPointMass End2;
    public float Stiffness;
    public float Damping;

    public Spring(IPointMass end1, IPointMass end2, float stiffness, float damping)
    {
        End1 = end1;
        End2 = end2;
        Stiffness = stiffness;
        Damping = damping;
    }

    public void Update()
    {
        Vector2 deltaPosition = End1.Position - End2.Position;
        Vector2 deltaVelocity = End2.Velocity - End1.Velocity;
        Vector2 force = (Stiffness * deltaPosition) - (deltaVelocity * Damping);

        End1.ApplyForce(-force);
        End2.ApplyForce(force);
    }
}
