using Assets.Scripts.SceneManagers;
using System.Collections.Generic;
using UnityEngine;

interface IGameModeHandler
{
    void Start();

    void Run(float deltaTime);

    void OnHitCollision(Hit hit, Collider2D other);

    void OnHitSplitCollision(HitSplit hitSplit, Collider2D other);

    void OnNoHitCollision(NoHit noHit, Collider2D other);
}
