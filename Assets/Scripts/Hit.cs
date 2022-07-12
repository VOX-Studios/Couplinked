using System;
using UnityEngine;

public class Hit : BaseObject 
{
	private IHitCollisionHandler _hitCollisionHandler;

	public int HitType;
	public int TeamId;
	public float Scale;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private SpriteRenderer _blurSpriteRenderer;

	private MaterialPropertyBlock _propertyBlock;

	[NonSerialized]
	public int LightIndex = -1;

	public Color Color;

	// Use this for initialization
	public void Initialize(IHitCollisionHandler hitCollisionHandler) 
	{
		_hitCollisionHandler = hitCollisionHandler;
		_propertyBlock = new MaterialPropertyBlock();
		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}

	public void SetScale(float scale)
	{
		Scale = scale;
		transform.localScale = new Vector3(scale, scale, 1);
	}

	public void SetColor(Color color)
    {
		Color = color;
		_setColor(_spriteRenderer, color);
		_setColor(_blurSpriteRenderer, color);
	}

	private void _setColor(SpriteRenderer renderer, Color color)
    {
		//get the current value of properties in the renderer
		renderer.GetPropertyBlock(_propertyBlock);

		_propertyBlock.SetColor("_OutsideColor", color);

		//apply values to the renderer
		renderer.SetPropertyBlock(_propertyBlock);
	}
	
	public void Move(float time) 
	{
		transform.position -= new Vector3 (Speed * Scale, 0, 0) * time;
		//_GameManager.Grid.Logic.ApplyImplosiveForce(1 * Scale, transform.position, 1 * Scale);
	}

	public void ReleaseLightIndex()
	{
		_GameManager.LightingManager.ReleaseLightIndex(LightIndex);
		LightIndex = -1;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_hitCollisionHandler.OnHitCollision(this, other);
	}
}
