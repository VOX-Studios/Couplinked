using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScoreJuiceManager : MonoBehaviour 
{
	public GameObject ScoreJuicePrefab;

	public List<GameObject> activeScoreJuices;
	public List<GameObject> inactiveScoreJuices;
	private int maxScoreJuices = 10;

	private GameManager _gameManager;

	public void Initialize () 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

		activeScoreJuices = new List<GameObject>();
		inactiveScoreJuices = new List<GameObject>();

		Transform parent = GameObject.Find("Canvas").transform;

		for(int i = 0; i < maxScoreJuices; i++)
		{
			GameObject scoreJuice = (GameObject)Instantiate(ScoreJuicePrefab);
			scoreJuice.transform.parent = parent;
			scoreJuice.SetActive(false);
			inactiveScoreJuices.Add(scoreJuice);
		}
	}

	public void Run(float deltaTime) 
	{
		for(int i = activeScoreJuices.Count - 1; i >= 0; i--)
		{
			ScoreJuice scoreJuiceComponent = activeScoreJuices[i].GetComponent<ScoreJuice>();
			scoreJuiceComponent.Run(deltaTime);

			if(scoreJuiceComponent.timeRemaining <= 0)
			{
				deactivateScoreJuice(i);
			}
		}
	}

	public void SpawnScoreJuice(Vector3 pos, int score, Vector3 direction, float scale)
	{		
		//TODO: Handle colors and such?

		_activateScoreJuice(pos, score, direction, scale);
	}

	
	private void _activateScoreJuice(Vector3 spawnPosition, int score, Vector3 direction, float scale)
	{
		int inactiveCount = inactiveScoreJuices.Count;
		if(inactiveCount > 0)
		{
			GameObject scoreJuice = inactiveScoreJuices[inactiveCount - 1];
			scoreJuice.transform.localScale = new Vector3(scale, scale, 1);
			scoreJuice.GetComponent<ScoreJuice>().Setup(spawnPosition, direction);
			scoreJuice.GetComponent<Text>().text = score.ToString();
			scoreJuice.SetActive(true);

			inactiveScoreJuices.RemoveAt(inactiveCount - 1);
			activeScoreJuices.Add(scoreJuice);
		}
	}
	
	public void deactivateScoreJuice(int index)
	{
		GameObject scoreJuice = activeScoreJuices[index];
		scoreJuice.SetActive(false);
		activeScoreJuices.RemoveAt(index);
		inactiveScoreJuices.Add(scoreJuice);
	}
	
	public void deactivateScoreJuice(GameObject scoreJuice)
	{
		scoreJuice.SetActive(false);
		activeScoreJuices.Remove(scoreJuice);
		inactiveScoreJuices.Add(scoreJuice);
	}

}
