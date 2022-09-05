using UnityEngine;
using System.Collections.Generic;

public class PlacingRow : MonoBehaviour 
{
	private GameManager _gameManager;
	private List<GameObject> _placeableObjectsTouching;

	public byte Row;

	private Color _startColor;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	// Use this for initialization
	void Start () 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		_startColor = _spriteRenderer.color;
		_placeableObjectsTouching = new List<GameObject>();

		transform.localScale = new Vector3(1,1,1);
		
		float width = _spriteRenderer.sprite.bounds.size.x;
		
		float worldScreenHeight = _gameManager.Cam.orthographicSize*2f;
		float worldScreenWidth = worldScreenHeight/Screen.height*Screen.width;
		
		Vector3 xWidth = transform.localScale;
		xWidth.x = worldScreenWidth / width;
		transform.localScale = xWidth;
	}
	
	// Update is called once per frame
	void Update() 
	{
		Color currentColor = _spriteRenderer.color;
		if(currentColor.a > _startColor.a)
		{
			_spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b,
			                                                 currentColor.a - (Time.deltaTime * 3));
		}

		if(_spriteRenderer.color.a < _startColor.a)
		{
			_spriteRenderer.color = _startColor;
		}
	}

	public void AddPlaceableObjectTouching(GameObject placeableObject)
	{
		if(!_placeableObjectsTouching.Contains(placeableObject))
		{
			_placeableObjectsTouching.Add(placeableObject);
		}
	}

	public void RemovePlaceableObjectTouching(GameObject placeableObject)
	{
		if(_placeableObjectsTouching.Contains(placeableObject))
		{
			_placeableObjectsTouching.Remove(placeableObject);
		}
	}

	public void StartFlash()
	{
		_spriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, 1f);
	}
}
