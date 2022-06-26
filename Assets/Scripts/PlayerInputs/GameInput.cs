
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput
{
    private InputActionMap _gameInputActionMap;

    private InputAction[] _moveInputActions;
    private InputAction _pauseInputAction;
    private InputAction _unpauseInputAction;
    private InputAction _exitInputAction;

    public Vector2 MoveInput(int index)
    {
        return _moveInputActions[index].ReadValue<Vector2>();
    }

    public bool PauseInput => _pauseInputAction.triggered;
    public bool UnpauseInput => _unpauseInputAction.triggered;
    public bool ExitInput => _exitInputAction.triggered;


    public GameInput(InputActionMap gameInputActionMap)
    {
        _gameInputActionMap = gameInputActionMap;

        _moveInputActions = new InputAction[]
        {
            gameInputActionMap.FindAction("Move1"),
            gameInputActionMap.FindAction("Move2"),
            gameInputActionMap.FindAction("Move Combined"),
        };

        _pauseInputAction = gameInputActionMap.FindAction("Pause");
        _unpauseInputAction = gameInputActionMap.FindAction("Unpause");
        _exitInputAction = gameInputActionMap.FindAction("Exit");
    }

    public void Enable()
    {
        _gameInputActionMap.Enable();
    }

    public void Disable()
    {
        _gameInputActionMap.Disable();
    }
}
