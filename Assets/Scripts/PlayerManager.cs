using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class PlayerManager : MonoBehaviour
{
    public const int MAX_PLAYERS = 2;

    [SerializeField]
    private InputActionAsset _player1Actions;

    [SerializeField]
    private InputActionAsset _player2Actions;

    public List<PlayerInput> Players;

    private HashSet<InputActionAsset> _pairedActions;

    public bool IsJoining { get; private set; }

    void Awake()
    {
        Players = new List<PlayerInput>();
        _pairedActions = new HashSet<InputActionAsset>();
    }

    public event EventHandler<PlayerAddedEventArgs> OnPlayerAdded;

    private void _onUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        //ignore if not a button press (or if it's mouse input)
        if (Players.Count == MAX_PLAYERS || !(control is ButtonControl) || control.device.displayName == "Mouse")
            return;

        InputActionAsset actions;

        if (!_pairedActions.Contains(_player1Actions))
            actions = _player1Actions;
        else if (!_pairedActions.Contains(_player2Actions))
            actions = _player2Actions;
        else //we've run out of actions to pair
            return;

        InputAction temp = actions.FindActionMap("Lobby").FindAction("Back");

        //check if they're pressing the back button
        for (int i = 0; i < temp.bindings.Count; i++)
        {
            //if they're pressing the back button, don't join
            bool isMatch = InputControlPath.Matches(temp.bindings[i].path, control);

            if (isMatch)
                return;
        }

        _pairedActions.Add(actions);

        //get a new InputUser, now paired with the device
        InputUser inputUser = InputUser.PerformPairingWithDevice(control.device);

        //lock down the input to the specific device
        inputUser.AssociateActionsWithUser(actions); 

        PlayerInput player = new PlayerInput(actions, inputUser);
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
        _pairedActions.Remove(player.InputActionAsset);
        player.InputUser.UnpairDevicesAndRemoveUser();
        Players.Remove(player);
    }
}
