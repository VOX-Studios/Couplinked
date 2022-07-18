using UnityEngine.InputSystem;

public class InputManager
{
	private InputActionMap _gameInputActionMap;
	private InputActionMap _menuInputActionMap;
	private InputActionMap _levelEditorInputActionMap;
	private InputActionMap _notificationManagerInputActionMap;

	private InputActionMap _defaultUiInputActionMap;

	public InputManager(InputActionAsset inputActions, InputActionAsset defaultInputActions)
	{
		_gameInputActionMap = inputActions.FindActionMap("Game");
		_menuInputActionMap = inputActions.FindActionMap("Menu");
		_levelEditorInputActionMap = inputActions.FindActionMap("Level Editor");
		_notificationManagerInputActionMap = inputActions.FindActionMap("Notification Manager");

		_defaultUiInputActionMap = defaultInputActions.FindActionMap("UI");
	}

	public void ToggleInputs(AppStateEnum appState, bool isNotificationActive)
    {
		if (isNotificationActive)
		{
			_notificationManagerInputActionMap.Enable();

			_gameInputActionMap.Disable();
			_menuInputActionMap.Disable();
			_levelEditorInputActionMap.Disable();
			_defaultUiInputActionMap.Disable();
			return;
		}

		_notificationManagerInputActionMap.Disable();
		_defaultUiInputActionMap.Enable();

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
				_menuInputActionMap.Disable();
				_levelEditorInputActionMap.Enable();
				break;
		}
	}
}