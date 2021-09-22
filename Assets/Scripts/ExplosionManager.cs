using UnityEngine;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour 
{
	public GameObject ExplosionPrefab;

	public List<GameObject> ActiveExplosions;
	public List<GameObject> InactiveExplosions;
	private int _maxExplosions = 10;
	
	public void Initialize(DataManager dataManager) 
	{
		ActiveExplosions = new List<GameObject>();
		InactiveExplosions = new List<GameObject>();

		Transform parent = GameObject.Find("ExplosionManager").transform;

		for(int i = 0; i < _maxExplosions; i++)
		{
			GameObject explosion = Instantiate(ExplosionPrefab);
			explosion.GetComponent<Explosion>().QualitySetting = dataManager.ExplosionParticleQuality.Get();
			explosion.transform.parent = parent;
			explosion.SetActive(false);
			InactiveExplosions.Add(explosion);
		}
	}

	public void Run()
	{
		int activeCount = ActiveExplosions.Count;
		for(int i = activeCount - 1; i >= 0; i--)
		{
			GameObject explosion = ActiveExplosions[i];
			if(!explosion.GetComponent<ParticleSystem>().IsAlive())
			{
				DeactivateExplosion(i);
			}
		}
	}
	
	public void ActivateExplosion(Vector3 spawnPosition, Color color)
	{
		int inactiveCount = InactiveExplosions.Count;
		if(inactiveCount > 0)
		{
			GameObject explosion = InactiveExplosions[inactiveCount - 1];
			explosion.GetComponent<Explosion>().Explode(spawnPosition, color);
			InactiveExplosions.RemoveAt(inactiveCount - 1);
			ActiveExplosions.Add(explosion);
		}
	}
	
	public void DeactivateExplosion(int index)
	{
		GameObject explosion = ActiveExplosions[index];
		explosion.SetActive(false);
		ActiveExplosions.RemoveAt(index);
		InactiveExplosions.Add(explosion);
	}
}
