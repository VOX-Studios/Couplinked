using UnityEngine;

public class CameraShake
{
	private Transform _camTransform;

	/// <summary>
	/// The original camera position before we've applied any shake.
	/// </summary>
	public Vector3 OriginalPos { get; private set; }

	//how long the camera should shake for
	private float _shakeTime = 0f;

	//how much longer the camera will shake for
	private float _shakeTimeRemaining = 0f;

	//strength of the shake
	private float _shakeStrength = .25f;

	public float Scale = 1;
	
	public void Initialize(GameManager gameManager)
	{
		_camTransform = gameManager.Cam.transform;
		OriginalPos = _camTransform.position;

		QualitySettingEnum shakeStrength = gameManager.DataManager.ScreenShakeStrength.Get();

		switch (shakeStrength)
		{
			default:
			case QualitySettingEnum.Off:
				_shakeStrength = 0f;
				break;
			case QualitySettingEnum.Low:
				_shakeStrength = 0.0625f;
				break;
			case QualitySettingEnum.Medium:
				_shakeStrength = 0.125f;
				break;
			case QualitySettingEnum.High:
				_shakeStrength = .25f;
				break;
		}
	}

	public void ClearShake()
	{
		_shakeTime = 0;
		_shakeTimeRemaining = 0;
		_camTransform.position = OriginalPos;
	}

	public void StartShake(float duration)
	{
		_shakeTime = duration;
		_shakeTimeRemaining = _shakeTime;
	}

	public void Run(float deltaTime)
	{
		if (_shakeTimeRemaining > 0)
		{
			_camTransform.position = OriginalPos + Random.insideUnitSphere * _shakeStrength * Scale * (_shakeTimeRemaining / _shakeTime);
			_shakeTimeRemaining -= deltaTime;
		}
		else
		{
			_shakeTimeRemaining = 0f;
			_shakeTime = 0f;
			
			//move towards the original position
			_camTransform.position = Vector3.Lerp(_camTransform.position, OriginalPos, deltaTime);

			//if we're close enough to where we should be
			if (Vector3.SqrMagnitude(_camTransform.position - OriginalPos) < .001f)
			{
				//just snap to the position
				_camTransform.position = OriginalPos;
			}
		}
	}
}