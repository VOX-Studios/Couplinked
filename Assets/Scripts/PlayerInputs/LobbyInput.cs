using UnityEngine.InputSystem;

public class LobbyInput
{
    private InputActionMap _lobbyInputActionMap;

    private InputAction _backInputAction;
    private InputAction _selectInputAction;
    private InputAction _changeTeamActionInput;
    private InputAction _cycleOrderActionInput;

    public bool BackInput => _backInputAction.triggered;
    public bool SelectInput => _selectInputAction.triggered;

    public bool ChangeTeamInputTriggered => _changeTeamActionInput.triggered;
    public float ChangeTeamInputValue => _changeTeamActionInput.ReadValue<float>();

    public bool CycleOrderInputTriggered => _cycleOrderActionInput.triggered;
    public float CycleOrderInputValue => _cycleOrderActionInput.ReadValue<float>();

    public LobbyInput(InputActionMap lobbyInputActionMap)
    {
        _lobbyInputActionMap = lobbyInputActionMap;

        _backInputAction = _lobbyInputActionMap.FindAction("Back");
        _selectInputAction = _lobbyInputActionMap.FindAction("Select");
        _changeTeamActionInput = _lobbyInputActionMap.FindAction("Change Team");
        _cycleOrderActionInput = _lobbyInputActionMap.FindAction("Cycle Order");
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
