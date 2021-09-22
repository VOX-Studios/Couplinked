
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput
{
    private InputActionMap _gameInputActionMap;

    private InputAction _move1InputAction;
    private InputAction _move2InputAction;
    private InputAction _pauseInputAction;
    private InputAction _unpauseInputAction;
    private InputAction _exitInputAction;
    public Vector2 Move1Input => _move1InputAction.ReadValue<Vector2>();
    public Vector2 Move2Input => _move2InputAction.ReadValue<Vector2>();
    public bool PauseInput => _pauseInputAction.triggered;
    public bool UnpauseInput => _unpauseInputAction.triggered;
    public bool ExitInput => _exitInputAction.triggered;


    public GameInput(InputActionMap gameInputActionMap)
    {
        _gameInputActionMap = gameInputActionMap;

        _move1InputAction = gameInputActionMap.FindAction("Move1");
        _move2InputAction = gameInputActionMap.FindAction("Move2");
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
