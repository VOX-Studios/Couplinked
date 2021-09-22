using UnityEngine;
using UnityEngine.UI;

public class GarbageBin : MonoBehaviour 
{
	public bool DeleteIsActive = false;
	float time = 0f;
	float pulseSpeed = 4f;
	float maxScaleIncrease = .25f;

	private Color originalColor;

	void Start()
	{
		originalColor = GetComponent<Image>().color;
	}

	public void Toggle()
	{
		if(DeleteIsActive)
		{
			Deactivate();
		}
		else
		{
			Activate();
		}
	}

	void Activate()
	{
		DeleteIsActive = true;
		GetComponent<Image>().color = Color.white;
	}

	void Deactivate()
	{
		time = 0f;
		DeleteIsActive = false;
		GetComponent<Image>().color = originalColor;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(DeleteIsActive)
		{
			Pulse();
		}
	}

	private void Pulse() 
	{
		time += pulseSpeed * Time.deltaTime;
		float scaleFactor = Mathf.Abs(Mathf.Sin(time)) * maxScaleIncrease;
		scaleFactor++;
        GetComponent<Image>().transform.localScale = new Vector2(scaleFactor, scaleFactor);
	}
}
