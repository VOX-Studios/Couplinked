using UnityEngine;

public interface ICollisionHandler<T>
{
    void OnCollision(T item, Collider2D other);
}