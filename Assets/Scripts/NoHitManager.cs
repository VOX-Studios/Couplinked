using UnityEngine;
using System.Collections.Generic;

public class NoHitManager : MonoBehaviour 
{
	float spawnInterval = 5f;
	float spawnTimer = 0f;
	public GameObject NoHitPrefab;

	public List<NoHit> activeNoHits;
	public List<NoHit> inactiveNoHits;

	private GameManager _gameManager;

	public float Scale { get; private set; }

	// Use this for initialization
	public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

		activeNoHits = new List<NoHit>();
		inactiveNoHits = new List<NoHit>();

		Transform parent = GameObject.Find("NoHitManager").transform;

		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject noHit = (GameObject)Instantiate(NoHitPrefab);
			noHit.transform.parent = parent;
			noHit.SetActive(false);
			inactiveNoHits.Add(noHit.GetComponent<NoHit>());
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
			activeNoHits[i].Move(deltaTime);

			if(activeNoHits[i].transform.position.x < GameManager.LeftX - activeNoHits[i].GetComponent<Renderer>().bounds.extents.x)
			{
				DeactivateNoHit(i);
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
			NoHit noHit = inactiveNoHits[inactiveCount - 1];
			noHit.SetScale(scale);
			noHit.transform.position = spawnPosition;

			noHit.LightIndex = _gameManager.Grid.ColorManager.GetLightIndex();
			_gameManager.Grid.ColorManager.SetLightColor(noHit.LightIndex, Color.red);
			_gameManager.Grid.ColorManager.SetLightPosition(noHit.LightIndex, spawnPosition);
			noHit.gameObject.SetActive(true);

			inactiveNoHits.RemoveAt(inactiveCount - 1);
			activeNoHits.Add(noHit);
		}
	}
	
	public void DeactivateNoHit(int index)
	{
		NoHit noHit = activeNoHits[index];
		noHit.ReleaseLightIndex();
		noHit.gameObject.SetActive(false);
		activeNoHits.RemoveAt(index);
		inactiveNoHits.Add(noHit);
	}
	
	public void deactivateNoHit(NoHit noHit)
	{
		noHit.ReleaseLightIndex();
		noHit.gameObject.SetActive(false);
		activeNoHits.Remove(noHit);
		inactiveNoHits.Add(noHit);
	}
}
