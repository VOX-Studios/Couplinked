using Assets.Scripts.GameEntities;
using UnityEngine;

public class HitSplit : GameEntity 
{
	public override GameEntityTypeEnum GameEntityType => GameEntityTypeEnum.HitSplit;

	private ICollisionHandler<HitSplit> _hitSplitCollisionHandler;

	[SerializeField]
	private SpriteRenderer _insideSpriteRenderer;

	[SerializeField]
	private SpriteRenderer _outsideSpriteRenderer;

	[SerializeField]
	private SpriteRenderer _blurInsideSpriteRenderer;

	[SerializeField]
	private SpriteRenderer _blurOutsideSpriteRenderer;

	public int FirstHitTeamId;
	public int SecondHitTeamId;
	public int HitSplitFirstType;
	public int HitSplitSecondType;
	public bool WasHitOnce = false;
	public bool WasHitTwice = false;

	public float Scale { get; private set; }

	private MaterialPropertyBlock _propertyBlock;

	public Color InsideColor { get; private set; }
	public Color OutsideColor { get; private set; }

	// Use this for initialization
	public void Initialize(ICollisionHandler<HitSplit> hitSplitCollisionHandler) 
	{
		_hitSplitCollisionHandler = hitSplitCollisionHandler;

		_propertyBlock = new MaterialPropertyBlock();

		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}
	public void OnSpawn()
    {
		_insideSpriteRenderer.enabled = true;
		_blurInsideSpriteRenderer.enabled = true;
	}

	public void SetScale(float scale)
	{
		Scale = scale;
		transform.localScale = new Vector3(scale, scale, 1);
	}

	public void SetColors(Color insideColor, Color outsideColor)
	{
		InsideColor = insideColor;
		OutsideColor = outsideColor;
		_setInsideColor(_blurInsideSpriteRenderer, insideColor);
		_setInsideColor(_insideSpriteRenderer, insideColor);

		_setOutsideColor(_blurOutsideSpriteRenderer, outsideColor);
		_setOutsideColor(_outsideSpriteRenderer, outsideColor);
	}

	public void SetColors(Color outsideColor)
	{
		SetColors(Color.clear, outsideColor);

		_insideSpriteRenderer.enabled = false;
		_blurInsideSpriteRenderer.enabled = false;
	}

	private void _setInsideColor(SpriteRenderer renderer, Color insideColor)
	{
		//get the current value of properties in the renderer
		renderer.GetPropertyBlock(_propertyBlock);

		_propertyBlock.SetColor("_InsideColor", insideColor);

		//apply values to the renderer
		renderer.SetPropertyBlock(_propertyBlock);
	}

	private void _setOutsideColor(SpriteRenderer renderer, Color outsideColor)
	{
		//get the current value of properties in the renderer
		renderer.GetPropertyBlock(_propertyBlock);

		_propertyBlock.SetColor("_OutsideColor", outsideColor);

		//apply values to the renderer
		renderer.SetPropertyBlock(_propertyBlock);
	}

	public override void Move(float time) 
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

		_hitSplitCollisionHandler.OnCollision(this, other);
	}
}
