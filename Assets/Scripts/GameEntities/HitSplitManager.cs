using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;

public class HitSplitManager : MonoBehaviour, IGameEntityManager<HitSplit>
{
	public GameObject HitSplitPrefab;

	public List<HitSplit> ActiveGameEntities { get; private set; }
	private List<HitSplit> _inactiveHitSplits;

	private GameManager _gameManager;

    private GameSceneManager _gameSceneManager;

    // Use this for initialization
    public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

        ActiveGameEntities = new List<HitSplit>();
		_inactiveHitSplits = new List<HitSplit>();

		Transform parent = GameObject.Find("HitSplitManager").transform;
		for(int i = 0; i < Common.MaxPerObjectInGame; i++)
		{
			GameObject hitSplit = (GameObject)Instantiate(HitSplitPrefab);
			HitSplit hitSplitComponent = hitSplit.GetComponent<HitSplit>();
			hitSplitComponent.Initialize();
			hitSplit.transform.parent = parent;
			hitSplit.SetActive(false);
			_inactiveHitSplits.Add(hitSplitComponent);
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
		int inactiveCount = _inactiveHitSplits.Count;
		if (inactiveCount > 0)
		{
			HitSplit hitSplit = _inactiveHitSplits[inactiveCount - 1];
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

			_inactiveHitSplits.RemoveAt(inactiveCount - 1);
			ActiveGameEntities.Add(hitSplit);
		}
	}

	public void DeactivateGameEntity(int index)
	{
		HitSplit hitSplit = ActiveGameEntities[index];
		hitSplit.ReleaseLightIndex();
		hitSplit.gameObject.SetActive(false);
		ActiveGameEntities.RemoveAt(index);
		_inactiveHitSplits.Add(hitSplit);
	}
	
	public void DeactivateGameEntity(HitSplit hitSplit)
	{
		hitSplit.ReleaseLightIndex();
		hitSplit.gameObject.SetActive(false);
		ActiveGameEntities.Remove(hitSplit);
		_inactiveHitSplits.Add(hitSplit);
	}
}
