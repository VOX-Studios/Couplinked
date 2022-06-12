using UnityEngine;
using Steamworks;
using System.Collections.Concurrent;

/// <summary>
/// This is a port of StatsAndAchievements.cpp from SpaceWar, the official Steamworks Example. 
/// </summary>
public class SteamStatsAndAchievements : MonoBehaviour, ISteamStatsAndAchievements
{
	/// <summary>
	/// Achievements that are pending unlock.
	/// </summary>
	private ConcurrentDictionary<string, AchievementData> _achievements = new ConcurrentDictionary<string, AchievementData>();

	// Our GameID
	private CGameID _gameID;

	// Did we get the stats from Steam?
	private bool _areStatsRequested;
	private bool _areStatsValid;

	// Should we store stats this frame?
	private bool _shouldStoreStats;

	protected Callback<UserStatsReceived_t> _userStatsReceived;
	protected Callback<UserStatsStored_t> _userStatsStored;
	protected Callback<UserAchievementStored_t> _userAchievementStored;

	void OnEnable()
	{
		if (!SteamManager.Initialized)
			return;

		// Cache the GameID for use in the Callbacks
		_gameID = new CGameID(SteamUtils.GetAppID());

		_userStatsReceived = Callback<UserStatsReceived_t>.Create(_onUserStatsReceived);
		_userStatsStored = Callback<UserStatsStored_t>.Create(_onUserStatsStored);
		_userAchievementStored = Callback<UserAchievementStored_t>.Create(_onAchievementStored);

		// These need to be reset to get the stats upon an Assembly reload in the Editor.
		_areStatsRequested = false;
		_areStatsValid = false;
	}

	void Update()
	{
		if (!SteamManager.Initialized)
			return;

		if (!_areStatsRequested)
		{
			// Is Steam Loaded? if no, can't get stats, done
			if (!SteamManager.Initialized)
			{
				_areStatsRequested = true;
				return;
			}

			// If yes, request our stats
			bool success = SteamUserStats.RequestCurrentStats();

			// This function should only return false if we weren't logged in, and we already checked that.
			// But handle it being false again anyway, just ask again later.
			_areStatsRequested = success;
		}

		if (!_areStatsValid)
			return;

		// Get info from sources

		// Evaluate achievements
		foreach (AchievementData achievement in _achievements.Values)
		{
			_queueUnlockAchievement(achievement);
		}

		//Store stats in the Steam database if necessary
		if (_shouldStoreStats)
		{
			//we already set any achievements in UnlockAchievement
			
			/*
			// set stats
			SteamUserStats.SetStat("NumLosses", m_nTotalNumLosses);
			SteamUserStats.SetStat("FeetTraveled", m_flTotalFeetTraveled);
			SteamUserStats.SetStat("MaxFeetTraveled", m_flMaxFeetTraveled);
			// Update average feet / second stat
			//SteamUserStats.UpdateAvgRateStat("AverageSpeed", m_flGameFeetTraveled, m_flGameDurationSeconds);
			// The averaged result is calculated for us
			SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);
			*/
			
			bool wereStatsStored = SteamUserStats.StoreStats();

			_achievements.Clear();

			// If this failed, we never sent anything to the server, try again later.
			_shouldStoreStats = !wereStatsStored;
		}
	}

	public void UnlockAchievement(Challenge challenge)
    {
		AchievementData achievement = new AchievementData(challenge.Id);

		_achievements.TryAdd(achievement.AchievementApiName, achievement);
	}

	/// <summary>
	/// This will unlock the achievement passed in.
	/// </summary>
	/// <param name="achievement"></param>
	private void _queueUnlockAchievement(AchievementData achievement)
	{
		// mark it down
		SteamUserStats.SetAchievement(achievement.AchievementApiName.ToString());

		// Store stats end of frame
		_shouldStoreStats = true;
	}

	/// <summary>
	/// This is called when steam sends back user stats.
	/// </summary>
	/// <param name="pCallback"></param>
	private void _onUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized)
			return;

		// we may get callbacks for other games' stats arriving, ignore them
		if (pCallback.m_nGameID != (ulong)_gameID)
			return;

		if (pCallback.m_eResult != EResult.k_EResultOK)
        {
			Debug.LogWarning($"RequestStats - failed, {pCallback.m_eResult}");
			return;
		}

		Debug.Log("Received stats and achievements from Steam");

		_areStatsValid = true;

		// load stats
		//SteamUserStats.GetStat("NumLosses", out m_nTotalNumLosses);
		//SteamUserStats.GetStat("FeetTraveled", out m_flTotalFeetTraveled);
		//SteamUserStats.GetStat("MaxFeetTraveled", out m_flMaxFeetTraveled);
		//SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);
	}

	/// <summary>
	/// This is called when steam stores our user stats.
	/// </summary>
	/// <param name="pCallback"></param>
	private void _onUserStatsStored(UserStatsStored_t pCallback)
	{
		// we may get callbacks for other games' stats arriving, ignore them
		if (pCallback.m_nGameID != (ulong)_gameID)
			return;

		switch(pCallback.m_eResult)
        {
			case EResult.k_EResultOK:
				Debug.Log("StoreStats - success");
				break;
			case EResult.k_EResultInvalidParam:
				// One or more stats we set broke a constraint. They've been reverted.  We should re-iterate the values now to keep in sync.
				Debug.LogWarning("StoreStats - some failed to validate");

				// Fake up a callback here so that we re-load the values.
				UserStatsReceived_t callback = new UserStatsReceived_t();
				callback.m_eResult = EResult.k_EResultOK;
				callback.m_nGameID = (ulong)_gameID;

				_onUserStatsReceived(callback);
				break;
			default:
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
				break;
		}
	}

	/// <summary>
	/// This is called when steam stores an achievement.
	/// </summary>
	/// <param name="results"></param>
	private void _onAchievementStored(UserAchievementStored_t results)
	{
		// we may get callbacks for other games' stats arriving, ignore them
		if (results.m_nGameID != (ulong)_gameID)
			return;

		if (results.m_nMaxProgress == 0)
		{
			Debug.Log($"Achievement '{results.m_rgchAchievementName }' unlocked!");
		}
		else
		{
			Debug.Log($"Achievement '{results.m_rgchAchievementName}' progress callback, ({results.m_nCurProgress}, {results.m_nMaxProgress})");
		}
	}

	public void ResetStats()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		
		SteamUserStats.ResetAllStats(true);
		SteamUserStats.RequestCurrentStats();
	}

	private class AchievementData
	{
		public string AchievementApiName;

		/// <summary>
		/// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/1404870
		/// </summary>
		/// <param name="achievementApiName">The "API Name" used to uniquely identify the achievement.</param>
		public AchievementData(string achievementApiName)
		{
			AchievementApiName = achievementApiName;
		}
	}

    private void OnApplicationQuit()
    {
		if (!SteamManager.Initialized)
			return;

		SteamAPI.Shutdown();
    }
}