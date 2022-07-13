using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitManager : MonoBehaviour 
{
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
	
	public void Run(bool isPaused, float deltaTime) 
	{
		if(isPaused)
        {
			_runPaused();
			return;
		}

		for(int i = activeHits.Count - 1; i >= 0; i--)
		{
			Hit hit = activeHits[i];
			hit.Move(deltaTime);
			_gameManager.LightingManager.SetLightPosition(hit.LightIndex, hit.transform.position);

			if (activeHits[i].transform.position.x < GameManager.LeftX - activeHits[i].gameObject.GetComponent<Renderer>().bounds.extents.x)
			{
				DeactivateHit(i);

				if (_gameSceneManager.EndGame(ReasonForGameEndEnum.HitOffScreen)) //TODO: should this be here?
				{
					break;
				}
			}

		}
	}

	private void _runPaused()
	{
		foreach (Hit hit in activeHits)
		{
			_gameManager.LightingManager.SetLightPosition(hit.LightIndex, hit.transform.position);
		}
	}

	public void SpawnHit(int nodeId, int teamId, Color hitColor, Vector3 position, float scale)
	{
		_activateHit(nodeId, teamId, hitColor, position, scale, _gameManager.SoundEffectManager.NextPitch());
	}

	private void _activateHit(
		int nodeId, 
		int teamId,
		Color hitColor,  
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
		hit.SetColor(hitColor);

		hit.LightIndex = _gameManager.LightingManager.GetLightIndex();
		_gameManager.LightingManager.SetLightColor(hit.LightIndex, hit.Color);
		_gameManager.LightingManager.SetLightPosition(hit.LightIndex, spawnPosition);

		hit.TeamId = teamId;
		hit.NodeId = nodeId;
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
