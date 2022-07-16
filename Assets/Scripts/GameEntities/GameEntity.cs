using UnityEngine;

public abstract class GameEntity : MonoBehaviour, IGameEntity
{
    public int LightIndex { get; set; } = -1;
    public float Speed { get; set; } = 7;

    public SoundEffectManager.PitchToPlay ExplosionPitch { get; set; }

    protected GameManager _GameManager;

    public Transform Transform => transform;

    void Awake()
    {
        _GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public abstract void Move(float deltaTime);
}
