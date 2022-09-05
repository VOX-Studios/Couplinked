using Assets.Scripts.SceneManagers;
using UnityEngine;
using UnityEngine.UI;

public class ScoreJuice : MonoBehaviour 
{
	private GameManager _gameManager;
	private GameSceneManager _gameSceneManager;
    private const int _speed = 1500; //TODO: make this dependent on screen size?
	public float timeRemaining = 0f;
	float lifeTime = 1.5f;

	Vector3 originalPosition;

	Vector3 direction;

	public void Initialize(GameManager gameManager, GameSceneManager gameSceneManager)
	{
		_gameManager = gameManager;
		_gameSceneManager = gameSceneManager;
	}

	public void Setup(Vector3 spawnPositionWorldSpace, Vector3 dir)
	{
		timeRemaining = lifeTime;
		GetComponent<Text>().color = Color.white;

		direction = dir;

		transform.position = _gameManager.Cam.WorldToScreenPoint(spawnPositionWorldSpace);
		originalPosition = transform.position;
	}

	public void Run(float deltaTime) 
	{
		timeRemaining -= deltaTime;

        originalPosition += _speed * direction * deltaTime * Mathf.Pow(timeRemaining / lifeTime, 5);

        //Set position to original position + camera shake
        transform.position = originalPosition 
            + _gameManager.Cam.WorldToScreenPoint(_gameSceneManager.GetCamOriginalPos()) 
            - _gameManager.Cam.WorldToScreenPoint(_gameManager.Cam.transform.position);

		ClampGUI(transform, 0.5f);

		GetComponent<Text>().color = new Color(GetComponent<Text>().color.r, GetComponent<Text>().color.g, GetComponent<Text>().color.b, timeRemaining / lifeTime);
	}

	void ClampGUI(Transform t, float clampBorderOffset)
	{
		if(_gameManager.Cam.ScreenToWorldPoint(t.position).y > GameManager.TopY - clampBorderOffset)
		{
			t.position = new Vector3(
                t.position.x, 
			    _gameManager.Cam.WorldToScreenPoint(
                    new Vector3(
                        0, 
                        GameManager.TopY - clampBorderOffset,
                        0
                    )
                ).y, 
			    t.position.z
                );

			direction.y = -Mathf.Abs(direction.y);
		}
		else if(_gameManager.Cam.ScreenToWorldPoint(t.position).y < GameManager.BotY + clampBorderOffset)
		{
			t.position = new Vector3(t.position.x, 
			                         _gameManager.Cam.WorldToScreenPoint(new Vector3(0,GameManager.BotY + clampBorderOffset,0)).y, 
			                         t.position.z);

			direction.y = Mathf.Abs(direction.y);
		}

		if(_gameManager.Cam.ScreenToWorldPoint(t.position).x > GameManager.RightX - clampBorderOffset)
		{
			t.position = new Vector3(_gameManager.Cam.WorldToScreenPoint(new Vector3(GameManager.RightX - clampBorderOffset,0,0)).x, 
			                         t.position.y, 
			                         t.position.z);

			direction.x = -Mathf.Abs(direction.x);
		}
		else if(_gameManager.Cam.ScreenToWorldPoint(t.position).x < GameManager.LeftX + clampBorderOffset)
		{
			t.position = new Vector3(_gameManager.Cam.WorldToScreenPoint(new Vector3(GameManager.LeftX + clampBorderOffset,0,0)).x, 
			                         t.position.y, 
			                         t.position.z);

			direction.x = Mathf.Abs(direction.x);
		}

	}
}
