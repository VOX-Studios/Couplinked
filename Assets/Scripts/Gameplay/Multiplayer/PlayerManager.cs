using Assets.Scripts.Gameplay.Multiplayer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class PlayerManager : MonoBehaviour
{
    public const byte MAX_PLAYERS = 4;

    [SerializeField]
    private InputActionAsset _playerActionsPrefab;

    private PlayerSlotsManager _playerSlotsManager;

    public List<PlayerInput> Players;

    //TODO: playerSlotIndex: an bag of available indices to indentify where we're placing the player as if there was a physical equivalent

    public bool IsJoining { get; private set; }

    void Awake()
    {
        Players = new List<PlayerInput>();
        _playerSlotsManager = new PlayerSlotsManager(MAX_PLAYERS);
    }

    public event EventHandler<PlayerAddedEventArgs> OnPlayerAdded;

    private void _onUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        //ignore if not a button press (or if it's mouse input)
        if (Players.Count == MAX_PLAYERS || !(control is ButtonControl) || control.device.displayName == "Mouse")
            return;

        InputActionAsset actions = Instantiate(_playerActionsPrefab);

        InputAction temp = actions.FindActionMap("Lobby").FindAction("Back");

        //check if they're pressing the back button
        for (int i = 0; i < temp.bindings.Count; i++)
        {
            //if they're pressing the back button, don't join
            bool isMatch = InputControlPath.Matches(temp.bindings[i].path, control);

            if (isMatch)
                return;
        }

        //get a new InputUser, now paired with the device
        InputUser inputUser = InputUser.PerformPairingWithDevice(control.device);

        //lock down the input to the specific device
        inputUser.AssociateActionsWithUser(actions);

        //assign a "slot" to the player
        int playerSlot = _playerSlotsManager.TakeSlot();

        PlayerInput player = new PlayerInput(actions, inputUser, playerSlot);
        Players.Add(player);

        _onPlayerAdded(player);
    }

    private void _onPlayerAdded(PlayerInput playerAdded)
    {
        PlayerAddedEventArgs eventArgs = new PlayerAddedEventArgs(playerAdded);
        EventHandler<PlayerAddedEventArgs> handler = OnPlayerAdded;
        handler?.Invoke(this, eventArgs);
    }

    public void BeginJoining()
    {
        if (IsJoining || Players.Count == MAX_PLAYERS)
            return;

        InputUser.listenForUnpairedDeviceActivity = 1;
        InputUser.onUnpairedDeviceUsed += _onUnpairedDeviceUsed;

        IsJoining = true;
    }

    public void EndJoining()
    {
        InputUser.listenForUnpairedDeviceActivity = 0;
        InputUser.onUnpairedDeviceUsed -= _onUnpairedDeviceUsed;
        IsJoining = false;
    }

    public void RemovePlayer(PlayerInput player)
    {
        player.InputUser.UnpairDevicesAndRemoveUser();
        Players.Remove(player);
        _playerSlotsManager.ReturnSlot(player.PlayerSlot);
    }
}
