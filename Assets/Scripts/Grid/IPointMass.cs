using UnityEngine;

public interface IPointMass
{
    Vector2 Position { get; set; }
    Vector2 Velocity { get; set; }

    void ApplyForce(Vector2 force);

    public void Update();
}
