using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
	private GameManager _gameManager;
	public Queue<string> Messages;
	public GameObject NotificationPrefab;
	public GameObject Notification;
	private Text TheText;

	public bool IsActive;

	public bool IsRequested;

	public bool IsOpening;
	public bool IsClosing;

	public void Initialize(GameManager gameManager) 
	{
		_gameManager = gameManager;

		Messages = new Queue<string>();
		if(Notification == null)
		{
			Notification = (GameObject)Instantiate(NotificationPrefab);
			Notification.transform.SetParent(transform, false);
			TheText = Notification.GetComponentInChildren<Text>();

			deactivate();
		}
	}

	public void QueueNotification(string text)
	{
		IsRequested = true;
		Messages.Enqueue(text);
	}

	public void Activate()
	{
		if(IsRequested)
		{
			activate();
		}
	}

	public void MiddleOfSceneActivation(string text)
	{
		Messages.Enqueue(text);
		activate();
	}

	private void activate()
	{
		Notification.SetActive(true);
		TheText.color = new Color(
			TheText.color.r,
		    TheText.color.g,
		    TheText.color.b,
		    0
		);
		
		IsRequested = false;
		
		IsOpening = true;
		IsActive = true;
		TheText.text = Messages.Dequeue();

		_gameManager.InputManager.ToggleInputs(_gameManager.AppState, IsActive);
	}

	public void Close()
	{
		IsClosing = true;
	}

	private void deactivate()
	{
		IsActive = false;
		Notification.SetActive(false);
		_gameManager.InputManager.ToggleInputs(_gameManager.AppState, IsActive);
	}

	private float _transitionStep = 5f;
	public void Run(float deltaTime)
	{
		if (IsOpening)
		{
			if(TheText.color.a < 1)
			{
				TheText.color = new Color(
					TheText.color.r,
				    TheText.color.g,
				    TheText.color.b,
				    TheText.color.a + (_transitionStep * deltaTime)
					);
			}
			else
			{
				TheText.color = new Color(
					TheText.color.r,
				    TheText.color.g,
				    TheText.color.b,
				    1
				);
			}

			if (TheText.color.a >= 1)
			{
				IsOpening = false;
			}
		}
		else if(IsClosing)
		{
			if(TheText.color.a > 0)
			{
				TheText.color = new Color(
					TheText.color.r,
					TheText.color.g,
					TheText.color.b,
					TheText.color.a - (_transitionStep * deltaTime)
				);
			}
			else
			{
				TheText.color = new Color(
					TheText.color.r,
					TheText.color.g,
					TheText.color.b,
					0
				);
			}

			if (TheText.color.a <= 0)
			{
				IsClosing = false;

				if(Messages.Count > 0)
                    activate();
				else
                    deactivate();
			}
		}
	}
}
