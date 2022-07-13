using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitSplitManager : MonoBehaviour 
{
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

	public void Run(bool isPaused, float deltaTime)
	{
		if (isPaused)
		{
			_runPaused();
			return;
		}

		for (int i = activeHitSplits.Count - 1; i >= 0; i--)
		{
			HitSplit hitSplit = activeHitSplits[i];
			hitSplit.Move(deltaTime);
			_gameManager.LightingManager.SetLightPosition(hitSplit.LightIndex, hitSplit.transform.position);

			if (activeHitSplits[i].transform.position.x < GameManager.LeftX - activeHitSplits[i].gameObject.GetComponent<Renderer>().bounds.size.x)
			{
				if(!activeHitSplits[i].GetComponent<HitSplit>().WasHitTwice)
				{
					DeactivateHitSplit(i);

					if (_gameSceneManager.EndGame(ReasonForGameEndEnum.HitSplitOffScreen)) //TODO: should this be here?
					{
						break;
					}
				}
			}
		}
	}

	private void _runPaused()
	{
		foreach (HitSplit hitSplit in activeHitSplits)
		{
			_gameManager.LightingManager.SetLightPosition(hitSplit.LightIndex, hitSplit.transform.position);
		}
	}

	public void SpawnHitSplit(
		int hitSplitFirstType,
		int hitSplitSecondType,
		int firstHitTeamId,
		int secondHitTeamId,
		Color firstHitColor,
		Color secondHitColor,
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

			hitSplit.SetColors(secondHitColor, firstHitColor);
			hitSplit.LightIndex = _gameManager.LightingManager.GetLightIndex();
			_gameManager.LightingManager.SetLightColor(hitSplit.LightIndex, hitSplit.OutsideColor);
			_gameManager.LightingManager.SetLightPosition(hitSplit.LightIndex, spawnPosition);


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
