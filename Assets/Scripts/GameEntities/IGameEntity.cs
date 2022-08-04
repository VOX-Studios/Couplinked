using Assets.Scripts.GameEntities;
using UnityEngine;

public interface IGameEntity
{
    GameEntityTypeEnum GameEntityType { get; }
    int LightIndex { get; set; }
    float Speed { get; set; }
    bool IsOffScreenLeft { get; set; }
    float Radius { get; }

    Transform Transform { get; }

    SoundEffectManager.PitchToPlay ExplosionPitch { get; }

    void Move(float deltaTime);
}
