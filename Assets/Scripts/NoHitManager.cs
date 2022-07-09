using UnityEngine;
using System.Collections.Generic;

public class NoHitManager : MonoBehaviour 
{
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

	public void Run(bool isPaused, float deltaTime) 
	{
		if(isPaused)
        {
			_runPaused();
			return;
		}

		for (int i = activeNoHits.Count - 1; i >= 0; i--)
		{
			NoHit noHit = activeNoHits[i];
			noHit.Move(deltaTime);
			_gameManager.Grid.ColorManager.SetLightPosition(noHit.LightIndex, noHit.transform.position);

			if (activeNoHits[i].transform.position.x < GameManager.LeftX - activeNoHits[i].GetComponent<Renderer>().bounds.extents.x)
			{
				DeactivateNoHit(i);
				continue;
			}
		}
	}

	private void _runPaused()
    {
		foreach(NoHit noHit in activeNoHits)
        {
			_gameManager.Grid.ColorManager.SetLightPosition(noHit.LightIndex, noHit.transform.position);
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
