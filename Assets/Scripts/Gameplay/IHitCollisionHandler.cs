using UnityEngine;

public interface IHitCollisionHandler
{
    void OnHitCollision(Hit hit, Collider2D other);
}