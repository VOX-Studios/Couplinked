using Steamworks;

public class Challenges 
{
	public Challenge[] ChallengesArray;

	public const string ID_IncorrectPronounciation = "Incorrect Pronunciation";
	public const string ID_SportsmanshipAward = "Sportsmanship Award";
	public const string ID_20Levels = "20 Levels";
	public const string ID_30Levels = "30 Levels";
	public const string ID_40Levels = "40 Levels";
	public const string ID_50Levels = "50 Levels";
	public const string ID_5StarRatings = "5 Star Ratings";
	public const string ID_Quit = "Quit";
	public const string ID_AlmostMissedOne = "Almost Missed One";
	public const string ID_H10 = "Hang 10";
	public const string ID_Twisted = "Get It Twisted";
	public const string ID_0Points = "0 Points";
	public const string ID_1000Points = "1000 Points";
	public const string ID_CollectEveryRingInALevel = "Collect Every Ring In A Level";
	public const string ID_SurviveToMaxSpeed = "Survive To Max Speed";
	public const string ID_Gymnast = "Gymnast";
	public const string ID_MarathonMan = "Marathon Man";
	public const string ID_Smile = "Complete Last Level";
	public const string ID_5StarHard = "5 Star Campaign On Hard";

	private ISteamStatsAndAchievements _steamStatsAndAchievements;

	public Challenges(ISteamStatsAndAchievements steamStatsAndAchievements)
    {
		_steamStatsAndAchievements = steamStatsAndAchievements;
	}

	public void LoadChallenges()
	{
		ChallengesArray = new Challenge[]
		{
			//Start with this one defaulted as completed
			new Challenge(
				id: ID_IncorrectPronounciation,
				displayText: "Pronounce Couplinked Incorrectly",
				description: "\"Cup\" - \"Linked\"\nIs that so difficult?"
				),

			new Challenge(
				id: ID_SportsmanshipAward,
				displayText: "Everybody Gets A Trophy!",
				description: "Complete the first 10 levels."
				),

			new Challenge(
				id: ID_20Levels,
				displayText: "20-20 Vision",
				description: "Complete the first 20 levels."
				),

			new Challenge(
				id: ID_30Levels,
				displayText: "30 And Flirty",
				description: "Complete the first 30 levels."
				),
			new Challenge(
				id: ID_40Levels,
				displayText: "Over The Hill",
				description: "Complete the first 40 levels."
				),
			new Challenge(
				id: ID_50Levels,
				displayText: "5-O",
				description: "Complete the first 50 levels."
				),

			new Challenge(
				id: ID_5StarRatings,
				displayText: "Perfectionist",
				description: "Get a 5 star rating on every campaign level."
				),

			new Challenge(
				id: ID_Quit,
				displayText: "Blue Balled",
				description: "Quit in the middle of a game."
				),

			new Challenge(
				id: ID_AlmostMissedOne,
				displayText: "Savior",
				description: "Collect a ring just before it goes off of the screen."
				),
			new Challenge(
				id: ID_H10,
				displayText: "Hang 10",
				description: "Get a perfect score off of a single ring."
				),
			new Challenge(
				id: ID_Twisted,
				displayText: "Get It Twisted",
				description: "Get it twisted while collecting a split ring."
				),

			new Challenge(
				id: ID_0Points,
				displayText: "You Can't Even...",
				description: "Zero points?  Really???"
				),
			new Challenge(
				id: ID_1000Points,
				displayText: "Mile High Club",
				description: "Score over 1,000 points in a single Survival Mode game."
				),

			new Challenge(
				id: ID_CollectEveryRingInALevel,
				displayText: "Collect All The Rings!",
				description: "Collect all of the rings in a 4th tier or higher level."
				),

			new Challenge(
				id: ID_SurviveToMaxSpeed,
				displayText: "Keep On Survivin'",
				description: "I'm a survivor (What?)\nI'm not gon' give up (What?)\nI'm not gon' stop (What?)\n"
			              + "I'm gonna last a really long time in Survival Mode."
						  ),

			new Challenge(
				id: ID_Gymnast,
				displayText: "Gymnast",
				description: "Collect over 500 split rings."
				),
			new Challenge(
				id: ID_MarathonMan,
				displayText: "Marathon Man",
				description: "Play Couplinked for at least 2 hours."
				),
			new Challenge(
				id: ID_Smile,
				displayText: "Smile",
				description: "Complete the last level."
				),
			new Challenge(
				id: ID_5StarHard,
				displayText: "Sadist",
				description: "Get a 5 star rating on every campaign level in hard mode."
				)
		};
	}

	public bool HandleUnlockingChallenge(string id, out string title, bool overrideForOnlineUnlock = false)
	{
		title = "";
		bool unlocked = false;
		LoadChallenges();

		//If this challenge wasn't already unlocked, unlock it
		if (!Challenge.GetCompletedStatus(id))
		{
			unlocked = true;
			Challenge.SetCompletedStatus(id);
			for(int i = 0; i < ChallengesArray.Length; i++)
			{
				if(ChallengesArray[i].Id == id)
				{
					_onlineUnlock(ChallengesArray[i]);
					title = "Achievement Unlocked\n" + ChallengesArray[i].DisplayText;
					break;
				}
			}
		}
		else if(overrideForOnlineUnlock)
		{
			for(int i = 0; i < ChallengesArray.Length; i++)
			{
				if(ChallengesArray[i].Id == id)
				{
					_onlineUnlock(ChallengesArray[i]);
					break;
				}
			}
		}
		Clear();
		return unlocked;
	}

	private void _onlineUnlock(Challenge challenge)
	{
		_steamStatsAndAchievements.UnlockAchievement(challenge);
	}

	/// <summary>
	/// Frees up memory
	/// </summary>
	public void Clear()
	{
		ChallengesArray = null;
	}
}
