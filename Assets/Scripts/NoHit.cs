using Assets.Scripts.SceneManagers;
using UnityEngine;

public class NoHit : BaseObject
{
    private GameSceneManager _gameSceneManager;

	public float Scale { get; private set; }

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
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_gameSceneManager.OnNoHitCollision(this, other);
	}
}
