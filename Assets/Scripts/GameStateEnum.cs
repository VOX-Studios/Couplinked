public enum GameStateEnum : byte
{
	Loading,
	InBetweenScenes, //TODO: what is this?
	StartScreen,
	GameModeSelection,
	MultiplayerControllerSelection,

	LevelEditor,
	LevelSelection,
	LevelEditorSelectionScreen,

	//main menu
	InstructionsScreen,
	OptionsScreen,

	//options
	GraphicsScreen,
	SoundScreen,
	AccountScreen,
	AchievementsScreen,

	//display
	ColorSelectionScreen,

	//account
	StatisticsScreen,

	//statistics
	LocalStatisticsScreen,

	Game,
	EndScreen
}