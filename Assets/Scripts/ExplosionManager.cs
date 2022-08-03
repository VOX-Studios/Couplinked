using UnityEngine;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour 
{
	public GameObject ExplosionPrefab;

	private List<GameObject> _activeExplosions;
	private  List<GameObject> _inactiveExplosions;
	private int _maxExplosions = 10; //TODO: scale this with the number of nodes
	private float _scale = 1;
	
	public void Initialize(DataManager dataManager) 
	{
		_activeExplosions = new List<GameObject>();
		_inactiveExplosions = new List<GameObject>();

		Transform parent = GameObject.Find("ExplosionManager").transform;

		for(int i = 0; i < _maxExplosions; i++)
		{
			GameObject explosion = Instantiate(ExplosionPrefab);
			explosion.GetComponent<Explosion>().QualitySetting = dataManager.ExplosionParticleQuality.Get();
			explosion.transform.parent = parent;
			explosion.SetActive(false);
			_inactiveExplosions.Add(explosion);
		}
	}

	/// <summary>
	/// This should only be called once after initialize.
	/// </summary>
	/// <param name="scale"></param>
	public void SetScale(float scale)
    {
		_scale = scale;
		foreach (GameObject explosion in _inactiveExplosions)
        {
            ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();

			ParticleSystem.MainModule particleMain = particleSystem.main;
			particleMain.startSize = new ParticleSystem.MinMaxCurve(particleMain.startSize.constant * _scale);
			particleMain.startSpeed = new ParticleSystem.MinMaxCurve(particleMain.startSpeed.constant * _scale);

			ParticleSystem.ShapeModule particleShape = particleSystem.shape;
			particleShape.radius *= _scale;
		}
	}

	public void Run()
	{
		int activeCount = _activeExplosions.Count;
		for(int i = activeCount - 1; i >= 0; i--)
		{
			GameObject explosion = _activeExplosions[i];
			if(!explosion.GetComponent<ParticleSystem>().IsAlive())
			{
				DeactivateExplosion(i);
			}
		}
	}

	public void Play()
	{
		for (int i = 0; i < _activeExplosions.Count; i++)
		{
			_activeExplosions[i].GetComponent<ParticleSystem>().Play();
		}
	}

	public void Pause()
    {
		for (int i = 0; i < _activeExplosions.Count; i++)
		{
			_activeExplosions[i].GetComponent<ParticleSystem>().Pause();
		}
	}
	
	public void ActivateExplosion(Vector3 spawnPosition, Color color)
	{
		int inactiveCount = _inactiveExplosions.Count;
		if(inactiveCount > 0)
		{
			GameObject explosion = _inactiveExplosions[inactiveCount - 1];
			explosion.GetComponent<Explosion>().Explode(spawnPosition, color);
			_inactiveExplosions.RemoveAt(inactiveCount - 1);
			_activeExplosions.Add(explosion);
		}
	}

	public void DeactiveExplosions()
    {
		for(int i = _activeExplosions.Count - 1; i >= 0; i--)
        {
			DeactivateExplosion(i);
        }
    }
	
	public void DeactivateExplosion(int index)
	{
		GameObject explosion = _activeExplosions[index];
		explosion.SetActive(false);
		_activeExplosions.RemoveAt(index);
		_inactiveExplosions.Add(explosion);
	}
}
