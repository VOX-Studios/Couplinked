using UnityEngine;

struct Spring
{
    public PointMass End1;
    public PointMass End2;
    public float Stiffness;
    public float Damping;

    public Spring(PointMass end1, PointMass end2, float stiffness, float damping)
    {
        End1 = end1;
        End2 = end2;
        Stiffness = stiffness;
        Damping = damping;
    }

    public void Update()
    {
        Vector3 deltaPosition = End1.Position - End2.Position;
        Vector3 deltaVelocity = End2.Velocity - End1.Velocity;
        Vector3 force = (Stiffness * deltaPosition) - (deltaVelocity * Damping);

        End1.ApplyForce(-force);
        End2.ApplyForce(force);
    }
}
