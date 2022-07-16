using UnityEngine;

interface IGameModeHandler
{
    void Initialize();

    void Run(bool isPaused, float deltaTime);

    void OnHitCollision(Hit hit, Collider2D other);

    void OnHitSplitCollision(HitSplit hitSplit, Collider2D other);

    void OnNoHitCollision(NoHit noHit, Collider2D other);
}
