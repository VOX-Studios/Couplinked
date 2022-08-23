using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.InputSystem;
using Assets.Scripts.Lighting;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{	public InputActionAsset InputActions;
	public InputActionAsset DefaultInputActions;
	public SteamStatsAndAchievements SteamStatsAndAchievementsManager;

	public Challenges Challenges;

	public readonly GameDifficultyManager GameDifficultyManager = new GameDifficultyManager();

	public const string NotAvailableInTrailModeMessage = "Not available in trial mode.";
	public bool IsTrialMode = false;

	public int NumberOfLevels;

	public DataManager DataManager;
	public InputManager InputManager;
	public NotificationManager NotificationManager;
	public PlayerManager PlayerManager;
	public ColorManager ColorManager;
	public LightingManager LightingManager;

	public Grid Grid;

	public DefaultUiSettings DefaultUiSettings;

	public MenuBackgroundManager MenuBackgroundManager;

	public Level CurrentLevel;
	public GameSetupInfo GameSetupInfo;

	public static float TopY;
	public static float BotY;
	public static float ClampYOffset;
	public static float TopYWithClamp;
	public static float BotYWithClamp;

	public static float LeftX;
	public static float RightX;
	public static float ClampXOffset;
	public static float LeftXWithClamp;
	public static float RightXWithClamp;

	public static float WorldWidth;

	// Use this for initialization
	void Awake()
	{
		GameObject.DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += _onSceneLoaded;

		DataManager = new DataManager();
		InputManager = new InputManager(InputActions, DefaultInputActions);
		NotificationManager.Initialize(this);

		Challenges = new Challenges(SteamStatsAndAchievementsManager);

		NumberOfLevels = Resources.LoadAll<TextAsset>("CampaignLevelData").Length;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		Hit1HitCount = PlayerPrefs.GetInt("Hit1HitCount");
		Hit2HitCount = PlayerPrefs.GetInt("Hit2HitCount");
		HitSplit1HitCount = PlayerPrefs.GetInt("HitSplit1HitCount");
		HitSplit2HitCount = PlayerPrefs.GetInt("HitSplit2HitCount");
		NoHitHitCount = PlayerPrefs.GetInt("NoHitHitCount");
		TimePlayedHours = PlayerPrefs.GetInt("TimePlayedHours");
		TimePlayedMinutes = PlayerPrefs.GetInt("TimePlayedMinutes");
		TimePlayedSeconds = PlayerPrefs.GetInt("TimePlayedSeconds");
		TimePlayedMilliseconds = PlayerPrefs.GetInt("TimePlayedMilliseconds");
		GamesPlayed = PlayerPrefs.GetInt("GamesPlayed");

		CurrentLevel = new Level();

		ClampXOffset = .5f;
		ClampYOffset = 2.5f;

		TopY = Cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
		BotY = Cam.ViewportToWorldPoint(Vector3.zero).y;
		LeftX = Cam.ViewportToWorldPoint(Vector3.zero).x;
		RightX = Cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

		TopYWithClamp = TopY - ClampYOffset;
		BotYWithClamp = BotY + ClampYOffset;
		LeftXWithClamp = LeftX + ClampXOffset;
		RightXWithClamp = RightX + ClampXOffset;

		WorldWidth = RightX - LeftX;

		LocalHighScore = DataManager.GetSurvivalHighScore();

		if (DataManager.IsThisTheInitialSetup.Get() == "")
		{
			DataManager.SetMusicPreference(true);
			DataManager.SetSfxPreference(true);
			DataManager.SetResolutionPreference($"{Screen.currentResolution.width}x{Screen.currentResolution.height}");
			DataManager.WindowedModePreference.Set(FullScreenMode.FullScreenWindow);

			SetDefaultDataValues_1_9();
			SetDefaultDataValues_2_1();
			SetDefaultDataValues_2_2();
			SetDefaultDataValues_2_3();
			SetDefaultDataValues_2_7();

			DataManager.IsThisTheInitialSetup.Set("false");
		}
		else
		{
			if (!DataManager.IsInitialSetupComplete_1_9.Get())
			{
				SetDefaultDataValues_1_9();
			}

			if (!DataManager.IsInitialSetupComplete_2_1.Get())
			{
				SetDefaultDataValues_2_1();
			}

			if (!DataManager.IsInitialSetupComplete_2_2.Get())
			{
				SetDefaultDataValues_2_2();
			}

			if (!DataManager.IsInitialSetupComplete_2_3.Get())
			{
				SetDefaultDataValues_2_3();
			}

			if (!DataManager.IsInitialSetupComplete_2_7.Get())
			{
				SetDefaultDataValues_2_7();
			}
		}

		MusicOn = DataManager.GetMusicPreference();
		SfxOn = DataManager.GetSfxPreference();

		QualitySettings.vSyncCount = (int)DataManager.VSyncCountPreference.Get();

		AudioManager.Initialize();
		SoundEffectManager.Initialize(this);

		LightingManager.Initialize();
		Grid.Initialize(this);

		AppState = AppStateEnum.Loading;

		string unlockMessage;
		Challenges.HandleUnlockingChallenge(Challenges.ID_IncorrectPronounciation, out unlockMessage);

		GameSetupInfo = new GameSetupInfo();

		ShouldShowFps = DataManager.ShouldShowFps.Get();
	}

    private void SetDefaultDataValues_1_9()
	{
		DataManager.GridDensity.Set(QualitySettingEnum.High);
		DataManager.ExplosionParticleQuality.Set(QualitySettingEnum.High);
		DataManager.TrailParticleQuality.Set(QualitySettingEnum.High);
		DataManager.ScreenShakeStrength.Set(QualitySettingEnum.High);
		DataManager.IsInitialSetupComplete_1_9.Set(true);
	}

	private void SetDefaultDataValues_2_1()
	{
		DataManager.VSyncCountPreference.Set(VSyncCountEnum.EveryVBlank);
		DataManager.IsInitialSetupComplete_2_1.Set(true);
	}

	private void SetDefaultDataValues_2_2()
	{
		//set default colors for players 1 and 2
		for (int i = 0; i < 2; i++)
		{
			DefaultPlayerColors defaultColors = ColorManager.DefaultPlayerColors[i];

			ColorData node1InsideColor = new ColorData($"P{i} Node {1} Inside Color");
			ColorData node1OutsideColor = new ColorData($"P{i} Node {1} Outside Color");
			ColorData node1ParticlesColor = new ColorData($"P{i} Node {1} Particles Color");

			ColorData node2InsideColor = new ColorData($"P{i} Node {2} Inside Color");
			ColorData node2OutsideColor = new ColorData($"P{i} Node {2} Outside Color");
			ColorData node2ParticlesColor = new ColorData($"P{i} Node {2} Particles Color");

			node1InsideColor.Set(defaultColors.NodeColors[0].InsideColor);
			node1OutsideColor.Set(defaultColors.NodeColors[0].OutsideColor);
			node1ParticlesColor.Set(defaultColors.NodeColors[0].ParticleColor);
			node2InsideColor.Set(defaultColors.NodeColors[1].InsideColor);
			node2OutsideColor.Set(defaultColors.NodeColors[1].OutsideColor);
			node2ParticlesColor.Set(defaultColors.NodeColors[1].ParticleColor);
		}

		//delete old lightning key
		PlayerPrefs.DeleteKey("Color Selected");
		PlayerPrefs.Save();

		DataManager.IsInitialSetupComplete_2_2.Set(true);
	}

	private void SetDefaultDataValues_2_3()
	{
		//set default colors for players 1 and 2
		for (int i = 0; i < 2; i++)
		{
			DataManager.PlayerColors[i].GridColor.Set(ColorManager.DefaultGridColor);
		}

		DataManager.IsInitialSetupComplete_2_3.Set(true);
	}

	private void SetDefaultDataValues_2_7()
	{
		//for players 1 and 2
		for (int i = 0; i < 2; i++)
		{
			ColorData newNode1InsideColor = new ColorData($"P{i} Node {0} Inside Color");
			ColorData newNode1OutsideColor = new ColorData($"P{i} Node {0} Outside Color");
			ColorData newNode1ParticlesColor = new ColorData($"P{i} Node {0} Particles Color");

			ColorData newNode2InsideColor = new ColorData($"P{i} Node {1} Inside Color");
			ColorData newNode2OutsideColor = new ColorData($"P{i} Node {1} Outside Color");
			ColorData newNode2ParticlesColor = new ColorData($"P{i} Node {1} Particles Color");

			ColorData oldNode1InsideColor = new ColorData($"P{i} Node {1} Inside Color");
			ColorData oldNode1OutsideColor = new ColorData($"P{i} Node {1} Outside Color");
			ColorData oldNode1ParticlesColor = new ColorData($"P{i} Node {1} Particles Color");

			ColorData oldNode2InsideColor = new ColorData($"P{i} Node {2} Inside Color");
			ColorData oldNode2OutsideColor = new ColorData($"P{i} Node {2} Outside Color");
			ColorData oldNode2ParticlesColor = new ColorData($"P{i} Node {2} Particles Color");

			//set first node colors
			//old node 1 uses same data key as new node 2, so we're shifting this one first
			newNode1InsideColor.Set(oldNode1InsideColor.Get());
			newNode1OutsideColor.Set(oldNode1OutsideColor.Get());
			newNode1ParticlesColor.Set(oldNode1ParticlesColor.Get());

			//set second node colors
			newNode2InsideColor.Set(oldNode2InsideColor.Get());
			newNode2OutsideColor.Set(oldNode2OutsideColor.Get());
			newNode2ParticlesColor.Set(oldNode2ParticlesColor.Get());

			PlayerPrefs.DeleteKey(oldNode2InsideColor.PlayerPreferenceKey);
			PlayerPrefs.DeleteKey(oldNode2OutsideColor.PlayerPreferenceKey);
			PlayerPrefs.DeleteKey(oldNode2ParticlesColor.PlayerPreferenceKey);

			//delete old lightning color
			PlayerPrefs.DeleteKey($"P{i} Lightning Color");

			PlayerPrefs.Save();
		}

		//add player 3 and 4 colors
		for(int i = 2; i < 4; i++)
        {
			DefaultPlayerColors defaultColors = ColorManager.DefaultPlayerColors[i];

			ColorData node1InsideColor = new ColorData($"P{i} Node {0} Inside Color");
			ColorData node1OutsideColor = new ColorData($"P{i} Node {0} Outside Color");
			ColorData node1ParticlesColor = new ColorData($"P{i} Node {0} Particles Color");

			ColorData node2InsideColor = new ColorData($"P{i} Node {1} Inside Color");
			ColorData node2OutsideColor = new ColorData($"P{i} Node {1} Outside Color");
			ColorData node2ParticlesColor = new ColorData($"P{i} Node {1} Particles Color");

			node1InsideColor.Set(defaultColors.NodeColors[0].InsideColor);
			node1OutsideColor.Set(defaultColors.NodeColors[0].OutsideColor);
			node1ParticlesColor.Set(defaultColors.NodeColors[0].ParticleColor);
			node2InsideColor.Set(defaultColors.NodeColors[1].InsideColor);
			node2OutsideColor.Set(defaultColors.NodeColors[1].OutsideColor);
			node2ParticlesColor.Set(defaultColors.NodeColors[1].ParticleColor);
		}


		DataManager.IsInitialSetupComplete_2_7.Set(true);
	}

	/// <summary>
	/// Handles the unlocking levels.  Levels must be campaign only!
	/// </summary>
	/// <returns>The message to display if we unlocked something!</returns>
	public string HandleUnlockingLevels()
	{
		string unlockMessage = "";
		int levelNumber = Convert.ToInt16(CurrentLevel.LevelData.Name.Substring(6, 2));

		//get ones place
		int onesPlace = Convert.ToInt16(CurrentLevel.LevelData.Name.Substring(7, 1));

		//get tens place
		int tensPlace = Convert.ToInt16(CurrentLevel.LevelData.Name.Substring(6, 1));

		int highestLevelUnlocked = DataManager.GetHighestCampaignLevelUnlocked(GameDifficultyManager.GameDifficulty);
		int highestLevelTierUnlocked = DataManager.GetHighestCampaignLevelTierUnlocked(GameDifficultyManager.GameDifficulty);

		//if we're on the last level
		if (levelNumber == NumberOfLevels)
		{
			bool perfectionist = true;

			//check all of the levels for appropriate ratings
			for (int i = 0; i < levelNumber; i++)
			{
				//check if we don't have a rating that's high enough for perfectionist
				if (DataManager.GetCampaignLevelPlayerScoreRating(i, GameDifficultyManager.GameDifficulty) < 5)
				{
					//can't unlock
					perfectionist = false;
					break;
				}
			}

			string message = null;

			if (perfectionist && GameDifficultyManager.GameDifficulty == GameDifficultyEnum.Hard && Challenges.HandleUnlockingChallenge(Challenges.ID_5StarHard, out message))
			{
				NotificationManager.QueueNotification(message);
			}

			if (perfectionist && Challenges.HandleUnlockingChallenge(Challenges.ID_5StarRatings, out message))
			{
				NotificationManager.QueueNotification(message);
			}

			if (Challenges.HandleUnlockingChallenge(Challenges.ID_Smile, out message))
			{
				NotificationManager.QueueNotification(message);
			}
		}
		else if (onesPlace > 0) //if we're not about to go to the next tier of levels
		{
			//just unlock the next level if we haven't already
			string levelToUnlock = (levelNumber + 1).ToString("00");

			if (DataManager.IsCampaignLevelLocked(levelNumber, GameDifficultyManager.GameDifficulty))
			{
				DataManager.UnlockCampaignLevel(levelNumber, GameDifficultyManager.GameDifficulty);
				unlockMessage = "Level " + levelToUnlock + " unlocked!";
				highestLevelUnlocked = levelNumber + 1;
			}
		}
		else
		{
			//Handle unlock achievements
			Challenges.LoadChallenges();
			string ChallengeID = "";
			switch (levelNumber)
			{
				case 10:
					ChallengeID = Challenges.ID_SportsmanshipAward;
					break;
				case 20:
					ChallengeID = Challenges.ID_20Levels;
					break;
				case 30:
					ChallengeID = Challenges.ID_30Levels;
					break;
				case 40:
					ChallengeID = Challenges.ID_40Levels;
					break;
				case 50:
					ChallengeID = Challenges.ID_50Levels;
					break;
			}
			Challenges.Clear();

			if (ChallengeID.Length > 0)
			{
				string message = "";
				if (Challenges.HandleUnlockingChallenge(ChallengeID, out message))
				{
					NotificationManager.QueueNotification(message);
				}
			}
			//end unlock achievements

			bool canUnlockNextTier = true;

			//check all of the previous/current levels for appropriate ratings
			for (int i = 0; i < levelNumber; i++)
			{
				//if rating is less than tensPlace, we can't unlock next tier
				if (DataManager.GetCampaignLevelPlayerScoreRating(i, GameDifficultyManager.GameDifficulty) < tensPlace)
				{
					//can't unlock
					canUnlockNextTier = false;
					break;
				}
			}

			if (canUnlockNextTier)
			{
				//unlock the next level
				if (DataManager.IsCampaignLevelLocked(levelNumber, GameDifficultyManager.GameDifficulty))
				{
					DataManager.UnlockCampaignLevel(levelNumber, GameDifficultyManager.GameDifficulty);
					unlockMessage = "Tier " + tensPlace.ToString("00") + " unlocked!";
					highestLevelUnlocked = levelNumber + 1;
					highestLevelTierUnlocked = tensPlace;
				}
			}
			else
			{
				unlockMessage = "You need a " + GetWordFromNumber(tensPlace) + " star rating\non every previous level \nto unlock Tier " + tensPlace.ToString("00") + ".";
			}
		}

		DataManager.SetHighestCampaignLevelUnlocked(GameDifficultyManager.GameDifficulty, highestLevelUnlocked);
		DataManager.SetHighestCampaignLevelTierUnlocked(GameDifficultyManager.GameDifficulty, highestLevelTierUnlocked);

		//if our highest level unlocked is on the edge of a tier
		//if current level isn't the highest level unlocked
		if (highestLevelUnlocked % 10 == 0 && highestLevelUnlocked != levelNumber)
		{
			bool canUnlock = true;

			int highestLevelTier = Mathf.FloorToInt(highestLevelUnlocked / 10f);
			//check all of the previous/current levels for appropriate ratings
			for (int i = 0; i < highestLevelUnlocked; i++)
			{
				//if rating is less than highest level tier, can't unlock level
				if (DataManager.GetCampaignLevelPlayerScoreRating(i, GameDifficultyManager.GameDifficulty) < highestLevelTier)
				{
					//can't unlock
					canUnlock = false;
					break;
				}
			}

			if (canUnlock)
			{
				//unlock the next level
				string levelToUnlock = (highestLevelUnlocked + 1).ToString("00");
				if (DataManager.IsCampaignLevelLocked(highestLevelUnlocked, GameDifficultyManager.GameDifficulty))
				{
					DataManager.UnlockCampaignLevel(highestLevelUnlocked, GameDifficultyManager.GameDifficulty);
					NotificationManager.QueueNotification("You unlocked Tier " + highestLevelTier.ToString("00") + "!");
					highestLevelUnlocked += 1;

					DataManager.SetHighestCampaignLevelUnlocked(GameDifficultyManager.GameDifficulty, highestLevelUnlocked);
				}
			}

		}
		return unlockMessage;
	}

	public string GetWordFromNumber(int oneToFive)
	{
		string number = "";
		switch (oneToFive)
		{
			case 1:
				number = "one";
				break;
			case 2:
				number = "two";
				break;
			case 3:
				number = "three";
				break;
			case 4:
				number = "four";
				break;
			case 5:
			default:
				number = "five";
				break;
		}

		return number;
	}

	public ReasonForGameEndEnum ReasonForGameEnd = ReasonForGameEndEnum.None;

	public string MapReasonsForLossToString(ReasonForGameEndEnum reasonForGameEnd)
	{
		switch (reasonForGameEnd)
		{
			case ReasonForGameEndEnum.HitOffScreen:
			case ReasonForGameEndEnum.HitSplitOffScreen:
				return "You let a ring go off the screen.";
			case ReasonForGameEndEnum.NoHitContactWithNode:
				return "You let one of your nodes hit a red node";
			case ReasonForGameEndEnum.NoHitContactWithElectricity:
				return "You let your electricity hit a red node";
			case ReasonForGameEndEnum.Mismatch:
				return "You matched the wrong color node.";
			case ReasonForGameEndEnum.Win:
				return "You won!";
			default:
				return "";
		}
	}

	public LevelTypeEnum TheLevelSelectionMode;

	public bool IsNewLevel;

	public GameObject ActivityIndicatorPrefab;

	public AudioManager AudioManager;
	public SoundEffectManager SoundEffectManager;
	public DefaultButtonStyleTemplate DefaultButtonStyleTemplate;

	public Camera Cam;

	public int score = 0;
	public int ringsCollected = 0;

	public bool IsPaused;

	public int resumeCountNormalFontSize; //TODO: this is only necessary because I'm sharing text object with "PAUSED"

	public int LocalHighScore = 0;

	public GameObject ColorSelectionSwabPrefab;

	public GameObject LevelInfoPrefab;

	public AppStateEnum AppState;

	public int Hit1HitCount, Hit2HitCount, HitSplit1HitCount, HitSplit2HitCount, NoHitHitCount;
	public int TimePlayedHours, TimePlayedMinutes, TimePlayedSeconds;
	public float TimePlayedMilliseconds;
	public int GamesPlayed;

	//Sound Screen
	public bool MusicOn = true;
	public bool SfxOn = true;

	public bool ShouldShowFps;

	//TODO: move these?
	public int LevelsPerPage = 5;
	public int LevelsDisplayingStart = -1;
	public int LevelsDisplayingEnd = -1;

	public FpsCounter FpsCounter;

	// Update is called once per frame
	void Update()
	{
		if (!IsPaused)
		{
			Grid.Run();
		}

		AudioManager.Run(Time.deltaTime);

		if (NotificationManager.IsActive) //TODO: move this into its own thing
		{
			if (NotificationManager.IsOpening || NotificationManager.IsClosing)
			{
				NotificationManager.Run(Time.deltaTime);
			}
			else
			{
				if (InputActions.FindActionMap("Notification Manager").FindAction("Accept").triggered || InputActions.FindActionMap("Notification Manager").FindAction("Back").triggered)
				{
					SoundEffectManager.PlaySelect();
					NotificationManager.Close();
					return;
				}
			}
			return;
		}

		switch (AppState)
		{
			case AppStateEnum.Loading:
				HandleLoading();
				break;
		}
	}

	void HandleLoading()
	{
		if (HandleBack())
			Application.Quit();

		LoadScene("Start");
	}

	public void LoadScene(string sceneToLoad)
	{
		AppState = AppStateEnum.InBetweenScenes;
		SceneManager.LoadScene(sceneToLoad);
	}


	private void _onSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		Cam = Camera.main;

		switch (scene.name)
		{
			case "Start":
				AppState = AppStateEnum.StartScreen;
				break;
			case "GameModeSelection":
				AppState = AppStateEnum.GameModeSelection;
				break;
			case "Level Editor":
				AppState = AppStateEnum.LevelEditor;
				break;
			case "Levels":
				AppState = AppStateEnum.LevelSelection;
				break;
			case "LevelEditorSelection":
				AppState = AppStateEnum.LevelEditorSelectionScreen;
				break;
			case "Multiplayer Controller Selection":
				AppState = AppStateEnum.MultiplayerControllerSelection;
				break;
			case "Instructions":
				AppState = AppStateEnum.InstructionsScreen;
				break;
			case "Options":
				AppState = AppStateEnum.OptionsScreen;
				break;
			case "Achievements":
				AppState = AppStateEnum.AchievementsScreen;
				break;
			case "Graphics":
				AppState = AppStateEnum.GraphicsScreen;
				break;
			case "Sound":
				AppState = AppStateEnum.SoundScreen;
				break;
			case "Account":
				AppState = AppStateEnum.AccountScreen;
				break;
			case "Color Selection":
				AppState = AppStateEnum.ColorSelectionScreen;
				break;
			case "Statistics":
				AppState = AppStateEnum.StatisticsScreen;
				break;
			case "Local Statistics":
				AppState = AppStateEnum.LocalStatisticsScreen;
				break;
			case "Game":
				AppState = AppStateEnum.Game;
				break;
			case "End":
				AppState = AppStateEnum.EndScreen;
				break;
		}

		switch (AppState)
		{
			default:
				Cursor.visible = true;
				break;
			case AppStateEnum.Game:
				Cursor.visible = false;
				break;
		}

		bool shouldKeepMenuBackgroundNodes = true;
		bool shouldKeepMenuBackgroundSparkleParticles = true;

		if (AppState == AppStateEnum.Game || AppState == AppStateEnum.LevelEditor)
		{
			shouldKeepMenuBackgroundSparkleParticles = false;
			shouldKeepMenuBackgroundNodes = false;
		}
		else if (AppState == AppStateEnum.MultiplayerControllerSelection)
		{
			shouldKeepMenuBackgroundNodes = false;
		}

		MenuBackgroundManager.SetNodesActive(shouldKeepMenuBackgroundNodes);
		MenuBackgroundManager.SetSparkleParticlesActive(shouldKeepMenuBackgroundSparkleParticles);

		if (MusicOn)
		{
			if (AppState == AppStateEnum.Game || AppState == AppStateEnum.EndScreen)
			{
				AudioManager.SwitchToGameMusic();
			}
			else
			{
				AudioManager.SwitchToMenuMusic();
			}

			AudioManager.FadeIn(1);
		}

		FpsCounter.gameObject.SetActive(ShouldShowFps);

		InputManager.ToggleInputs(AppState, NotificationManager.IsActive);

		if (NotificationManager.IsRequested)
			NotificationManager.Activate();
	}

	public void ResetLevelDisplayNumbers()
	{
		if (TheLevelSelectionMode == LevelTypeEnum.Campaign)
			LevelsPerPage = 5;
		else
			LevelsPerPage = 3;

		LevelsDisplayingStart = 0;
		LevelsDisplayingEnd = LevelsPerPage;
	}

	public void RestartGame()
	{
		LoadScene("Game");
	}

	public bool HandleBack() //TODO: rework this to be a button listener per scene manager
	{
		return InputActions.FindActionMap("Menu").FindAction("Back").triggered;
	}

	static string[] tempSplit;
	public static Vector3 ConvertStringToVector3(string strV3)
	{
		tempSplit = strV3.Substring(1).Replace(")", "").Split(',');

		return new Vector3(Convert.ToSingle(tempSplit[0]),
						   Convert.ToSingle(tempSplit[1]),
						   Convert.ToSingle(tempSplit[2]));
	}

	public int PotentialMaxSurvivalScore;

	public void SetStatistics()
	{
		PlayerPrefs.SetInt("Hit1HitCount", Hit1HitCount);
		PlayerPrefs.SetInt("Hit2HitCount", Hit2HitCount);
		PlayerPrefs.SetInt("HitSplit1HitCount", HitSplit1HitCount);
		PlayerPrefs.SetInt("HitSplit2HitCount", HitSplit2HitCount);
		PlayerPrefs.SetInt("NoHitHitCount", NoHitHitCount);
		PlayerPrefs.SetInt("TimePlayedHours", TimePlayedHours);
		PlayerPrefs.SetInt("TimePlayedMinutes", TimePlayedMinutes);
		PlayerPrefs.SetInt("TimePlayedSeconds", TimePlayedSeconds);
		PlayerPrefs.SetInt("TimePlayedMilliseconds", (int)Mathf.Round(TimePlayedMilliseconds));
		PlayerPrefs.SetInt("GamesPlayed", GamesPlayed);

		string unlockMessage = "";
		if (HitSplit1HitCount + HitSplit2HitCount >= 500)
		{
			if (Challenges.HandleUnlockingChallenge(Challenges.ID_Gymnast, out unlockMessage))
				NotificationManager.QueueNotification(unlockMessage);
		}

		unlockMessage = "";
		if (TimePlayedHours >= 2)
		{
			if (Challenges.HandleUnlockingChallenge(Challenges.ID_MarathonMan, out unlockMessage))
				NotificationManager.QueueNotification(unlockMessage);
		}
	}

	public void ClampObjectIntoView(Transform t)
    {
		_clampObjectIntoView(t, ClampXOffset, ClampYOffset);
	}

	// Clamp object into view, requires a transform and an offset
	private void _clampObjectIntoView(Transform t, float clampBorderXOffset, float clampBorderYOffset) 
	{	
		// Set limits within the frustrum of the camera
		Vector3 objectPosition = t.position;

		// Clamp top
		if (objectPosition.y > TopY - clampBorderYOffset) 
		{
			t.position = new Vector3(t.position.x,
			                         TopY - clampBorderYOffset,
			                         t.position.z);
		} 
		else if (objectPosition.y < BotY + clampBorderYOffset) // Clamp bottom
		{
			t.position = new Vector3(t.position.x,
			                         BotY + clampBorderYOffset,
			                         t.position.z);
		}
		
		// Clamp left
		if (objectPosition.x < LeftX + clampBorderXOffset)
		{
			t.position = new Vector3(LeftX + clampBorderXOffset,
			                         t.position.y,
			                         t.position.z);
		}
		else if (objectPosition.x > RightX - clampBorderXOffset) // Clamp right
		{
			t.position = new Vector3(RightX - clampBorderXOffset,
			                         t.position.y,
			                         t.position.z);
		}
	}

	public GameObject CreateActivityIndicator(Vector3 pos)
	{
		GameObject activityIndicator = (GameObject)Instantiate(ActivityIndicatorPrefab, pos, Quaternion.identity);
		activityIndicator.GetComponent<ActivityIndicator>().SetOriginalPos(pos);
		StartCoroutine(RunActivityIndicator(activityIndicator));
		return activityIndicator;
	}

	public IEnumerator RunActivityIndicator(GameObject activityIndicator)
	{
		ActivityIndicator activityIndicatorScript = activityIndicator.GetComponent<ActivityIndicator>();
		while(activityIndicator != null)
		{
			activityIndicatorScript.Move();
			yield return null;
		}
	}

	public void PlayExplosionSound(SoundEffectManager.PitchToPlay pitchToPlay)
	{
		SoundEffectManager.PlayExplosionAtPitch(pitchToPlay);
	}

	public void PlayGameOverSound()
	{
		SoundEffectManager.PlayGameOver();
	}
}