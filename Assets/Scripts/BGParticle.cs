using UnityEngine;

public class BGParticle : MonoBehaviour 
{
	private GameManager _gameManager;
	private float _speed = 5;

    void Awake()
    {
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	void Start()
    {
		_speed = _gameManager.GameDifficultyManager.BackgroundParticleSpeed;
	}

    // Update is called once per frame
    public void UpdateParticle(float deltaTime) 
	{
		transform.position -= new Vector3(deltaTime * _speed, 0, 0);
		transform.LookAt(new Vector3(0,0,75));
	}
}
