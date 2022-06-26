using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitManager : MonoBehaviour 
{
	float spawnInterval = 5f;
	float spawnTimer = 0f;
	public GameObject HitPrefab;

	public List<GameObject> activeHits;
	public List<GameObject> inactiveHits;

	private GameManager _gameManager;

    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

		activeHits = new List<GameObject>();
		inactiveHits = new List<GameObject>();

		//TODO: move them to be under bolts
		Transform parent = GameObject.Find("HitManager").transform;
		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject hit = (GameObject)Instantiate(HitPrefab);
			Hit hitComponent = hit.GetComponent<Hit>();
			hitComponent.Initialize(_gameSceneManager);

			hit.transform.parent = parent;
			hit.SetActive(false);
			inactiveHits.Add(hit);
		}
	}
	
	public void Run(float time) 
	{
		spawnTimer -= time;
		
		if(spawnTimer <= 0)
		{
			spawnTimer = spawnInterval;
		}

		for(int i = activeHits.Count - 1; i >= 0; i--)
		{
			activeHits[i].GetComponent<Hit>().Move(time);

			if(activeHits[i].transform.position.x < GameManager.LeftX - activeHits[i].gameObject.GetComponent<Renderer>().bounds.extents.x)
			{
				DeactivateHit(i);

				if(_gameSceneManager.EndGame(ReasonForGameEndEnum.HitOffScreen))
                    break;
			}

		}
	}
	public void SpawnHit(HitTypeEnum hitType, int teamId, PlayerColors playerColors, Vector3 position, float scale)
	{
		_activateHit(hitType, teamId, playerColors, position, scale, _gameManager.SoundEffectManager.NextPitch());
	}

	private void _activateHit(
		HitTypeEnum hitType, 
		int teamId,
		PlayerColors playerColors,  
		Vector3 spawnPosition,
		float scale,
		SoundEffectManager.PitchToPlay explosionPitch
		)
	{
		int inactiveCount = inactiveHits.Count;
		if(inactiveCount > 0)
		{
			GameObject hit = inactiveHits[inactiveCount - 1];
			Hit hitComponent = hit.GetComponent<Hit>();
			hitComponent.SetScale(scale);
			hit.transform.position = spawnPosition;

			hitComponent.SetColor(playerColors.NodeColors[(int)hitType].OutsideColor);

			hitComponent.TeamId = teamId;
			hitComponent.HitType = hitType;
			hitComponent.ExplosionPitch = explosionPitch;

			hit.SetActive(true);
			inactiveHits.RemoveAt(inactiveCount - 1);
			activeHits.Add(hit);
		}
	}
	
	public void DeactivateHit(int index)
	{
		GameObject hit = activeHits[index];
		hit.SetActive(false);
		activeHits.RemoveAt(index);
		inactiveHits.Add(hit);
	}

	public void DeactivateHit(GameObject hit)
	{
		hit.SetActive(false);
		activeHits.Remove(hit);
		inactiveHits.Add(hit);
	}

}
