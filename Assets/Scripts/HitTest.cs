using UnityEngine;

public class HitTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter2D(Collision2D coll) 
	{
		Debug.Log ("WORK");
	}

	void OnTriggerEnter2D(Collider2D other) 
	{
		Debug.Log("Trigger");
	}
}
