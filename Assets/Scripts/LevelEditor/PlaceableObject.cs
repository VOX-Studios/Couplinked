using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SceneManagers;
using UnityEngine.EventSystems;

public class PlaceableObject : MonoBehaviour 
{
	public PlaceableObjectImage PlaceableObjectImage;
	public ObjectTypeEnum ObjectType;
	public PlacingRow SelectedPlacingRow;
	public bool SpaceToPlace;

	private LevelEditorSceneManager _levelEditorSceneManager;

	[SerializeField]
	private GameObject _indicatorLine;

	private RaycastHit2D[] _hits;
	private List<RaycastResult> _raycastResults;
	private GameManager _gameManager;

	public void Initialize(GameManager gameManager)
	{
		_gameManager = gameManager;
		_raycastResults = new List<RaycastResult>();
		_levelEditorSceneManager = GameObject.FindObjectOfType<LevelEditorSceneManager>();
		_indicatorLine.transform.position = new Vector3(0, (GameManager.TopY + GameManager.BotY) / 2, 0);

		SpriteRenderer sr = _indicatorLine.GetComponent<SpriteRenderer>();
		
		float width = sr.sprite.bounds.size.x;
		
		float worldScreenHeight = GameManager.TopY - GameManager.BotY;
		
		float xScale = worldScreenHeight / width;
		
		_indicatorLine.transform.localScale = new Vector3(
			xScale,
			_indicatorLine.transform.localScale.y,
		    _indicatorLine.transform.localScale.z
			);
	}

	public void Activate(Vector3 position)
	{
		transform.position = position;

		_handleOverlappingPlacingRow();

		_indicatorLine.transform.position = new Vector3(
					x: transform.position.x,
					y: (GameManager.TopY + GameManager.BotY) / 2,
					z: _indicatorLine.transform.position.z
				);

	}
	
	public void Run(Vector2 input) 
	{
		SpaceToPlace = true;
		_handleInputMove(input);

		if (_indicatorLine.activeSelf)
		{
			_indicatorLine.transform.position = new Vector3(
					x: transform.position.x,
					y: (GameManager.TopY + GameManager.BotY) / 2,
					z: _indicatorLine.transform.position.z
				);
		}

		PlaceableObjectImage.transform.position = _gameManager.Cam.WorldToScreenPoint(transform.position);
	}

	private void _handleTimeMarkerSnap(Vector2 input)
    {
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = _gameManager.Cam.WorldToScreenPoint(input);
		EventSystem.current.RaycastAll(eventData, _raycastResults);

		for (int i = 0; i < _raycastResults.Count; i++)
		{
			if (_raycastResults[i].gameObject.tag == "Level Editor Buttons BG")
			{
				_raycastResults.Clear();
				return; //don't snap if we're hovering UI BG
			}
		}

		_raycastResults.Clear();


		_hits = Physics2D.CircleCastAll(
			transform.position,
			GetComponent<Collider2D>().bounds.extents.x,
			Vector2.zero,
			float.MaxValue,
			1 << 13
			); //Time Markers

		if (_hits.Length > 0)
		{
			float closestXToSnapTo = float.MaxValue;

			for (int i = 0; i < _hits.Length; i++)
			{
				if (Mathf.Abs(transform.position.x - _hits[i].transform.position.x) < closestXToSnapTo)
				{
					closestXToSnapTo = _hits[i].transform.position.x;
				}
			}

			transform.position = new Vector3(
				closestXToSnapTo,
				transform.position.y,
				transform.position.z
				);
		}
	}

	private void _handleOverlappingPlacingRow()
    {
		//Check to see if we're hitting any placing rows
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, float.MaxValue, 1 << 11); //placing rows

