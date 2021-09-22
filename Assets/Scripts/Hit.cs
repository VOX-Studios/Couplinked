using UnityEngine;

public class Hit : BaseObject 
{
	private IHitCollisionHandler _hitCollisionHandler;

	public HitTypeEnum HitType;
	public int TeamId;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private SpriteRenderer _blurSpriteRenderer;

	private MaterialPropertyBlock _propertyBlock;

	// Use this for initialization
	public void Initialize(IHitCollisionHandler hitCollisionHandler) 
	{
		_hitCollisionHandler = hitCollisionHandler;
		_propertyBlock = new MaterialPropertyBlock();
		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}

	public void SetColor(Color color)
    {
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
		transform.position -= new Vector3 (Speed, 0, 0) * time;
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_hitCollisionHandler.OnHitCollision(this, other);
	}
}
