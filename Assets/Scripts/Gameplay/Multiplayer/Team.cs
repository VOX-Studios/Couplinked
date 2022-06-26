using System.Collections.Generic;

public class Team
{
    public int TeamSlot { get; private set; }

    public List<PlayerInput> PlayerInputs;

    public Team(int teamSlot)
    {
        TeamSlot = teamSlot;
    }
}