		if (hit.collider == null)
		{
			if (SelectedPlacingRow != null)
			{
				SelectedPlacingRow.RemovePlaceableObjectTouching(gameObject);
				SelectedPlacingRow = null;
			}
		}
		else
		{
			bool wasNull = SelectedPlacingRow == null;

			SelectedPlacingRow = hit.transform.gameObject.GetComponent<PlacingRow>();

			if (wasNull)
			{
				SelectedPlacingRow.AddPlaceableObjectTouching(gameObject);
				SelectedPlacingRow.StartFlash();
			}

			transform.position = new Vector3(
				transform.position.x,
				SelectedPlacingRow.transform.position.y,
				transform.position.z
				);
		}
	}

	private void _handleInputMove(Vector3 input)
	{
		transform.position = new Vector3(input.x, input.y, transform.position.z);

		//lock X bounds
		if (transform.position.x < GameManager.LeftX + (GetComponent<Collider2D>().bounds.extents.x * 2))
		{
			transform.position = new Vector3(GameManager.LeftX + (GetComponent<Collider2D>().bounds.extents.x * 2),
			                                 transform.position.y,
			                                 transform.position.z);
		}
		else if(transform.position.x > GameManager.RightX - (GetComponent<Collider2D>().bounds.extents.x * 2))
		{
			transform.position = new Vector3(GameManager.RightX - (GetComponent<Collider2D>().bounds.extents.x * 2),
			                                 transform.position.y,
			                                 transform.position.z);
		}

		if (_levelEditorSceneManager.ShouldSnap)
		{
			_handleTimeMarkerSnap(input);
		}

		_handleOverlappingPlacingRow();

		_hits = Physics2D.CircleCastAll(
			transform.position, 
            GetComponent<Collider2D>().bounds.extents.x, 
            Vector2.zero,
            float.MaxValue, 
            1 << 12
			); //Placed Objects

		//check mouse as well
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, float.MaxValue, 1 << 12); //Placed Objects


		//if our new location would overlap some placed objects
		if(SelectedPlacingRow != null && (_hits.Length > 0 || hit.collider != null))
		{
			PlacedObject[] objs = FindObjectsOfType<PlacedObject>() as PlacedObject[];
			List<float> xes = new List<float>();

			//get the x positions of the objects on the same row
			for(int i = 0; i < objs.Length; i++)
			{
				if(objs[i].ObjectData.ObjectRow == SelectedPlacingRow.Row)
				{
					xes.Add(objs[i].transform.position.x);
				}
			}
			xes.Sort();

			//get which object comes first
			int leftMost = 0;
			int rightMost = 0;

			if(_hits.Length > 1)
			{
				if(_hits[0].transform.position.x < _hits[1].transform.position.x)
				{
					leftMost = xes.IndexOf(_hits[0].transform.position.x);
					rightMost = xes.IndexOf(_hits[1].transform.position.x);
				}
				else
				{
					leftMost = xes.IndexOf(_hits[1].transform.position.x);
					rightMost = xes.IndexOf(_hits[0].transform.position.x);
				}
			}
			else if(_hits.Length == 1)
			{
				leftMost = xes.IndexOf(_hits[0].transform.position.x);
				rightMost = xes.IndexOf(_hits[0].transform.position.x);
			}
			else //mouse hit
			{
				leftMost = xes.IndexOf(hit.transform.position.x);
				rightMost = xes.IndexOf(hit.transform.position.x);
			}

			for(int i = leftMost; i >= 1; i--)
			{
				if(xes[i] - xes[i - 1] <= GetComponent<Collider2D>().bounds.extents.x * 4)
				{
					leftMost--;
				}
				else break;
			}

			for(int i = rightMost; i < xes.Count - 1; i++)
			{
				if(xes[i + 1] - xes[i] <= GetComponent<Collider2D>().bounds.extents.x * 4)
				{
					rightMost++;
				}
				else break;
			}


			float avgX = 0;
			for(int i = leftMost; i <= rightMost; i++)
			{
				avgX += xes[i];
			}

			avgX /= (rightMost - leftMost) + 1;

			float finalX = 0;

			//if we're to the left of the average of the group
			if(input.x < avgX)
			{
				//if the leftMost has enough room to spawn on the left
				if(xes[leftMost] >= GameManager.LeftX + (GetComponent<Collider2D>().bounds.extents.x * 3))
				{
					finalX = xes[leftMost] - GetComponent<Collider2D>().bounds.extents.x * 2;
				}
				else if(xes[rightMost] <= GameManager.RightX - (GetComponent<Collider2D>().bounds.extents.x * 3))
				{
					finalX = xes[rightMost] + GetComponent<Collider2D>().bounds.extents.x * 2;
				}
				else
				{
					finalX = transform.position.x;

					//we can't place here so don't let place here
					SpaceToPlace = false;
				}
			}
			else if(input.x >= avgX)
			{
				if(xes[rightMost] <= GameManager.RightX - (GetComponent<Collider2D>().bounds.extents.x * 3))
				{
					finalX = xes[rightMost] + GetComponent<Collider2D>().bounds.extents.x * 2;
				}
				else if(xes[leftMost] >= GameManager.LeftX + (GetComponent<Collider2D>().bounds.extents.x * 3))
				{
					finalX = xes[leftMost] - GetComponent<Collider2D>().bounds.extents.x * 2;
				}
				else
				{
					finalX = transform.position.x;

					//we can't place here so don't let place here
					SpaceToPlace = false;
				}
			}

			transform.position = new Vector3(finalX, transform.position.y, transform.position.z);
		}
	}
}
