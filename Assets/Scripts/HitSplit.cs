﻿using Assets.Scripts.SceneManagers;
using UnityEngine;

public class HitSplit : BaseObject 
{
    private GameSceneManager _gameSceneManager;

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
	public HitTypeEnum HitSplitFirstType;
	public HitTypeEnum HitSplitSecondType;
	public bool WasHitOnce = false;
	public bool WasHitTwice = false;

	private MaterialPropertyBlock _propertyBlock;

	// Use this for initialization
	public void Initialize() 
	{
        _gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();

		_propertyBlock = new MaterialPropertyBlock();

		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}
	public void OnSpawn()
    {
		_insideSpriteRenderer.enabled = true;
		_blurInsideSpriteRenderer.enabled = true;
	}

	public void SetColors(Color insideColor, Color outsideColor)
	{
		_setInsideColor(_blurInsideSpriteRenderer, insideColor);
		_setInsideColor(_insideSpriteRenderer, insideColor);

		_setOutsideColor(_blurOutsideSpriteRenderer, outsideColor);
		_setOutsideColor(_outsideSpriteRenderer, outsideColor);
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


	public void Move(float time) 
	{
		transform.position -= new Vector3 (Speed, 0, 0) * time;
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_gameSceneManager.OnHitSplitCollision(this, other);
	}

	public void SetColors(Color outsideColor)
	{
		SetColors(Color.clear, outsideColor);

		_insideSpriteRenderer.enabled = false;
		_blurInsideSpriteRenderer.enabled = false;
	}
}