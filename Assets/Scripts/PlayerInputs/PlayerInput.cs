using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInput
{
    public InputUser InputUser;
    public InputActionAsset InputActionAsset;

    public LobbyInput Lobby;
    public GameInput Game;

    public PlayerInput(InputActionAsset inputActionAsset, InputUser inputUser)
    {
        InputActionMap lobbyInputActionMap = inputActionAsset.FindActionMap("Lobby");
        Lobby = new LobbyInput(lobbyInputActionMap);

        InputActionMap gameInputActionMap = inputActionAsset.FindActionMap("Game");
        Game = new GameInput(gameInputActionMap);

        InputUser = inputUser;
        InputActionAsset = inputActionAsset;
    }
}
