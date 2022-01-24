using System.Collections.Generic;

public class GameDifficultyManager 
{
	public GameDifficultyEnum GameDifficulty { get; private set; }

	public float NodeSpeed { get; private set; }
	private readonly Dictionary<GameDifficultyEnum, float> _nodeSpeed = new Dictionary<GameDifficultyEnum, float>()
	{
		{  GameDifficultyEnum.VeryEasy, 7 },
		{  GameDifficultyEnum.Easy, 7 },
		{  GameDifficultyEnum.Hard, 10 },
	};

	public float ObjectSpeed { get; private set; }
	private readonly Dictionary<GameDifficultyEnum, float> _objectSpeed = new Dictionary<GameDifficultyEnum, float>()
	{
		{  GameDifficultyEnum.VeryEasy, 3 },
		{  GameDifficultyEnum.Easy, 3 },
		{  GameDifficultyEnum.Hard, 7 },
	};

	public float BackgroundParticleSpawnInterval { get; private set; }
	private float _backgroundParticleBaseSpawnInterval = .3f;

	public float BackgroundParticleSpeed { get; private set; }
	private readonly Dictionary<GameDifficultyEnum, float> _backgroundParticleSpeed = new Dictionary<GameDifficultyEnum, float>()
	{
		{  GameDifficultyEnum.VeryEasy, 2 },
		{  GameDifficultyEnum.Easy, 2 },
		{  GameDifficultyEnum.Hard, 5 },
	};

	public float GameTimeModifier { get; private set; }

	public float LevelStartDelay { get; private set; }
	private readonly Dictionary<GameDifficultyEnum, float> _levelStartDelay = new Dictionary<GameDifficultyEnum, float>()
	{
		{  GameDifficultyEnum.VeryEasy, 4 },
		{  GameDifficultyEnum.Easy, 4 },
		{  GameDifficultyEnum.Hard, 3 },
	};

	public void ChangeDifficulty(GameDifficultyEnum gameDifficulty)
    {
		GameDifficulty = gameDifficulty;

		GameTimeModifier = _objectSpeed[gameDifficulty] / _objectSpeed[GameDifficultyEnum.Hard];
		LevelStartDelay = _levelStartDelay[gameDifficulty] * GameTimeModifier;

		NodeSpeed = _nodeSpeed[gameDifficulty];
		ObjectSpeed = _objectSpeed[gameDifficulty];
		BackgroundParticleSpawnInterval = _backgroundParticleBaseSpawnInterval / (_backgroundParticleSpeed[gameDifficulty] / _backgroundParticleSpeed[GameDifficultyEnum.Hard]);
		BackgroundParticleSpeed = _backgroundParticleSpeed[gameDifficulty];	
	}
}
