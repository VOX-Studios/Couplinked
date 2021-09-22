using Assets.Scripts.SceneManagers;
using UnityEngine;

public class NoHit : BaseObject
{
    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    void Start () 
	{
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();
		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}

	public void Move(float time) 
	{
		transform.position -= new Vector3(Speed, 0, 0) * time;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_gameSceneManager.OnNoHitCollision(this, other);
	}
}
