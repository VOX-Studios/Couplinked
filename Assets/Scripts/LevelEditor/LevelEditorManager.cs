using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Assets.Scripts.Gameplay;

public static class ExtensionMethods
{
	public static float RoundToInterval (this float f, float interval)
	{
		return (Mathf.Round(f / interval)) * interval;
	}
}

public class LevelEditorManager : MonoBehaviour 
{
	private PlaceableObjectSpawner _noHitPlaceableObjectSpawner;
	private PlaceableObjectSpawner _hit1PlaceableObjectSpawner;
	private PlaceableObjectSpawner _hit2PlaceableObjectSpawner;
	private PlaceableObjectSpawner _hitSplit1PlaceableObjectSpawner;
	private PlaceableObjectSpawner _hitSplit2PlaceableObjectSpawner;

	public GameObject TimeMarkerPrefab;
	private List<GameObject> _timeMarkers;
	private int _numTimeMarkersNeeded;
	private float _secondsPerTimeMarker = .25f;


	private float _timeAtLeftScreen;
	private int _leftIndex;
	private int _rightIndex;

	private GameManager _gameManager;

	private bool _isDragging;
	private Text _txtTime;

	//mouse
	private Vector3 _lastDragInputPosition;

	private Camera _cam;

	/// <summary>
	/// How many seconds the length of the screen represents
	/// </summary>
	private float _timeLengthOfScreen;

	/// <summary>
	/// True when an object is currently being placed
	/// </summary>
	private bool _isPlacingObject = false;
	private ObjectTypeEnum? _placeableObjectType;

	private InputAction _selectInputAction;
	private InputAction _scrollInputAction;
	private InputAction _rightStickInputAction;
	private InputAction _leftStickInputAction;
	private InputAction _gamepadActivateInputAction;

	private InputAction _mouseDeltaInputAction;
	private Vector2 _phantomGamepadCursorPosition;
	private bool _usingGamepad;

	[SerializeField]
	private Image _dragArea;

	/// <summary>
	/// This is used to solve issues with UI button press working with Input System
	/// </summary>
	private bool _selectStarted;
	private bool _objectWasSpawned;

	private float[] _rowPositions;
	private float _scale;

	public void Initialize(List<Transform> placeholderPositions)
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		_gameManager.GameDifficultyManager.ChangeDifficulty(GameDifficultyEnum.Hard);

		InputActionMap levelEditorInputActionMap = _gameManager.InputActions.FindActionMap("Level Editor");
		_selectInputAction = levelEditorInputActionMap.FindAction("Select");
		_scrollInputAction = levelEditorInputActionMap.FindAction("Scroll");
		_rightStickInputAction = levelEditorInputActionMap.FindAction("RightStick");
		_leftStickInputAction = levelEditorInputActionMap.FindAction("LeftStick");
		_mouseDeltaInputAction = levelEditorInputActionMap.FindAction("MouseDelta");
		_gamepadActivateInputAction = levelEditorInputActionMap.FindAction("GamepadActivate");

		_rowPositions = RowPositionsUtility.CalculateRowPositions(_gameManager.CurrentLevel.NumberOfRows);
		_scale = ScaleUtility.CalculateScale(_gameManager.CurrentLevel.NumberOfRows);
		_timeLengthOfScreen = GameManager.WorldWidth / (_gameManager.GameDifficultyManager.ObjectSpeed * _scale);

		_timeMarkers = new List<GameObject>();

		_computeNumberOfTimeMarkers();

		_leftIndex = 0;
		_rightIndex = -1;
		_cam = Camera.main;

		PlacingRow[] rows = FindObjectsOfType<PlacingRow>();

		for(int i = 0; i < rows.Length; i++)
		{
			rows[i].transform.position = new Vector3(rows[i].transform.position.x, _rowPositions[rows[i].Row], rows[i].transform.position.z);
		}

		_timeAtLeftScreen = 0f;
		_isDragging = false;

		_txtTime = GameObject.Find("txtTime").GetComponent<Text>();
		_txtTime.text = _timeAtLeftScreen.ToString();

