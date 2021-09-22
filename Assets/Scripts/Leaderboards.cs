using UnityEngine;
using System.Collections.Generic;

public class Leaderboards : MonoBehaviour 
{
	public static string SurvivalLeaderboardID = "CgkIq6PqhosPEAIQAA";
	private static List<string> LevelLeaderboardIDs;

	private static void LoadLevelLeaderboards()
	{
		LevelLeaderboardIDs = new List<string>();

		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQFg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQFw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQGA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQGQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQGg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQGw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQHA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQHQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQHg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQHw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQIA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQIQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQIg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQIw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQJA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQJQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQJg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQJw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQKA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQKQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQKg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQKw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQLA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQLQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQLg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQLw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQMA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQMQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQMg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQMw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQNA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQNQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQNg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQNw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQOA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQOQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQOg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQOw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQPA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQPQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQPg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQPw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQQA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQQQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQQg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQQw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQRA");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQRQ");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQRg");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQRw");
		LevelLeaderboardIDs.Add("CgkIq6PqhosPEAIQSA");
	}

	private static void ClearLevelLeaderboards()
	{
		LevelLeaderboardIDs = null;
	}

	public static string GetLevelLeaderboardID(int levelNumber)
	{
		string returnValue;
		LoadLevelLeaderboards();

		returnValue = LevelLeaderboardIDs[levelNumber - 1];

		ClearLevelLeaderboards();

		return returnValue;
	}
}
