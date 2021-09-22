using System;

public class PlayerAddedEventArgs : EventArgs
{
    public PlayerInput PlayerAdded { get; set; }

    public PlayerAddedEventArgs(PlayerInput playerAdded)
    {
        PlayerAdded = playerAdded;
    }
}
