using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitSplitManager : MonoBehaviour 
{
	float spawnInterval = 5f;
	float spawnTimer = 0f;
	public GameObject HitSplitPrefab;

	public List<HitSplit> activeHitSplits;
	public List<HitSplit> inactiveHitSplits;

	private GameManager _gameManager;

    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

        activeHitSplits = new List<HitSplit>();
		inactiveHitSplits = new List<HitSplit>();

		Transform parent = GameObject.Find("HitSplitManager").transform;
		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject hitSplit = (GameObject)Instantiate(HitSplitPrefab);
			HitSplit hitSplitComponent = hitSplit.GetComponent<HitSplit>();
			hitSplitComponent.Initialize();
			hitSplit.transform.parent = parent;
			hitSplit.SetActive(false);
			inactiveHitSplits.Add(hitSplitComponent);
		}
	}
	
	public void Run(float time) 
	{
		spawnTimer -= time;
		
		if(spawnTimer <= 0)
		{
			spawnTimer = spawnInterval;
		}

		for(int i = activeHitSplits.Count - 1; i >= 0; i--)
		{
			activeHitSplits[i].Move(time);


			if(activeHitSplits[i].transform.position.x < GameManager.LeftX - activeHitSplits[i].gameObject.GetComponent<Renderer>().bounds.size.x)
			{
				if(!activeHitSplits[i].GetComponent<HitSplit>().WasHitTwice)
				{
					DeactivateHitSplit(i);
					if(_gameSceneManager.EndGame(ReasonForGameEndEnum.HitSplitOffScreen))
                        break;
				}
			}
		}
	}

	public void SpawnHitSplit(
		int hitSplitFirstType,
		int hitSplitSecondType,
		int firstHitTeamId,
		int secondHitTeamId,
		NodeColors firstHitNodeColors,
		NodeColors secondHitNodeColors,
		Vector3 spawnPosition,
		float scale
		)
	{
		int inactiveCount = inactiveHitSplits.Count;
		if (inactiveCount > 0)
		{
			HitSplit hitSplit = inactiveHitSplits[inactiveCount - 1];
			hitSplit.OnSpawn();
			hitSplit.SetScale(scale);
			hitSplit.transform.position = spawnPosition;

			hitSplit.HitSplitFirstType = hitSplitFirstType;
			hitSplit.HitSplitSecondType = hitSplitSecondType;

			Color firstHitColor = firstHitNodeColors.OutsideColor;
			Color secondHitColor = secondHitNodeColors.OutsideColor;

			hitSplit.SetColors(secondHitColor, firstHitColor);
			hitSplit.LightIndex = _gameManager.Grid.ColorManager.GetLightIndex();
			_gameManager.Grid.ColorManager.SetLightColor(hitSplit.LightIndex, hitSplit.OutsideColor);
			_gameManager.Grid.ColorManager.SetLightPosition(hitSplit.LightIndex, spawnPosition);


			hitSplit.FirstHitTeamId = firstHitTeamId;
			hitSplit.SecondHitTeamId = secondHitTeamId;
			hitSplit.WasHitOnce = false;
			hitSplit.WasHitTwice = false;
			hitSplit.ExplosionPitch = _gameManager.SoundEffectManager.NextPitch();

			hitSplit.gameObject.SetActive(true);

			inactiveHitSplits.RemoveAt(inactiveCount - 1);
			activeHitSplits.Add(hitSplit);
		}
	}

	public void DeactivateHitSplit(int index)
	{
		HitSplit hitSplit = activeHitSplits[index];
		hitSplit.ReleaseLightIndex();
		hitSplit.gameObject.SetActive(false);
		activeHitSplits.RemoveAt(index);
		inactiveHitSplits.Add(hitSplit);
	}
	
	public void DeactivateHitSplit(HitSplit hitSplit)
	{
		hitSplit.ReleaseLightIndex();
		hitSplit.gameObject.SetActive(false);
		activeHitSplits.Remove(hitSplit);
		inactiveHitSplits.Add(hitSplit);
	}
}
