using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitSplitManager : MonoBehaviour 
{
	float spawnInterval = 5f;
	float spawnTimer = 0f;
	public GameObject HitSplitPrefab;

	public List<GameObject> activeHitSplits;
	public List<GameObject> inactiveHitSplits;

	private GameManager _gameManager;

    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

        activeHitSplits = new List<GameObject>();
		inactiveHitSplits = new List<GameObject>();

		Transform parent = GameObject.Find("HitSplitManager").transform;
		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject hitSplit = (GameObject)Instantiate(HitSplitPrefab);
			HitSplit hitSplitComponent = hitSplit.GetComponent<HitSplit>();
			hitSplitComponent.Initialize();
			hitSplit.transform.parent = parent;
			hitSplit.SetActive(false);
			inactiveHitSplits.Add(hitSplit);
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
			activeHitSplits[i].GetComponent<HitSplit>().Move(time);


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
		HitTypeEnum hitSplitFirstType,
		HitTypeEnum hitSplitSecondType,
		int firstHitTeamId,
		int secondHitTeamId,
		PlayerColors firstHitPlayerColors,
		PlayerColors secondHitPlayerColors,
		Vector3 spawnPosition,
		float scale
		)
	{
		_activateHitSplit(
			hitSplitFirstType: hitSplitFirstType,
			hitSplitSecondType: hitSplitSecondType,
			firstHitTeamId: firstHitTeamId,
			secondHitTeamId: secondHitTeamId,
			firstHitPlayerColors: firstHitPlayerColors,
			secondHitPlayerColors: secondHitPlayerColors,
			spawnPosition: spawnPosition,
			scale: scale
			);
	}
	
	private void _activateHitSplit(
		HitTypeEnum hitSplitFirstType,
		HitTypeEnum hitSplitSecondType,
		int firstHitTeamId,
		int secondHitTeamId,
		PlayerColors firstHitPlayerColors,
		PlayerColors secondHitPlayerColors,
		Vector3 spawnPosition,
		float scale
		)
	{
		int inactiveCount = inactiveHitSplits.Count;
		if(inactiveCount > 0)
		{
			GameObject hitSplit = inactiveHitSplits[inactiveCount - 1];
			HitSplit hitSplitComponent = hitSplit.GetComponent<HitSplit>();
			hitSplitComponent.OnSpawn();
			hitSplitComponent.SetScale(scale);
			hitSplit.transform.position = spawnPosition;

			hitSplitComponent.HitSplitFirstType = hitSplitFirstType;
			hitSplitComponent.HitSplitSecondType = hitSplitSecondType;

			Color firstHitColor = firstHitPlayerColors.NodeColors[(int)hitSplitFirstType].OutsideColor;
			Color secondHitColor = secondHitPlayerColors.NodeColors[(int)hitSplitSecondType].OutsideColor;

			hitSplitComponent.SetColors(secondHitColor, firstHitColor);
			hitSplitComponent.FirstHitTeamId = firstHitTeamId;
			hitSplitComponent.SecondHitTeamId = secondHitTeamId;
			hitSplitComponent.WasHitOnce = false;
			hitSplitComponent.WasHitTwice = false;
			hitSplitComponent.ExplosionPitch = _gameManager.SoundEffectManager.NextPitch();

			hitSplit.SetActive(true);

			inactiveHitSplits.RemoveAt(inactiveCount - 1);
			activeHitSplits.Add(hitSplit);
		}
	}

	public void DeactivateHitSplit(int index)
	{
		GameObject hitSplit = activeHitSplits[index];
		hitSplit.SetActive(false);
		activeHitSplits.RemoveAt(index);
		inactiveHitSplits.Add(hitSplit);
	}
	
	public void DeactivateHitSplit(GameObject hitSplit)
	{
		hitSplit.SetActive(false);
		activeHitSplits.Remove(hitSplit);
		inactiveHitSplits.Add(hitSplit);
	}
}
