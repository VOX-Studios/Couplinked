using System;
using UnityEngine;

public class Explosion : MonoBehaviour 
{
	[NonSerialized]
	public QualitySettingEnum QualitySetting;

	public void Explode(Vector3 position, Color color)
	{
		ParticleSystem particleSystem = GetComponent<ParticleSystem>();
		particleSystem.Stop();
		particleSystem.Clear();
		gameObject.SetActive(true);

        ParticleSystem.MainModule main = particleSystem.main;
		main.startColor = color;

		ParticleSystem.EmissionModule emission = particleSystem.emission;
        ParticleSystem.Burst burst = emission.GetBurst(0);

        switch (QualitySetting)
        {
			default:
            case QualitySettingEnum.Off:
				burst.count = 0;
				break;
            case QualitySettingEnum.Low:
				burst.count = 250;
				break;
            case QualitySettingEnum.Medium:
				burst.count = 500;
				break;
            case QualitySettingEnum.High:
				burst.count = 1000;
				break;
        }
        
		emission.SetBurst(0, burst);

		transform.position = position;
		particleSystem.Play();
	}
}
