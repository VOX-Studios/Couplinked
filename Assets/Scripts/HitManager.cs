using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitManager : MonoBehaviour 
{
	float spawnInterval = 5f;
	float spawnTimer = 0f;
	public GameObject HitPrefab;

	public List<Hit> activeHits;
	public List<Hit> inactiveHits;

	private GameManager _gameManager;

    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

		activeHits = new List<Hit>();
		inactiveHits = new List<Hit>();

		//TODO: move them to be under bolts
		Transform parent = GameObject.Find("HitManager").transform;
		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject hit = (GameObject)Instantiate(HitPrefab);
			Hit hitComponent = hit.GetComponent<Hit>();
			hitComponent.Initialize(_gameSceneManager);

			hit.transform.parent = parent;
			hit.SetActive(false);
			inactiveHits.Add(hitComponent);
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
			activeHits[i].Move(time);

			if(activeHits[i].transform.position.x < GameManager.LeftX - activeHits[i].gameObject.GetComponent<Renderer>().bounds.extents.x)
			{
				DeactivateHit(i);

				if (_gameSceneManager.EndGame(ReasonForGameEndEnum.HitOffScreen))
				{
					break;
				}
			}

		}
	}
	public void SpawnHit(int nodeId, int teamId, NodeColors nodeColors, Vector3 position, float scale)
	{
		_activateHit(nodeId, teamId, nodeColors, position, scale, _gameManager.SoundEffectManager.NextPitch());
	}

	private void _activateHit(
		int nodeId, 
		int teamId,
		NodeColors nodeColors,  
		Vector3 spawnPosition,
		float scale,
		SoundEffectManager.PitchToPlay explosionPitch
		)
	{
		int inactiveCount = inactiveHits.Count;
		if(inactiveCount == 0)
        {
			return;
        }

		Hit hit = inactiveHits[inactiveCount - 1];
		hit.SetScale(scale);
		hit.transform.position = spawnPosition;
		hit.SetColor(nodeColors.OutsideColor);

		hit.LightIndex = _gameManager.Grid.ColorManager.GetLightIndex();
		_gameManager.Grid.ColorManager.SetLightColor(hit.LightIndex, hit.Color);
		_gameManager.Grid.ColorManager.SetLightPosition(hit.LightIndex, spawnPosition);

		hit.TeamId = teamId;
		hit.HitType = nodeId;
		hit.ExplosionPitch = explosionPitch;

		hit.gameObject.SetActive(true);
		inactiveHits.RemoveAt(inactiveCount - 1);
		activeHits.Add(hit);
	}
	
	public void DeactivateHit(int index)
	{
		Hit hit = activeHits[index];
		hit.ReleaseLightIndex();
		hit.gameObject.SetActive(false);
		activeHits.RemoveAt(index);
		inactiveHits.Add(hit);
	}

	public void DeactivateHit(Hit hit)
	{
		hit.ReleaseLightIndex();
		hit.gameObject.SetActive(false);
		activeHits.Remove(hit);
		inactiveHits.Add(hit);
	}

}
