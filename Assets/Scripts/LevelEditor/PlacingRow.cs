using UnityEngine;
using System.Collections.Generic;

public class PlacingRow : MonoBehaviour 
{
	List<GameObject> placeableObjectsTouching;
	public ObjectRowEnum Row;

	Color startColor;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	// Use this for initialization
	void Start () 
	{
		startColor = _spriteRenderer.color;
		placeableObjectsTouching = new List<GameObject>();

		transform.localScale = new Vector3(1,1,1);
		
		float width = _spriteRenderer.sprite.bounds.size.x;
		
		float worldScreenHeight = Camera.main.orthographicSize*2f;
		float worldScreenWidth = worldScreenHeight/Screen.height*Screen.width;
		
		Vector3 xWidth = transform.localScale;
		xWidth.x = worldScreenWidth / width;
		transform.localScale = xWidth;
	}
	
	// Update is called once per frame
	void Update() 
	{
		Color currentColor = _spriteRenderer.color;
		if(currentColor.a > startColor.a)
		{
			_spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b,
			                                                 currentColor.a - (Time.deltaTime * 3));
		}

		if(_spriteRenderer.color.a < startColor.a)
		{
			_spriteRenderer.color = startColor;
		}
	}

	public void AddPlaceableObjectTouching(GameObject placeableObject)
	{
		if(!placeableObjectsTouching.Contains(placeableObject))
		{
			placeableObjectsTouching.Add(placeableObject);
		}
	}

	public void RemovePlaceableObjectTouching(GameObject placeableObject)
	{
		if(placeableObjectsTouching.Contains(placeableObject))
		{
			placeableObjectsTouching.Remove(placeableObject);
		}
	}

	public void StartFlash()
	{
		_spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
	}
}
