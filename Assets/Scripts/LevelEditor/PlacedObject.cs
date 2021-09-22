using UnityEngine;

public class PlacedObject : MonoBehaviour 
{
	public ObjectData ObjectData;
	public PlaceableObjectSpawner Spawner { get; private set; }

	public PlacedObjectButton PlacedObjectButton;

	public void Initialize(PlaceableObjectSpawner spawner)
	{
		Spawner = spawner;
	}
	
	public void Activate(Vector3 position, ObjectData objectData) 
	{
		transform.position = position;
		ObjectData = objectData;
	}
}
