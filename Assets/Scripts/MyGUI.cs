using UnityEngine;
using UnityEngine.UI;

public class MyGUI : MonoBehaviour 
{
	public Text txtScore;
	GameManager gameManager;

	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		txtScore.text = gameManager.Score.ToString();
	}

	void OnGUI()
	{
		txtScore.text = gameManager.Score.ToString();
	}
}
