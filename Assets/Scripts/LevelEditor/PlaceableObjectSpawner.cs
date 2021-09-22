using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlaceableObjectSpawner : MonoBehaviour 
{
	public GameObject PlaceableObjectPrefab;
	public GameObject PlaceableObjectImagePrefab;
	public GameObject PlacedObjectPrefab;
	public GameObject PlacedObjectButtonPrefab;

	public ObjectPool<GameObject> PlaceableObjectsPool;
	public ObjectPool<GameObject> PlaceableObjectImagesPool;
	public ObjectPool<GameObject> PlacedObjectsPool;
	public ObjectPool<GameObject> PlacedObjectButtonsPool;

	public ObjectTypeEnum PlaceableObjectType;

	private LevelEditorManager _levelEditorManager;
	private GameManager _gameManager;

	public Button SpawnButton; //spawn button

	[SerializeField]
	private Sprite _sprite;

	[SerializeField]
	private Sprite _backingSprite;

	[SerializeField]
	private Transform _placedObjectButtonParent;

	[SerializeField]
	private Transform _placeableObjectImageParent;

	[SerializeField]
	private Image _uiBlocker; //used to block accidental button presses while placing object

	public void Initialize() 
	{
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		InputActionMap levelEditorInputActionMap = _gameManager.InputActions.FindActionMap("Level Editor");

		_levelEditorManager = GameObject.Find("LevelEditorManager").GetComponent<LevelEditorManager>();

		PlaceableObjectsPool = new ObjectPool<GameObject>();
		PlaceableObjectImagesPool = new ObjectPool<GameObject>();
		PlacedObjectsPool = new ObjectPool<GameObject>();
		PlacedObjectButtonsPool = new ObjectPool<GameObject>();

		
		PlaceableObjectsPool.Initialize(1, 
		                           new ObjectPool<GameObject>.InstantiateObjectDelegate(_instantiatePlaceableObject),
		                           new ObjectPool<GameObject>.DeactivateObjectDelegate(_deactivatePlaceableObjectMeat),
		                           new ObjectPool<GameObject>.ActivateObjectDelegate(_activatePlaceableObjectMeat));

		PlaceableObjectImagesPool.Initialize(1,
								   new ObjectPool<GameObject>.InstantiateObjectDelegate(_instantiatePlaceableObjectImage),
								   new ObjectPool<GameObject>.DeactivateObjectDelegate(_deactivatePlaceableObjectImageMeat),
								   new ObjectPool<GameObject>.ActivateObjectDelegate(_activatePlaceableObjectImageMeat));


		//note: the max needs to correspond with object pool maxes during gameplay
		PlacedObjectsPool.Initialize(Common.MaxPerObjectInEditor, 
		                        new ObjectPool<GameObject>.InstantiateObjectDelegate(_instantiatePlacedObject),
		                        new ObjectPool<GameObject>.DeactivateObjectDelegate(_deactivatePlacedObjectMeat),
		                        new ObjectPool<GameObject>.ActivateObjectDelegate(_activatePlacedObjectMeat));

		PlacedObjectButtonsPool.Initialize(Common.MaxPerObjectInEditor,
								new ObjectPool<GameObject>.InstantiateObjectDelegate(_instantiatePlacedObjectButton),
								new ObjectPool<GameObject>.DeactivateObjectDelegate(_deactivatePlacedObjectButtonMeat),
								new ObjectPool<GameObject>.ActivateObjectDelegate(_activatePlacedObjectButtonMeat));

		SpawnButton.onClick.AddListener(_handleButtonClicked);
	}

	private GameObject _instantiatePlaceableObject()
	{
		GameObject obj = (GameObject)Instantiate(PlaceableObjectPrefab);
		obj.transform.parent = transform;
		obj.GetComponent<PlaceableObject>().ObjectType = PlaceableObjectType;
		obj.GetComponent<CircleCollider2D>().radius = .5f; //.5f for a lil gap
		obj.GetComponent<PlaceableObject>().Initialize();
		obj.SetActive(false);

		return obj;
	}
	private GameObject _instantiatePlaceableObjectImage()
	{
		GameObject obj = (GameObject)Instantiate(PlaceableObjectImagePrefab);
		obj.transform.parent = _placeableObjectImageParent;
		obj.transform.localScale = Vector3.one;

		PlaceableObjectImage placeableObjectImage = obj.GetComponent<PlaceableObjectImage>();
		placeableObjectImage.Image.sprite = _sprite;
		obj.SetActive(false);
		return obj;
	}

	private GameObject _instantiatePlacedObject()
	{
		GameObject obj = (GameObject)Instantiate(PlacedObjectPrefab);
		obj.transform.parent = transform;
		obj.GetComponent<CircleCollider2D>().radius = .5f; //.5f for a lil gap
		obj.GetComponent<PlacedObject>().Initialize(this);
		obj.SetActive(false);
		return obj;
	}

	private GameObject _instantiatePlacedObjectButton()
	{
		GameObject obj = (GameObject)Instantiate(PlacedObjectButtonPrefab);
		obj.transform.parent = _placedObjectButtonParent;
		obj.transform.localScale = Vector3.one;

		PlacedObjectButton placedObjectButton = obj.GetComponent<PlacedObjectButton>();
		placedObjectButton.Image.sprite = _sprite;
		placedObjectButton.BackingImage.sprite = _backingSprite;
		obj.SetActive(false);
		return obj;
	}

	private Vector3 _activatePlaceableObjectPos;

	public PlaceableObject ActivatePlaceableObject(Vector3 pos)
	{
		//Define vars that will be used on callback
		_activatePlaceableObjectPos = new Vector3(pos.x, pos.y, 0);
		PlaceableObject placeableObject = PlaceableObjectsPool.ActivateObject()?.GetComponent<PlaceableObject>();

		if (placeableObject != null)
		{
			PlaceableObjectImage placeableObjectImage = PlaceableObjectImagesPool.ActivateObject()?.GetComponent<PlaceableObjectImage>();
			placeableObjectImage.transform.position = Camera.main.WorldToScreenPoint(placeableObject.transform.position);
			placeableObject.PlaceableObjectImage = placeableObjectImage;
		}

		return placeableObject;
	}

	private Vector3 _activatePlacedObjectPos;
	private ObjectData _activatePlacedObjectData;

	public PlacedObject ActivatePlacedObject(Vector3 pos, ObjectData data)
	{
		//Define vars that will be used on callback
		_activatePlacedObjectPos = pos; 
		_activatePlacedObjectData = data;
		PlacedObject placedObject = PlacedObjectsPool.ActivateObject()?.GetComponent<PlacedObject>();

		if (placedObject != null)
		{
			PlacedObjectButton placedObjectButton = PlacedObjectButtonsPool.ActivateObject()?.GetComponent<PlacedObjectButton>();
			placedObjectButton.transform.position = Camera.main.WorldToScreenPoint(placedObject.transform.position);

			placedObject.PlacedObjectButton = placedObjectButton;
		}

		return placedObject;
	}

	//Will get called in the middle of placeableObjectsPool.activateObject();
	private void _activatePlaceableObjectMeat(ref GameObject obj) //TODO: could get rid of the meat shit
	{
		obj.SetActive(true);
		obj.GetComponent<PlaceableObject>().Activate(_activatePlaceableObjectPos);
	}

	private void _activatePlaceableObjectImageMeat(ref GameObject obj)
	{
		obj.SetActive(true);
		_uiBlocker.gameObject.SetActive(true);
	}

	//Will get called in the middle of placedObjectsPool.activateObject();
	private void _activatePlacedObjectMeat(ref GameObject obj)
	{
		obj.SetActive(true);
		obj.GetComponent<PlacedObject>().Activate(_activatePlacedObjectPos, _activatePlacedObjectData);
	}

	private void _activatePlacedObjectButtonMeat(ref GameObject obj)
	{
		obj.SetActive(true);
	}

	private void _deactivatePlaceableObjectMeat(ref GameObject obj)
	{
		PlaceableObject placeableObject = obj.GetComponent<PlaceableObject>();
		PlaceableObjectImagesPool.DeactivateObject(placeableObject.PlaceableObjectImage.gameObject);
		placeableObject.PlaceableObjectImage = null;
		obj.SetActive(false);
		_uiBlocker.gameObject.SetActive(false);
	}

	private void _deactivatePlaceableObjectImageMeat(ref GameObject obj)
	{
		obj.SetActive(false);
	}

	private void _deactivatePlacedObjectMeat(ref GameObject obj)
	{
        PlacedObject placedObject = obj.GetComponent<PlacedObject>();
		PlacedObjectButtonsPool.DeactivateObject(placedObject.PlacedObjectButton.gameObject);
		placedObject.PlacedObjectButton = null;
		obj.SetActive(false);
	}

	private void _deactivatePlacedObjectButtonMeat(ref GameObject obj)
	{
		obj.SetActive(false);
		obj.GetComponent<PlacedObjectButton>().Deactivate();
	}

	private GameObject _temp;
	public void MoveActivePlacedObjects(float deltaX)
	{
		int count = PlacedObjectsPool.activeObjects.Count;
		for(int i = count - 1; i >= 0; i--)
		{
			_temp = PlacedObjectsPool.activeObjects[i];
			
			_temp.transform.position += new Vector3(deltaX,0,0);
			_temp.GetComponent<PlacedObject>().PlacedObjectButton.transform.position = Camera.main.WorldToScreenPoint(_temp.transform.position);

			if ((_temp.transform.position.x < GameManager.LeftX //- temp.renderer.bounds.extents.x
			    && deltaX < 0)
			   || (_temp.transform.position.x > GameManager.RightX //+ temp.renderer.bounds.extents.x
			    && deltaX > 0))
			{
				_levelEditorManager.DeactivatePlacedObject(this, _temp);
			}
		}

		_temp = null;
	}

	private void _handleButtonClicked()
	{
		bool shouldSpawn = _levelEditorManager.OnSpawnButtonPressed(PlaceableObjectType);

		if (!shouldSpawn)
			return;

		PlaceableObject placeableObject = ActivatePlaceableObject(Camera.main.ScreenToWorldPoint(SpawnButton.transform.position));

		_levelEditorManager.OnPlaceableObjectSpawned(placeableObject);
	}

	public void Run(Vector2 input)
	{
		if (input == Vector2.zero)
			return;

		for(int i = PlaceableObjectsPool.activeObjects.Count - 1; i >= 0; i--)
		{
			PlaceableObjectsPool.activeObjects[i].GetComponent<PlaceableObject>().Run(input);
		}
	}
}
