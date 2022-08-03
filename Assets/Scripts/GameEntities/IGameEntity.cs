using Assets.Scripts.GameEntities;
using UnityEngine;

public interface IGameEntity
{
    GameEntityTypeEnum GameEntityType { get; }
    int LightIndex { get; set; }
    float Speed { get; set; }

    Transform Transform { get; }

    SoundEffectManager.PitchToPlay ExplosionPitch { get; }

    void Move(float deltaTime);
}
