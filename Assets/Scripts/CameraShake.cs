using UnityEngine;

public class CameraShake
{
	private Transform _camTransform;

	/// <summary>
	/// The original camera position before we've applied any shake.
	/// </summary>
	public Vector3 OriginalPos { get; private set; }

	/// <summary>
	/// How long the camera should shake for.
	/// </summary>
	private float _shakeTime = 0f;

	/// <summary>
	/// How much longer the camera will shake for.
	/// </summary>
	private float _shakeTimeRemaining = 0f;

	/// <summary>
	/// Strength of the shake.
	/// </summary>
	private float _shakeStrength = .25f;

	public float Scale = 1;

	private Vector3 _targetPosition;

	/// <summary>
	/// How many times the camera should move each second.
	/// </summary>
	private uint _shakesPerSecond = 60;

	/// <summary>
	/// Time until the camera should move again.
	/// </summary>
	private float _timeUntilNextShake = 0f;

	/// <summary>
	/// Value for lerping between current and target position.
	/// </summary>
	private float _shakeLerp = 0f;

	/// <summary>
	/// Value for lerping between current and original position.
	/// </summary>
	private float _resetLerp = 0f;
	
	public void Initialize(GameManager gameManager)
	{
		_camTransform = gameManager.Cam.transform;
		OriginalPos = _camTransform.position;
		_targetPosition = OriginalPos;

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
			_resetLerp = 0f;

			_timeUntilNextShake -= deltaTime;
			if(_timeUntilNextShake <= 0)
            {
				Vector2 randomOffset = Random.insideUnitCircle;
				_targetPosition = OriginalPos + new Vector3(randomOffset.x, randomOffset.y) * _shakeStrength * Scale * (_shakeTimeRemaining / _shakeTime);
				_timeUntilNextShake = 1f/_shakesPerSecond;
				_shakeLerp = 0;
			}

			_shakeLerp += deltaTime * _shakesPerSecond * 2;
			_shakeLerp = Mathf.Clamp(_shakeLerp, 0f, 1f);
			_camTransform.position = Vector3.Lerp(_camTransform.position, _targetPosition, _shakeLerp);
			_shakeTimeRemaining -= deltaTime;
		}
		else
		{
			_shakeTimeRemaining = 0f;
			_shakeTime = 0f;
			_timeUntilNextShake = 0f;

			//multiplied by shakes per second so it keeps the same timing (more or less)
			_resetLerp += deltaTime * _shakesPerSecond;

			_resetLerp = Mathf.Clamp(_resetLerp, 0f, 1f);

			//move towards the original position
			_camTransform.position = Vector3.Lerp(_camTransform.position, OriginalPos, _resetLerp);
		}
	}
}