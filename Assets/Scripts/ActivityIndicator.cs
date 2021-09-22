using UnityEngine;
using System.Collections;

public class ActivityIndicator : MonoBehaviour
{
	public Vector3 originalPos;
	public float radius = .03f;

	float time = 0f;
	public float rotationSpeed;

	// Use this for initialization
	void Start () 
	{
		originalPos = transform.position;
	}

	public void SetOriginalPos(Vector3 oP)
	{
		originalPos = oP;
		Debug.Log(originalPos);
	}

	public void Move() 
	{
		time += rotationSpeed / GameManager.WorldWidth;
		transform.position = originalPos + new Vector3(Mathf.Sin(time), Mathf.Cos(time), 0) * radius;
	}
}