		_noHitPlaceableObjectSpawner = GameObject.Find("NoHitPlaceableObjectSpawner").GetComponent<PlaceableObjectSpawner>();
		_hit1PlaceableObjectSpawner = GameObject.Find("Hit1PlaceableObjectSpawner").GetComponent<PlaceableObjectSpawner>();
		_hit2PlaceableObjectSpawner = GameObject.Find("Hit2PlaceableObjectSpawner").GetComponent<PlaceableObjectSpawner>();
		_hitSplit1PlaceableObjectSpawner = GameObject.Find("HitSplit1PlaceableObjectSpawner").GetComponent<PlaceableObjectSpawner>();
		_hitSplit2PlaceableObjectSpawner = GameObject.Find("HitSplit2PlaceableObjectSpawner").GetComponent<PlaceableObjectSpawner>();

		//Move spawners to bottom
		_noHitPlaceableObjectSpawner.transform.position = _convertFromPlaceholderPosition(placeholderPositions[0].position);
		_hit1PlaceableObjectSpawner.transform.position = _convertFromPlaceholderPosition(placeholderPositions[1].position);
		_hit2PlaceableObjectSpawner.transform.position = _convertFromPlaceholderPosition(placeholderPositions[2].position);
		_hitSplit1PlaceableObjectSpawner.transform.position = _convertFromPlaceholderPosition(placeholderPositions[3].position);
		_hitSplit2PlaceableObjectSpawner.transform.position = _convertFromPlaceholderPosition(placeholderPositions[4].position);

		_noHitPlaceableObjectSpawner.Initialize();
		_hit1PlaceableObjectSpawner.Initialize();
		_hit2PlaceableObjectSpawner.Initialize();
		_hitSplit1PlaceableObjectSpawner.Initialize();
		_hitSplit2PlaceableObjectSpawner.Initialize();

		//TODO: Make this way more effecient
		for(int i = 0; i < _gameManager.CurrentLevel.Data.Count; i++)
		{
			if(_gameManager.CurrentLevel.Data[i].Time >= _timeAtLeftScreen 
			   && _gameManager.CurrentLevel.Data[i].Time <= _timeAtLeftScreen + _timeLengthOfScreen)
			{
				ObjectData objectData = _gameManager.CurrentLevel.Data[i];
				objectData.Time /= _gameManager.GameDifficultyManager.GameTimeModifier;
				_rightIndex++;

				_activatePlacedObject(objectData);
			}
		}

