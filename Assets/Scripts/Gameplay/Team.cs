using System.Collections.Generic;

public class Team
{
    public int Id;
    public List<Team> SubTeams;

    public Team(int id)
    {
        Id = id;
    }

    public Team(int id, List<Team> subTeams)
    {
        Id = id;
        SubTeams = subTeams;
    }
}
