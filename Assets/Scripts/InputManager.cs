﻿using UnityEngine.InputSystem;

public class InputManager
{
	private InputActionMap _gameInputActionMap;
	private InputActionMap _menuInputActionMap;
	private InputActionMap _levelEditorInputActionMap;
	private InputActionMap _notificationManagerInputActionMap;

	public InputManager(InputActionAsset inputActions)
	{
		_gameInputActionMap = inputActions.FindActionMap("Game");
		_menuInputActionMap = inputActions.FindActionMap("Menu");
		_levelEditorInputActionMap = inputActions.FindActionMap("Level Editor");
		_notificationManagerInputActionMap = inputActions.FindActionMap("Notification Manager");
	}

	public void ToggleInputs(AppStateEnum appState, bool isNotificationActive)
    {
		if (isNotificationActive)
		{
			_notificationManagerInputActionMap.Enable();

			_gameInputActionMap.Disable();
			_menuInputActionMap.Disable();
			_levelEditorInputActionMap.Disable();
			return;
		}

		_notificationManagerInputActionMap.Disable();

		switch (appState)
		{
			default:
				_gameInputActionMap.Disable();
				_menuInputActionMap.Enable();
				_levelEditorInputActionMap.Disable();
				break;
			case AppStateEnum.Game:
				_gameInputActionMap.Enable();
				_menuInputActionMap.Disable();
				_levelEditorInputActionMap.Disable();
				break;
			case AppStateEnum.LevelEditor:
				_gameInputActionMap.Disable();
				_menuInputActionMap.Enable(); //sharing menu controls for now
				_levelEditorInputActionMap.Enable();
				break;
		}
	}
}