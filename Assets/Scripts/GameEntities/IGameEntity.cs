using UnityEngine;

public interface IGameEntity
{
    int LightIndex { get; set; }
    float Speed { get; set; }

    Transform Transform { get; }

    SoundEffectManager.PitchToPlay ExplosionPitch { get; }

    void Move(float deltaTime);
}
