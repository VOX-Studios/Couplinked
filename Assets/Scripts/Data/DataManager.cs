using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
	private const string pp_LocalHighScore = "Local High Score";
	private const string pp_MusicPreference = "Music Preference";
	private const string pp_SfxPreference = "FX Preference";
	private const string pp_ResolutionPreference = "Resolution";
	public readonly EnumData<FullScreenMode> WindowedModePreference = new EnumData<FullScreenMode>("Windowed Mode");
	public readonly EnumData<VSyncCountEnum> VSyncCountPreference = new EnumData<VSyncCountEnum>("VSync Count");
	
	public readonly StringData IsThisTheInitialSetup = new StringData("Initial Set Up");
	public readonly BoolData IsInitialSetupComplete_1_9 = new BoolData("v1.9 Initial Set Up");
	public readonly BoolData IsInitialSetupComplete_2_1 = new BoolData("v2.1 Initial Set Up");
	public readonly BoolData IsInitialSetupComplete_2_2 = new BoolData("v2.2 Initial Set Up");
	public readonly BoolData IsInitialSetupComplete_2_3 = new BoolData("v2.3 Initial Set Up");
	public readonly BoolData IsInitialSetupComplete_2_7 = new BoolData("v2.7 Initial Set Up");

	public readonly BoolData ShouldShowFps = new BoolData("Should Show FPS");

	private const string pp_CustomLevelIDs = "Custom Level IDs";

	private const string pp_LevelMaxScoreAddOn = "Max Score";
	private const string pp_LevelPlayerScoreAddOn = "Player Score";
	private const string pp_LevelPlayerScoreRatingAddOn = "Player Rating";
	private const string pp_LevelLockedStatusAddOn = "Locked Status";

	private const string pp_HighestCampaignLevelUnlocked = "Highest Level Unlocked";
	private const string pp_HighestCampaignLevelTierUnlocked = "Highest Level Tier Unlocked";

	private const string pp_LevelNameAddOn = "name";

	public readonly CustomPlayerColorData[] PlayerColors;

	public readonly EnumData<QualitySettingEnum> GridDensity = new EnumData<QualitySettingEnum>("Grid Density");
	public readonly EnumData<QualitySettingEnum> ExplosionParticleQuality = new EnumData<QualitySettingEnum>("Explosion Particle Quality");
	public readonly EnumData<QualitySettingEnum> TrailParticleQuality = new EnumData<QualitySettingEnum>("Trail Particle Quality");
	public readonly EnumData<QualitySettingEnum> ScreenShakeStrength = new EnumData<QualitySettingEnum>("Screen Shake Strength");

	public DataManager()
    {
		PlayerColors = new CustomPlayerColorData[PlayerManager.MAX_PLAYERS];

		for(int i = 0; i < PlayerColors.Length; i++)
        {
			//two is the number of node colors we're allowed to save per player
			PlayerColors[i] = new CustomPlayerColorData(i, 2);
        }
	}

	private string _getGameDifficultyString(GameDifficultyEnum gameDifficulty)
    {
		string stringValue = null;

		if (gameDifficulty != GameDifficultyEnum.Hard)
			stringValue = gameDifficulty.ToString();

		return stringValue;
	}

	/// <summary>
	/// Used to unlock a campaign level.
	/// </summary>
	/// <param name="levelNumber"></param>
	/// <param name="gameDifficulty"></param>
	public void UnlockCampaignLevel(int levelNumber, GameDifficultyEnum gameDifficulty)
    {
		if (levelNumber >= 100) //TODO: just pass in level name if we can?
		{
			Debug.LogError("Cannot handle levels with more than two digits!");
			return;
		}

		string difficultyToUnlock = _getGameDifficultyString(gameDifficulty);

		PlayerPrefs.SetString($"Level {(levelNumber + 1):00}{pp_LevelLockedStatusAddOn}{difficultyToUnlock}", "UNLOCKED");

		//update highest level unlocked if necessary
		int highestLevelUnlocked = GetHighestCampaignLevelUnlocked(gameDifficulty);
		if (highestLevelUnlocked < levelNumber + 1)
			SetHighestCampaignLevelUnlocked(gameDifficulty, levelNumber + 1);

		PlayerPrefs.Save();
	}

	public int GetHighestCampaignLevelUnlocked(GameDifficultyEnum gameDifficulty)
    {
		string difficultyString = _getGameDifficultyString(gameDifficulty);
		return PlayerPrefs.GetInt($"{pp_HighestCampaignLevelUnlocked}{difficultyString}");
	}

	public void SetHighestCampaignLevelUnlocked(GameDifficultyEnum gameDifficulty, int highestCampaignLevelUnlocked)
	{
		string difficultyString = _getGameDifficultyString(gameDifficulty);
		PlayerPrefs.SetInt($"{pp_HighestCampaignLevelUnlocked}{difficultyString}", highestCampaignLevelUnlocked);
		PlayerPrefs.Save();
	}

	public int GetHighestCampaignLevelTierUnlocked(GameDifficultyEnum gameDifficulty)
	{
		string difficultyString = _getGameDifficultyString(gameDifficulty);
		return PlayerPrefs.GetInt($"{pp_HighestCampaignLevelTierUnlocked}{difficultyString}");
	}

	public void SetHighestCampaignLevelTierUnlocked(GameDifficultyEnum gameDifficulty, int highestCampaignLevelTierUnlocked)
	{
		string difficultyString = _getGameDifficultyString(gameDifficulty);
		PlayerPrefs.SetInt($"{pp_HighestCampaignLevelTierUnlocked}{difficultyString}", highestCampaignLevelTierUnlocked);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Used to check if a campaign level is unlocked.
	/// </summary>
	/// <param name="levelNumber"></param>
	/// <param name="gameDifficulty"></param>
	public bool IsCampaignLevelLocked(int levelNumber, GameDifficultyEnum gameDifficulty)
	{
		if (levelNumber >= 100) //TODO: just pass in level name if we can?
		{
			Debug.LogError("Cannot handle levels with more than two digits!");
			return true;
		}

		string difficultyToUnlock = _getGameDifficultyString(gameDifficulty);

		return PlayerPrefs.GetString($"Level {(levelNumber + 1):00}{pp_LevelLockedStatusAddOn}{difficultyToUnlock}") != "UNLOCKED";
	}

	/// <summary>
	/// This is used for both campaign and custom levels.  Does not rely on game difficulty.
	/// </summary>
	/// <param name="levelId"></param>
	/// <returns></returns>
	public int GetLevelMaxScore(string levelId)
	{
		return PlayerPrefs.GetInt($"{levelId}{pp_LevelMaxScoreAddOn}");
	}

	/// <summary>
	/// This is used for both campaign and custom levels.
	/// </summary>
	/// <param name="levelId"></param>
	/// <param name="gameDifficulty"></param>
	public int GetLevelPlayerScore(string levelId, GameDifficultyEnum gameDifficulty)
	{
		string gameDifficultyString = _getGameDifficultyString(gameDifficulty);
		return PlayerPrefs.GetInt($"{levelId}{pp_LevelPlayerScoreAddOn}{gameDifficultyString}");
	}

	/// <summary>
	/// This is used for both campaign and custom levels.
	/// </summary>
	/// <param name="levelId"></param>
	/// <param name="gameDifficulty"></param>
	/// <param name="playerScore"></param>
	public void SetLevelPlayerScore(string levelId, GameDifficultyEnum gameDifficulty, int playerScore)
	{
		string gameDifficultyString = _getGameDifficultyString(gameDifficulty);
		PlayerPrefs.SetInt($"{levelId}{pp_LevelPlayerScoreAddOn}{gameDifficultyString}", playerScore);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// This is used for survival.
	/// </summary>
	public int GetSurvivalHighScore()
	{
		return PlayerPrefs.GetInt(pp_LocalHighScore);
	}

	/// <summary>
	/// This is used for survival.
	/// </summary>
	/// <param name="highScore"></param>
	public void SetSurvivalHighScore(int highScore)
	{
		PlayerPrefs.SetInt(pp_LocalHighScore, highScore);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// This is used for campaign levels.
	/// </summary>
	/// <param name="levelId"></param>
	/// <param name="gameDifficulty"></param>
	public int GetCampaignLevelPlayerScoreRating(int levelNumber, GameDifficultyEnum gameDifficulty)
	{
		return GetLevelPlayerScoreRating($"Level {(levelNumber + 1):00}", gameDifficulty);
	}

	/// <summary>
	/// This is used for both campaign and custom levels.
	/// </summary>
	/// <param name="levelId"></param>
	/// <param name="gameDifficulty"></param>
	public int GetLevelPlayerScoreRating(string levelId, GameDifficultyEnum gameDifficulty)
	{
		string gameDifficultyString = _getGameDifficultyString(gameDifficulty);
		return PlayerPrefs.GetInt($"{levelId}{pp_LevelPlayerScoreRatingAddOn}{gameDifficultyString}");
	}

	/// <summary>
	/// This is used for both campaign and custom levels.
	/// </summary>
	/// <param name="levelId"></param>
	/// <param name="gameDifficulty"></param>
	/// <param name="playerScoreRating"></param>
	public void SetLevelPlayerScoreRating(string levelId, GameDifficultyEnum gameDifficulty, int playerScoreRating)
    {
		string gameDifficultyString = _getGameDifficultyString(gameDifficulty);
		PlayerPrefs.SetInt($"{levelId}{pp_LevelPlayerScoreRatingAddOn}{gameDifficultyString}", playerScoreRating);
		PlayerPrefs.Save();
	}

	public string[] GetCustomLevelIds()
    {
		string customLevelIds = PlayerPrefs.GetString(pp_CustomLevelIDs);
		string[] customLevelIdsArray = customLevelIds.Split(new string[] { "|--|" }, StringSplitOptions.RemoveEmptyEntries);

		return customLevelIdsArray;
	}

	public void SetCustomLevelIds(IEnumerable<string> levelIds)
	{
		string data = string.Join("|--|", levelIds);
		PlayerPrefs.SetString(pp_CustomLevelIDs, data);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Used to get custom level names.
	/// </summary>
	/// <param name="levelId"></param>
	/// <returns></returns>
	public string GetCustomLevelName(string levelId)
    {
		return PlayerPrefs.GetString($"{levelId}{pp_LevelNameAddOn}");
	}

	/// <summary>
	/// Used to save custom level names.
	/// </summary>
	/// <param name="levelId"></param>
	/// /// <param name="levelName"></param>
	public void SetCustomLevelName(string levelId, string levelName)
	{
		PlayerPrefs.SetString($"{levelId}{pp_LevelNameAddOn}", levelName);
		PlayerPrefs.Save();
	}

	public void SetCustomLevelData(string levelId, string levelData)
    {
		PlayerPrefs.SetString(levelId, levelData);
		PlayerPrefs.Save();
	}

	public void DeleteCustomLevel(string levelId)
    {
		PlayerPrefs.DeleteKey($"{levelId}{pp_LevelMaxScoreAddOn}");
		PlayerPrefs.DeleteKey($"{levelId}{pp_LevelPlayerScoreAddOn}");
		PlayerPrefs.DeleteKey($"{levelId}{pp_LevelPlayerScoreRatingAddOn}");
		PlayerPrefs.DeleteKey(levelId);

		List<string> levelIds = new List<string>(GetCustomLevelIds());
		int index = levelIds.IndexOf(levelId);
		if (index > -1)
		{
			levelIds.Remove(levelId);
			SetCustomLevelIds(levelIds);
		}

		PlayerPrefs.Save();
	}

	public bool GetMusicPreference()
	{
		int pref = PlayerPrefs.GetInt(pp_MusicPreference);
		return pref == 1;
	}

	public void SetMusicPreference(bool isOn)
    {
		PlayerPrefs.SetInt(pp_MusicPreference, isOn ? 1 : 0);
		PlayerPrefs.Save();
	}

	public bool GetSfxPreference()
	{
		int pref = PlayerPrefs.GetInt(pp_SfxPreference);
		return pref == 1;
	}

	public void SetSfxPreference(bool isOn)
	{
		PlayerPrefs.SetInt(pp_SfxPreference, isOn ? 1 : 0);
		PlayerPrefs.Save();
	}

	public string GetResolutionPreference()
	{
		return PlayerPrefs.GetString(pp_ResolutionPreference);
	}

	public void SetResolutionPreference(string resolutionPreference)
	{
		PlayerPrefs.SetString(pp_ResolutionPreference, resolutionPreference);
		PlayerPrefs.Save();
	}
}