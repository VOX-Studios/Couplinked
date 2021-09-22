using System;
using UnityEngine;

public class BaseObject : MonoBehaviour 
{
    [NonSerialized]
	public float Speed = 7;

    [NonSerialized]
    public SoundEffectManager.PitchToPlay ExplosionPitch;

    protected GameManager _GameManager;

    void Awake()
    {
        _GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}
