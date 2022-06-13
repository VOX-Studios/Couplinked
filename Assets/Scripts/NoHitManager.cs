using UnityEngine;
using System.Collections.Generic;

public class NoHitManager : MonoBehaviour 
{
	float spawnInterval = 5f;
	float spawnTimer = 0f;
	public GameObject NoHitPrefab;

	public List<GameObject> activeNoHits;
	public List<GameObject> inactiveNoHits;

	private GameManager _gameManager;

	public float Scale { get; private set; }

	// Use this for initialization
	public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

		activeNoHits = new List<GameObject>();
		inactiveNoHits = new List<GameObject>();

		Transform parent = GameObject.Find("NoHitManager").transform;

		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject noHit = (GameObject)Instantiate(NoHitPrefab);
			noHit.transform.parent = parent;
			noHit.SetActive(false);
			inactiveNoHits.Add(noHit);
		}
	}

	public void SetScale(float scale)
	{
		Scale = scale;
	}

	public void Run(float deltaTime) 
	{
		spawnTimer -= deltaTime;

		if(spawnTimer <= 0)
		{

			spawnTimer = spawnInterval;
		}

		for(int i = activeNoHits.Count - 1; i >= 0; i--)
		{
			activeNoHits[i].GetComponent<NoHit>().Move(deltaTime);

			if(activeNoHits[i].transform.position.x < GameManager.LeftX - activeNoHits[i].gameObject.GetComponent<Renderer>().bounds.extents.x)
			{
				deactivateNoHit(i);
			}
		}
	}

	public void SpawnNoHit(Vector3 pos, float scale)
	{
		activateNoHit(pos, scale);
	}
	
	void activateNoHit(Vector3 spawnPosition, float scale)
	{
		int inactiveCount = inactiveNoHits.Count;
		if(inactiveCount > 0)
		{
			GameObject noHit = inactiveNoHits[inactiveCount - 1];
			NoHit noHitComponent = noHit.GetComponent<NoHit>();
			noHitComponent.SetScale(scale);
			noHit.transform.position = spawnPosition;

			noHit.SetActive(true);

			inactiveNoHits.RemoveAt(inactiveCount - 1);
			activeNoHits.Add(noHit);
		}
	}
	
	public void deactivateNoHit(int index)
	{
		GameObject noHit = activeNoHits[index];
		noHit.SetActive(false);
		activeNoHits.RemoveAt(index);
		inactiveNoHits.Add(noHit);
	}
	
	public void deactivateNoHit(GameObject noHit)
	{
		noHit.SetActive(false);
		activeNoHits.Remove(noHit);
		inactiveNoHits.Add(noHit);
	}

}