		_setupDragArea();
		_setupInteractions();
	}

	private void _computeNumberOfTimeMarkers()
	{
		_secondsPerTimeMarker = _timeLengthOfScreen / 9.9f;
		_numTimeMarkersNeeded = (int)Mathf.Ceil(_timeLengthOfScreen / _secondsPerTimeMarker);
	}

	private void _setupDragArea()
	{
		EventTrigger trigger = _dragArea.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.BeginDrag;
		entry.callback.AddListener((data) => 
		{ 
			_handleBeginDrag((PointerEventData)data); 
		});
		trigger.triggers.Add(entry);
	}

	private void _setupInteractions()
    {
        _selectInputAction.started += _selectInputAction_started;
        _selectInputAction.performed += _selectInputAction_performed;
	}

    private void _selectInputAction_started(InputAction.CallbackContext obj)
    {
		_selectStarted = true;
	}

    private void _selectInputAction_performed(InputAction.CallbackContext obj)
    {
		_selectStarted = false;
		if (!_objectWasSpawned)
		{
			_handleSelectActionTriggered();
		}
		_objectWasSpawned = false;
	}

    private void _handleBeginDrag(PointerEventData data)
	{
		if (_isPlacingObject || data.button != PointerEventData.InputButton.Left)
			return;

		Vector3 inputPos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		
		_isDragging = true;
		_lastDragInputPosition = inputPos;
	}

	private void _handleTimeMarkers()
	{
		int i = _timeMarkers.Count - 1;
		while (_timeMarkers.Count > _numTimeMarkersNeeded)
		{
			_timeMarkers.RemoveAt(i);
			i--;
		}

		while (_timeMarkers.Count < _numTimeMarkersNeeded)
		{
			GameObject timeMarker = (GameObject)Instantiate(
				TimeMarkerPrefab,
				new Vector3(0, (GameManager.TopY + GameManager.BotY) / 2, 0),
				TimeMarkerPrefab.transform.rotation
				);
			SpriteRenderer sr = timeMarker.GetComponent<SpriteRenderer>();

			float width = sr.sprite.bounds.size.x;

			float worldScreenHeight = GameManager.TopY - GameManager.BotY;

			float xScale = worldScreenHeight / width;


			timeMarker.transform.localScale = new Vector3(xScale,
														  timeMarker.transform.localScale.y,
														  timeMarker.transform.localScale.z);
			_timeMarkers.Add(timeMarker);
		}

		float startX = GameManager.LeftX;
		startX += ((_timeAtLeftScreen.RoundToInterval(_secondsPerTimeMarker) - _timeAtLeftScreen) / _timeLengthOfScreen) * GameManager.WorldWidth;

		float xSpacing = (_secondsPerTimeMarker / _timeLengthOfScreen) * GameManager.WorldWidth;

		for (i = 0; i < _timeMarkers.Count; i++)
		{
			_timeMarkers[i].transform.position = new Vector3(startX + (i * xSpacing),
															_timeMarkers[i].transform.position.y,
															0);
		}
	}

	private Vector3 _convertFromPlaceholderPosition(Vector3 placeholderPosition)
    {
		Vector3 rawConverted = Camera.main.ScreenToWorldPoint(placeholderPosition);
		return new Vector3(rawConverted.x, rawConverted.y);
	}
	
	public void Run() 
	{
		if(_usingGamepad && _mouseDeltaInputAction.phase == InputActionPhase.Started)
        {
			_usingGamepad = false;

			EventSystem.current.SetSelectedGameObject(null);
		}

		if(!_usingGamepad && _gamepadActivateInputAction.phase == InputActionPhase.Started)
        {
			_usingGamepad = true;

			if(!_isPlacingObject)
				EventSystem.current.SetSelectedGameObject(_noHitPlaceableObjectSpawner.SpawnButton.gameObject);
		}

		_handleTimeMarkers();

		Vector2 input = Vector2.zero;
		if (_isPlacingObject)
		{
			
			if (_mouseDeltaInputAction.phase == InputActionPhase.Started)
			{
				input = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
				_phantomGamepadCursorPosition = input;
			}
			else if (_leftStickInputAction.phase == InputActionPhase.Started)
			{
				_phantomGamepadCursorPosition += _leftStickInputAction.ReadValue<Vector2>() / 10;
				input = _phantomGamepadCursorPosition;
			}

			_clampPhantomGamepadCursorPosition();
		}

		_noHitPlaceableObjectSpawner.Run(input);
		_hit1PlaceableObjectSpawner.Run(input);
		_hit2PlaceableObjectSpawner.Run(input);
		_hitSplit1PlaceableObjectSpawner.Run(input);
		_hitSplit2PlaceableObjectSpawner.Run(input);

		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			_handleInputUp();
		}

		if (_isDragging)
		{
			Vector3 inputPos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			Vector3 delta = inputPos - _lastDragInputPosition;
			HandleDragging(delta);
			_lastDragInputPosition = inputPos;
		}
		else if(_scrollInputAction.triggered)
        {
			float scrollValue = _scrollInputAction.ReadValue<float>();
			
			if (scrollValue > 0)
				scrollValue = 1.5f;
			else
				scrollValue = -1.5f;

			Vector3 delta = new Vector3(scrollValue, 0);
			HandleDragging(delta);
		}
		else
        {
			float stickValue = _rightStickInputAction.ReadValue<float>();
			if (Mathf.Abs(stickValue) >= .4f)
			{
				Vector3 delta = new Vector3(-stickValue / 10, 0);
				HandleDragging(delta);
			}
		}
	}

	private void _clampPhantomGamepadCursorPosition()
	{
		// Clamp top
		if (_phantomGamepadCursorPosition.y > GameManager.TopY)
		{
			_phantomGamepadCursorPosition = new Vector2(
				_phantomGamepadCursorPosition.x,
				GameManager.TopY
				);
		}
		else if (_phantomGamepadCursorPosition.y < GameManager.BotY) // Clamp bottom
		{
			_phantomGamepadCursorPosition = new Vector2(
				_phantomGamepadCursorPosition.x,
				GameManager.BotY
				);
		}

		// Clamp left
		if (_phantomGamepadCursorPosition.x < GameManager.LeftX)
		{
			_phantomGamepadCursorPosition = new Vector2(
				GameManager.LeftX,
				_phantomGamepadCursorPosition.y
				);
		}
		else if (_phantomGamepadCursorPosition.x > GameManager.RightX) // Clamp right
		{
			_phantomGamepadCursorPosition = new Vector2(
				GameManager.RightX,
				_phantomGamepadCursorPosition.y
				);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="placeableObjectType"></param>
	/// <returns>True if a placeable object should be spawned.</returns>
	public bool OnSpawnButtonPressed(ObjectTypeEnum placeableObjectType)
    {
		//if we were already placing an object
		if (_isPlacingObject)
        {
            //if we were already placing this object type
            if (placeableObjectType == _placeableObjectType)
            {
				//stop placing that object
				_removeAllActivePlaceableObjects();

				_isPlacingObject = false;
				_placeableObjectType = null;

				return false;
            }

			//stop placing that object
			_removeAllActivePlaceableObjects();
            _placeableObjectType = placeableObjectType;
			EventSystem.current.SetSelectedGameObject(null); //TODO: cache what was selected previously?
			if (_selectStarted)
			{
				_objectWasSpawned = true;
			}
			return true;
		}

		_isPlacingObject = true;
		_placeableObjectType = placeableObjectType;
		EventSystem.current.SetSelectedGameObject(null); //TODO: cache what was selected previously?
		if (_selectStarted)
		{
			_objectWasSpawned = true;
		}
		return true;
	}

	public void OnPlaceableObjectSpawned(PlaceableObject placeableObject)
	{
		_phantomGamepadCursorPosition = placeableObject.transform.position;
	}

	private void _removeAllActivePlaceableObjects()
    {
		//stop placing object
		PlaceableObject[] po = FindObjectsOfType<PlaceableObject>();

		//Loop through all of the active placeableObjects
		for (int i = po.Length - 1; i >= 0; i--)
		{
			po[i].transform.parent.gameObject.GetComponent<PlaceableObjectSpawner>().PlaceableObjectsPool.DeactivateObject(po[i].gameObject);
		}
	}

	private void _handleSelectActionTriggered()
    {
		if (!_isPlacingObject)
		{
			return;
		}

        PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = Mouse.current.position.ReadValue();
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);

		for (int i = 0; i < results.Count; i++)
		{
			if (results[i].gameObject.tag == "PlaceableObjectSpawner")
			{
				return;
			}
		}

		PlaceableObject[] po = FindObjectsOfType<PlaceableObject>();

		bool didPlaceObject = false;

		//Loop through all of the active placeableObjects
		for (int i = po.Length - 1; i >= 0; i--)
		{
			PlaceableObject placeableObjectComponment = po[i];

			//if we've snapped to a placing row
			if (placeableObjectComponment.SpaceToPlace && placeableObjectComponment.SelectedPlacingRow != null)
			{
				//Calculate the time the object would appear at
				float offset = _timeAtLeftScreen
					+ ((po[i].transform.position.x - GameManager.LeftX) / GameManager.WorldWidth) * _timeLengthOfScreen;

				ObjectData objData = new ObjectData()
				{
					Time = offset,
					ObjectRow = placeableObjectComponment.SelectedPlacingRow.Row,
					ObjectType = placeableObjectComponment.ObjectType
				};

				PlacedObject placedObject = _placeAndAddObjectDataToList(objData);

				if(placedObject != null)
                {
					didPlaceObject = true;

					if (_usingGamepad)
					{
						EventSystem.current.SetSelectedGameObject(placedObject.PlacedObjectButton.Button.gameObject);
					}
				}
			}

			po[i].transform.parent.gameObject.GetComponent<PlaceableObjectSpawner>().PlaceableObjectsPool.DeactivateObject(po[i].gameObject);

			if(!didPlaceObject && _usingGamepad)
            {
				EventSystem.current.SetSelectedGameObject(_noHitPlaceableObjectSpawner.SpawnButton.gameObject);
			}
		}

		_isPlacingObject = false;
		_placeableObjectType = null;
	}

	private void _handleInputUp()
	{
		//End the dragging
		_isDragging = false;
	}

	void HandleDragging(Vector3 delta)
	{			
		float timeChange = -(delta.x / GameManager.WorldWidth) * _timeLengthOfScreen;
			
		if(timeChange + _timeAtLeftScreen < 0)
		{
			delta.x += ((timeChange + _timeAtLeftScreen) / _timeLengthOfScreen) * GameManager.WorldWidth;
			timeChange -= timeChange + _timeAtLeftScreen;
		}
			
		_timeAtLeftScreen += timeChange;
			
		_noHitPlaceableObjectSpawner.MoveActivePlacedObjects(delta.x);
		_hit1PlaceableObjectSpawner.MoveActivePlacedObjects(delta.x);
		_hit2PlaceableObjectSpawner.MoveActivePlacedObjects(delta.x);
		_hitSplit1PlaceableObjectSpawner.MoveActivePlacedObjects(delta.x);
		_hitSplit2PlaceableObjectSpawner.MoveActivePlacedObjects(delta.x);
			
		if(_gameManager.CurrentLevel.Data.Count > 0)
		{
			if(delta.x > 0)
			{
				//TODO: Compensate for half width
				if(_leftIndex >= 0 
					&& _gameManager.CurrentLevel.Data[_leftIndex - ((_leftIndex > 0) ? 1 : 0)].Time > _timeAtLeftScreen
					&& _gameManager.CurrentLevel.Data[_leftIndex - ((_leftIndex > 0) ? 1 : 0)].Time < _timeAtLeftScreen + _timeLengthOfScreen)
				{
					PlacedObject[] placedObjects = GameObject.FindObjectsOfType<PlacedObject>();
					bool canSpawn = true;
					for(int i = 0; i < placedObjects.Length; i++)
					{
						if(placedObjects[i].ObjectData == _gameManager.CurrentLevel.Data[_leftIndex - ((_leftIndex > 0)? 1 : 0)])
						{
							canSpawn = false;
							break;
						}
					}

					if(canSpawn)
					{
						_leftIndex--;

						_activatePlacedObject(_gameManager.CurrentLevel.Data[_leftIndex]);
					}
				}
			}
			else if(delta.x < 0)
			{
				if(_rightIndex <= _gameManager.CurrentLevel.Data.Count - 1 
					&& _gameManager.CurrentLevel.Data[_rightIndex + ((_rightIndex < _gameManager.CurrentLevel.Data.Count - 1 )? 1 : 0)].Time < _timeAtLeftScreen + _timeLengthOfScreen
					&&  _gameManager.CurrentLevel.Data[_rightIndex + ((_rightIndex < _gameManager.CurrentLevel.Data.Count - 1 )? 1 : 0)].Time > _timeAtLeftScreen)
				{

					PlacedObject[] POs = GameObject.FindObjectsOfType<PlacedObject>() as PlacedObject[];
					bool canSpawn = true;
					for(int i = 0; i < POs.Length; i++)
					{
						if(POs[i].ObjectData == _gameManager.CurrentLevel.Data[_rightIndex + ((_rightIndex < _gameManager.CurrentLevel.Data.Count - 1 ) ? 1 : 0)])
						{
							canSpawn = false;
							break;
						}
					}
						
					if(canSpawn)
					{
						if(_rightIndex < _gameManager.CurrentLevel.Data.Count - 1) 
							_rightIndex++;

						_activatePlacedObject(_gameManager.CurrentLevel.Data[_rightIndex]);
					}
				}
			}
		}
			
		_txtTime.text = _timeAtLeftScreen.ToString();
	}
	private PlacedObject _activatePlacedObject(ObjectData data)
	{
		switch (data.ObjectType)
		{
			case ObjectTypeEnum.NoHit:
				return _activatePlacedObject(_noHitPlaceableObjectSpawner, data);
			case ObjectTypeEnum.Hit1:
				return _activatePlacedObject(_hit1PlaceableObjectSpawner, data);
			case ObjectTypeEnum.Hit2:
				return _activatePlacedObject(_hit2PlaceableObjectSpawner, data);
			case ObjectTypeEnum.HitSplit1:
				return _activatePlacedObject(_hitSplit1PlaceableObjectSpawner, data);
			case ObjectTypeEnum.HitSplit2:
				return _activatePlacedObject(_hitSplit2PlaceableObjectSpawner, data);
		}

		return null;
	}

	private PlacedObject _activatePlacedObject(PlaceableObjectSpawner spawner, ObjectData data)
	{
		PlacingRow[] rows = FindObjectsOfType<PlacingRow>();

		float y = 0;
		for(int i = 0; i < rows.Length; i++)
		{
			if(rows[i].Row == data.ObjectRow)
			{
				y = rows[i].transform.position.y;
				break;
			}
		}

		float offset = GameManager.LeftX + ((data.Time - _timeAtLeftScreen) / _timeLengthOfScreen) * GameManager.WorldWidth;
        PlacedObject placedObject = spawner.ActivatePlacedObject(new Vector3(offset, y, 0), data);

		placedObject?.PlacedObjectButton.Button.onClick.AddListener(() => _movePlacedObject(placedObject));

		return placedObject;
	}

	public void DeactivatePlacedObject(PlaceableObjectSpawner spawner, GameObject po)
	{
		if(po.transform.position.x <= GameManager.LeftX)
		{
			spawner.PlacedObjectsPool.DeactivateObject(po);
			_leftIndex++;
		}
		else if(po.transform.position.x >= GameManager.RightX)
		{
			spawner.PlacedObjectsPool.DeactivateObject(po);
			_rightIndex--;
		}
	}

	private void _movePlacedObject(PlacedObject placedObject)
	{
		//set to null so our highlights show up properly
		EventSystem.current.SetSelectedGameObject(null);

		_gameManager.CurrentLevel.Data.Remove(placedObject.ObjectData);
		placedObject.Spawner.PlacedObjectsPool.DeactivateObject(placedObject.gameObject);

		placedObject.Spawner.ActivatePlaceableObject(placedObject.transform.position);
		_phantomGamepadCursorPosition = placedObject.transform.position;

		_isPlacingObject = true;
		_placeableObjectType = placedObject.ObjectData.ObjectType;
		
		if (_selectStarted)
		{
			_objectWasSpawned = true;
		}

		//don't need to move left index because list will slide left
		//	and left index will stay the same

		//take away from right index if we can
		if (_rightIndex > 0) 
			_rightIndex--;
	}

	private PlacedObject _placeAndAddObjectDataToList(ObjectData objData)
	{
		int insertBefore = -1;
		//TODO: Could better implement with a binary search
		for (int i = 0; i < _gameManager.CurrentLevel.Data.Count; i++)
		{
			if (objData.Time <= _gameManager.CurrentLevel.Data[i].Time)
			{
				insertBefore = i;
				break;
			}
		}

		PlacedObject placedObject = _activatePlacedObject(objData);

		if (placedObject == null)
		{
			_gameManager.NotificationManager.MiddleOfSceneActivation("Too many similar objects nearby.");
			_gameManager.SoundEffectManager.PlayBack();
			return placedObject;
		}

		//increment the right index since we've added an object
		_rightIndex++;

		//insert into data
		if (insertBefore == -1)
			_gameManager.CurrentLevel.Data.Add(objData);
		else
			_gameManager.CurrentLevel.Data.Insert(insertBefore, objData);

		return placedObject;
	}

	public bool OnExitButtonPressed(InputAction exitInputAction)
    {
		if(_isPlacingObject)
        {

			_removeAllActivePlaceableObjects();
			switch (_placeableObjectType)
			{
				case ObjectTypeEnum.NoHit:
					EventSystem.current.SetSelectedGameObject(_noHitPlaceableObjectSpawner.SpawnButton.gameObject);
					break;
				case ObjectTypeEnum.Hit1:
					EventSystem.current.SetSelectedGameObject(_hit1PlaceableObjectSpawner.SpawnButton.gameObject);
					break;
				case ObjectTypeEnum.Hit2:
					EventSystem.current.SetSelectedGameObject(_hit2PlaceableObjectSpawner.SpawnButton.gameObject);
					break;
				case ObjectTypeEnum.HitSplit1:
					EventSystem.current.SetSelectedGameObject(_hitSplit1PlaceableObjectSpawner.SpawnButton.gameObject);
					break;
				case ObjectTypeEnum.HitSplit2:
					EventSystem.current.SetSelectedGameObject(_hitSplit2PlaceableObjectSpawner.SpawnButton.gameObject);
					break;
				default:
					Debug.LogError($"Don't know how to handle {_placeableObjectType}");
					break;
			}

			_isPlacingObject = false;
			_placeableObjectType = null;
			return false;
        }

		_dragArea.GetComponent<EventTrigger>().triggers.Clear();
		_selectInputAction.started -= _selectInputAction_started;
		_selectInputAction.performed -= _selectInputAction_performed;

		return true;
	}
}