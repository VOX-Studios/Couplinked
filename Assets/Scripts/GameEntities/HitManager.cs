using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitManager : MonoBehaviour, IGameEntityManager<Hit>
{
	public GameObject HitPrefab;

	public List<Hit> ActiveGameEntities { get; private set; }
	private List<Hit> _inactiveHits;

	private GameManager _gameManager;

    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

		ActiveGameEntities = new List<Hit>();
		_inactiveHits = new List<Hit>();

		//TODO: move them to be under bolts
		Transform parent = GameObject.Find("HitManager").transform;
		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject hit = (GameObject)Instantiate(HitPrefab);
			Hit hitComponent = hit.GetComponent<Hit>();
			hitComponent.Initialize(_gameSceneManager);

			hit.transform.parent = parent;
			hit.SetActive(false);
			_inactiveHits.Add(hitComponent);
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
		int inactiveCount = _inactiveHits.Count;
		if(inactiveCount == 0)
        {
			return;
        }

		Hit hit = _inactiveHits[inactiveCount - 1];
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
		_inactiveHits.RemoveAt(inactiveCount - 1);
		ActiveGameEntities.Add(hit);
	}
	
	public void DeactivateGameEntity(int index)
	{
		Hit hit = ActiveGameEntities[index];
		hit.ReleaseLightIndex();
		hit.gameObject.SetActive(false);
		ActiveGameEntities.RemoveAt(index);
		_inactiveHits.Add(hit);
	}

	public void DeactivateGameEntity(Hit hit)
	{
		hit.ReleaseLightIndex();
		hit.gameObject.SetActive(false);
		ActiveGameEntities.Remove(hit);
		_inactiveHits.Add(hit);
	}

}
