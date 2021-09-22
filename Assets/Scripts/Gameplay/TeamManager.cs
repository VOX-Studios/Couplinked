public class TeamManager
{
    public Team Teams;

    public bool IsOnTeam(int potentialContainingTeamId, int teamIdToCheck)
    {
        Team mainTeam = _find(Teams, potentialContainingTeamId);

        //if the ID passed in was bogus, nothing can be on the bogus team
        if (mainTeam == null)
            return false;

        //check if team is sub team
        Team teamMatch = _find(mainTeam, teamIdToCheck);

        return teamMatch != null;
    }

    private Team _find(Team currentTeam, int teamId)
    {
        //if we match current team, return it
        if (currentTeam.Id == teamId)
            return currentTeam;

        if (currentTeam.SubTeams == null)
            return null;

        //loop through child teams
        foreach(Team team in currentTeam.SubTeams)
        {
            //check for match
            Team matchedTeam = _find(team, teamId);

            //if we found a match, return it
            if (matchedTeam != null)
                return matchedTeam;
        }

        return null;
    }
}
