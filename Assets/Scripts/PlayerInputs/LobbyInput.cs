using UnityEngine.InputSystem;

public class LobbyInput
{
    private InputActionMap _lobbyInputActionMap;

    private InputAction _backInputAction;
    private InputAction _selectInputAction;

    public bool BackInput => _backInputAction.triggered;
    public bool SelectInput => _selectInputAction.triggered;

    public LobbyInput(InputActionMap lobbyInputActionMap)
    {
        _lobbyInputActionMap = lobbyInputActionMap;

        _backInputAction = _lobbyInputActionMap.FindAction("Back");
        _selectInputAction = _lobbyInputActionMap.FindAction("Select");
    }

    public void Enable()
    {
        _lobbyInputActionMap.Enable();
    }

    public void Disable()
    {
        _lobbyInputActionMap.Disable();
    }
}
