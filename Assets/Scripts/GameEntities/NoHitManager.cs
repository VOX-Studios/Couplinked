using UnityEngine;
using System.Collections.Generic;

public class NoHitManager : MonoBehaviour, IGameEntityManager<NoHit>
{
	public GameObject NoHitPrefab;

	public List<NoHit> ActiveGameEntities { get; private set; }
	private List<NoHit> _inactiveNoHits;

	private GameManager _gameManager;

	public float Scale { get; private set; }

	// Use this for initialization
	public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

		ActiveGameEntities = new List<NoHit>();
		_inactiveNoHits = new List<NoHit>();

		Transform parent = GameObject.Find("NoHitManager").transform;

		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject noHit = (GameObject)Instantiate(NoHitPrefab);
			noHit.transform.parent = parent;
			noHit.SetActive(false);
			_inactiveNoHits.Add(noHit.GetComponent<NoHit>());
		}
	}

	public void SpawnNoHit(Vector3 pos, float scale)
	{
		activateNoHit(pos, scale);
	}
	
	void activateNoHit(Vector3 spawnPosition, float scale)
	{
		int inactiveCount = _inactiveNoHits.Count;
		if(inactiveCount > 0)
		{
			NoHit noHit = _inactiveNoHits[inactiveCount - 1];
			noHit.SetScale(scale);
			noHit.transform.position = spawnPosition;

			noHit.LightIndex = _gameManager.LightingManager.GetLightIndex();
			_gameManager.LightingManager.SetLightColor(noHit.LightIndex, Color.red);
			_gameManager.LightingManager.SetLightPosition(noHit.LightIndex, spawnPosition);
			noHit.gameObject.SetActive(true);

			_inactiveNoHits.RemoveAt(inactiveCount - 1);
			ActiveGameEntities.Add(noHit);
		}
	}
	
	public void DeactivateGameEntity(int index)
	{
		NoHit noHit = ActiveGameEntities[index];
		noHit.ReleaseLightIndex();
		noHit.gameObject.SetActive(false);
		ActiveGameEntities.RemoveAt(index);
		_inactiveNoHits.Add(noHit);
	}
	
	public void DeactivateGameEntity(NoHit noHit)
	{
		noHit.ReleaseLightIndex();
		noHit.gameObject.SetActive(false);
		ActiveGameEntities.Remove(noHit);
		_inactiveNoHits.Add(noHit);
	}
}
