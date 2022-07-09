using Assets.Scripts.SceneManagers;
using System;
using UnityEngine;

public class NoHit : BaseObject
{
    private GameSceneManager _gameSceneManager;

	public float Scale { get; private set; }

	[NonSerialized]
	public int LightIndex = -1;

    // Use this for initialization
    void Start () 
	{
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();
		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}

	public void SetScale(float scale)
    {
		Scale = scale;
		transform.localScale = new Vector3(scale, scale, 1);
	}

	public void Move(float time) 
	{
		transform.position -= new Vector3(Speed * Scale, 0, 0) * time;
		//_GameManager.Grid.Logic.ApplyImplosiveForce(1 * Scale, transform.position, 1 * Scale);
	}

	public void ReleaseLightIndex()
    {
		_GameManager.Grid.ColorManager.ReleaseLightIndex(LightIndex);
		LightIndex = -1;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_gameSceneManager.OnNoHitCollision(this, other);
	}
}
