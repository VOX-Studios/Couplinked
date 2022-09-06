using Assets.Scripts.Gameplay;
using Assets.Scripts.Lighting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    public class GameSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Text _resumeCountText;

        [SerializeField]
        private Text _scoreText;

        [SerializeField]
        private Text _livesText;

        private float _resumeCount;

        public CameraShake CameraShake;

        [SerializeField]
        private NoHitManager _noHitManager;

        [SerializeField]
        private HitManager _hitManager;

        [SerializeField]
        private HitSplitManager _hitSplitManager;

        [SerializeField]
        private ExplosionManager _explosionManager;

        [SerializeField]
        private ScoreJuiceManager _scoreJuiceManager;

        private SurvivalHandler _survivalHandler;
        private LevelHandler _levelHandler;

        [SerializeField]
        private GameObject _nodePrefab;

        [SerializeField]
        private GameObject _lightningManagerPrefab;

        [SerializeField]
        private Transform _midground;

        [SerializeField]
        private Transform _foreground;

        public NodePairing[] _nodePairs;
        private GameInput[][] _gameInputs;

        private GameStateEnum _gameState;

        public VignetteManager VignetteManager;
        public SideExplosionManager SideExplosionManager;

        [SerializeField]
        private ParticleSystem _backgroundParticles;

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            //disable lives text if we're not using more than one life
            if(_gameManager.GameSetupInfo.RuleSet.NumberOfLives == 1)
            {
                _livesText.transform.parent.gameObject.SetActive(false);
            }

            ParticleSystem.ShapeModule backgroundParticlesShape = _backgroundParticles.shape;
            backgroundParticlesShape.scale = new Vector3(GameManager.RightX - GameManager.LeftX, GameManager.TopY - GameManager.BotY, 1);

            VignetteManager.Initialize(_gameManager);

            //if there's only 1 team and 1 player
            if (_gameManager.GameSetupInfo.Teams.Count == 1 && _gameManager.GameSetupInfo.Teams[0].PlayerInputs.Count == 1)
            {
                _setupSinglePlayer();
            }
            else
            {
                _setupMultiplayer();
            }

            foreach(NodePairing nodePairing in _nodePairs)
            {
                foreach(Node node in nodePairing.Nodes)
                {
                    node.LightIndex = _gameManager.LightingManager.GetLightIndex();
                    _gameManager.LightingManager.SetLightColor(node.LightIndex, node.OutsideColor);
                }
            }

            IGameModeHandler gameModeHandler;

            //if survival mode
            if(_gameManager.CurrentLevel == null)
            {
                _survivalHandler = new SurvivalHandler(
                    gameManager: _gameManager,
                    gameSceneManager: this,
                    hitManager: _hitManager,
                    hitSplitManager: _hitSplitManager,
                    noHitManager: _noHitManager,
                    gameInputs: _gameInputs,
                    nodePairs: _nodePairs,
                    explosionManager: _explosionManager,
                    gameSetupInfo: _gameManager.GameSetupInfo
                    );

                gameModeHandler = _survivalHandler;
            }
            else
            {
                _levelHandler = new LevelHandler(
                    gameManager: _gameManager,
                    gameSceneManager: this,
                    hitManager: _hitManager,
                    hitSplitManager: _hitSplitManager,
                    noHitManager: _noHitManager,
                    gameInputs: _gameInputs,
                    nodePairings: _nodePairs,
                    explosionManager: _explosionManager
                    );

                gameModeHandler = _levelHandler;
            }


            _hitManager.Initialize(gameModeHandler);
            _noHitManager.Initialize(gameModeHandler);
            _hitSplitManager.Initialize(gameModeHandler);
            _explosionManager.Initialize(_gameManager.DataManager);
            _scoreJuiceManager.Initialize(_gameManager, this);
            SideExplosionManager.Initialize(5); //TODO: we could make this smart and have the game mode handler initialize it with num rows, but realistically we can use the max number of rows

            CameraShake = new CameraShake();
            CameraShake.Initialize(_gameManager);

            _setGameState(GameStateEnum.Playing);

            _gameManager.Score = 0;
            _gameManager.ringsCollected = 0;

            UpdateScoreText(_gameManager.Score);

            gameModeHandler.Initialize();
            _startGame();
        }

        private void _setGameState(GameStateEnum gameState)
        {
            _gameState = gameState;

            if(gameState == GameStateEnum.Paused || gameState == GameStateEnum.Resuming)
            {
                _gameManager.IsPaused = true;
            }
            else
            {
                _gameManager.IsPaused = false;
            }
        }

        private void _setupSinglePlayer()
        {
            int teamIndex = 0;

            _nodePairs = new NodePairing[1];
            NodePairing nodePair = _makeNodePair(teamIndex, 2);
            _setNodeColors(nodePair, 0);
            _nodePairs[0] = nodePair;

            //TODO: pass player input?

            _gameInputs = new GameInput[1][];
            InputActionMap gameInputActionMap = _gameManager.InputActions.FindActionMap("Game");
            GameInput gameInput = new GameInput(gameInputActionMap);
            _gameInputs[0] = new GameInput[]
            {
                gameInput
            };
        }

        private void _setupMultiplayer()
        {
            List<Team> teams = new List<Team>();
            _nodePairs = new NodePairing[_gameManager.GameSetupInfo.Teams.Count];
            _gameInputs = new GameInput[_gameManager.GameSetupInfo.Teams.Count][];

            for (int i = 0; i < _gameManager.GameSetupInfo.Teams.Count; i++)
            {
                List<PlayerInput> playerInputs = _gameManager.GameSetupInfo.Teams[i].PlayerInputs;

                _gameInputs[i] = new GameInput[playerInputs.Count];

                for (int j = 0; j < playerInputs.Count; j++)
                {
                    PlayerInput playerInput = playerInputs[j];

                    playerInput.Lobby.Disable();
                    playerInput.Game.Enable();

                    _gameInputs[i][j] = playerInput.Game;
                }
                int numberOfNodes = playerInputs.Count == 1 ? 2 : playerInputs.Count;
                NodePairing nodePair = _makeNodePair(i, numberOfNodes);
                _setNodeColors(nodePair, playerInputs);
                _nodePairs[i] = nodePair;
            }
        }

        private NodePairing _makeNodePair(int teamIndex, int numberOfNodes)
        {
            List<Node> nodes = new List<Node>();

            for (int i = 0; i < numberOfNodes; i++)
            {
                GameObject nodeGameObject = GameObject.Instantiate(_nodePrefab);
                nodeGameObject.name = $"Node {i + 1}";

                nodeGameObject.transform.SetParent(_foreground, false);

                Node node = nodeGameObject.GetComponent<Node>();

                node.TeamId = teamIndex;
                node.NodeId = i;

                _setupNodeTrail(node.ParticleSystem);

                _gameManager.Grid.Logic.ApplyExplosiveForce(20f, node.transform.position, 1f);

                nodes.Add(node);
            }

            //only add lasers if the setting is turned on
            List<LaserManager> laserManagers = null;

            if (_gameManager.GameSetupInfo.RuleSet.AreLasersOn)
            {
                laserManagers = new List<LaserManager>();
                for (int i = 0; i < numberOfNodes - 1; i++)
                {
                    GameObject laserManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
                    LaserManager laserManager = laserManagerGameObject.GetComponent<LaserManager>();

                    laserManager.Initialize(_midground);
                    laserManager.LaserNodeConnections.Node1 = nodes[i];
                    laserManager.LaserNodeConnections.Node2 = nodes[i + 1];

                    laserManagers.Add(laserManager);
                }
            }

            NodePairing nodePair = new NodePairing(nodes, laserManagers);

            return nodePair;
        }

        /// <summary>
        /// Set colors for singleplayer.
        /// </summary>
        /// <param name="nodePairing"></param>
        /// <param name="playerIndex"></param>
        private void _setNodeColors(NodePairing nodePairing, int playerIndex)
        {
            CustomPlayerColorData playerColorData = _gameManager.DataManager.PlayerColors[playerIndex];
            for (int i = 0; i < nodePairing.Nodes.Count; i++)
            {
                NodeColorData nodeColors = playerColorData.NodeColors[i];
                Node node = nodePairing.Nodes[i];
                node.SetColors(nodeColors.InsideColor.Get(), nodeColors.OutsideColor.Get());
                node.SetParticleColor(nodeColors.ParticleColor.Get());

                if (i > 0 && nodePairing.LaserManagers != null)
                {
                    nodePairing.LaserManagers[i - 1].SetLaserColor(nodePairing.Nodes[i - 1].OutsideColor, nodePairing.Nodes[i].OutsideColor);
                }
            }
        }

        /// <summary>
        /// Set colors for multiplayer.
        /// </summary>
        /// <param name="nodePairing"></param>
        /// <param name="playerInputs"></param>
        private void _setNodeColors(NodePairing nodePairing, List<PlayerInput> playerInputs)
        {
            if(playerInputs.Count == 1)
            {
                for (int i = 0; i < nodePairing.Nodes.Count; i++)
                {
                    DefaultNodeColors nodeColors = _gameManager.ColorManager.DefaultPlayerColors[playerInputs[0].PlayerSlot].NodeColors[i];

                    Node node = nodePairing.Nodes[i];
                    node.SetColors(nodeColors.InsideColor, nodeColors.OutsideColor);
                    node.SetParticleColor(nodeColors.ParticleColor);

                    if (i > 0 && nodePairing.LaserManagers != null)
                    {
                        nodePairing.LaserManagers[i - 1].SetLaserColor(nodePairing.Nodes[i - 1].OutsideColor, nodePairing.Nodes[i].OutsideColor);
                    }
                }

                return;
            }

            for(int i = 0; i < nodePairing.Nodes.Count; i++)
            {
                DefaultNodeColors nodeColors = _gameManager.ColorManager.DefaultPlayerColors[playerInputs[i].PlayerSlot].NodeColors[0];

                Node node = nodePairing.Nodes[i];
                node.SetColors(nodeColors.InsideColor, nodeColors.OutsideColor);
                node.SetParticleColor(nodeColors.ParticleColor);

                if (i > 0 && nodePairing.LaserManagers != null)
                {
                    nodePairing.LaserManagers[i - 1].SetLaserColor(nodePairing.Nodes[i - 1].OutsideColor, nodePairing.Nodes[i].OutsideColor);
                }
            }
        }

        void Update()
        {
            if (_gameState == GameStateEnum.Ending)
            {
                _handleGameEnding();
                return;
            }

            if (_gameState == GameStateEnum.Resuming)
            {
                _resumeCount -= Time.deltaTime;
                _resumeCountText.text = Mathf.Ceil(_resumeCount).ToString();

                if (_resumeCount > 0)
                {
                    return;
                }

                _resumeCountText.gameObject.SetActive(false);

                _setGameState(GameStateEnum.Playing);

                foreach(NodePairing nodePair in _nodePairs)
                {
                    foreach(Node node in nodePair.Nodes)
                    {
                        node.ParticleSystem.Play();
                    }
                }

                _explosionManager.Play();
                _backgroundParticles.Play();
            }

            if(_gameState == GameStateEnum.Paused)
            {
                if (_gameInputs.Any(inputs => inputs.Any(input => input.UnpauseInput)))
                {
                    _startResume();
                    return;
                }
                else if (_gameInputs.Any(inputs => inputs.Any(input => input.ExitInput)))
                {
                    _exit();
                    return;
                }
            }
            else
            {
                //not using Linq.Any because it creates garbage
                foreach(GameInput[] gameInputs in _gameInputs)
                {
                    foreach(GameInput gameInput in gameInputs)
                    {
                        if(gameInput.PauseInput)
                        {
                            _pause();
                            return;
                        }
                    }
                }
            }

            //if we're not paused or resuming, run the camera shake before we set our lighting
            if (_gameState != GameStateEnum.Paused && _gameState != GameStateEnum.Resuming)
            {
                CameraShake.Run(Time.deltaTime);
            }

            _setNodeLightPositions();

            _gameManager.TimePlayedMilliseconds += Time.deltaTime * 1000;

            //Handle time formatting
            if (_gameManager.TimePlayedMilliseconds >= 1000)
            {
                _gameManager.TimePlayedMilliseconds -= 1000;
                _gameManager.TimePlayedSeconds++;

                if (_gameManager.TimePlayedSeconds >= 60)
                {
                    _gameManager.TimePlayedSeconds -= 60;
                    _gameManager.TimePlayedMinutes++;

                    if (_gameManager.TimePlayedMinutes >= 60)
                    {
                        _gameManager.TimePlayedMinutes -= 60;
                        _gameManager.TimePlayedHours++;
                    }
                }
            }

            //if we've got a level loaded (not survival)
            if (_gameManager.CurrentLevel != null)
            {
                _levelHandler.Run(_gameState == GameStateEnum.Paused || _gameState == GameStateEnum.Resuming, Time.deltaTime);
            }
            else
            {
                _survivalHandler.Run(_gameState == GameStateEnum.Paused || _gameState == GameStateEnum.Resuming, Time.deltaTime);
            }

            //nothing left to do if we're paused
            if (_gameState == GameStateEnum.Paused || _gameState == GameStateEnum.Resuming)
            {
                return;
            }

            _explosionManager.Run();
            _scoreJuiceManager.Run(Time.deltaTime);
            SideExplosionManager.Run(Time.deltaTime);            

            //if we've won
            if (_gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win && _gameManager.AppState == AppStateEnum.Game)
            {
                EndGame(_gameManager.ReasonForGameEnd, true);
            }
        }

        private void _setNodeLightPositions()
        {
            foreach (NodePairing nodePairing in _nodePairs)
            {
                foreach (Node node in nodePairing.Nodes)
                {
                    _gameManager.LightingManager.SetLightPosition(node.LightIndex, node.transform.position);
                }
            }
        }

        public void UpdateScoreText(int score)
        {
            _scoreText.text = score.ToString();
        }

        public void UpdateLivesText(int lives)
        {
            if(_gameManager.GameSetupInfo.RuleSet.NumberOfLives == 1)
            {
                //only show lives text if we're using more than one life
                return;
            }

            _livesText.text = lives.ToString();
        }

        private void _handleGameEnding()
        {
            CameraShake.Run(Time.deltaTime);

            foreach (NodePairing nodePair in _nodePairs)
            {
                foreach (Node node in nodePair.Nodes)
                {
                    //TODO: don't do this every frame, just on start end
                    node.ParticleSystem.Pause();
                }
            }

            _setNodeLightPositions();

            //if we've got a level loaded (not survival)
            if (_gameManager.CurrentLevel != null)
            {
                _levelHandler.Run(true, Time.deltaTime);
            }
            else
            {
                _survivalHandler.Run(true, Time.deltaTime);
            }

            //TODO: don't do this every frame, just on start end
            _explosionManager.Pause();
            _backgroundParticles.Pause();

            VignetteManager.Run(Time.deltaTime);
            SideExplosionManager.Run(Time.deltaTime);
            _scoreJuiceManager.Run(Time.deltaTime);

            if (VignetteManager.Phase == 1)
            {
                //check for input
                if (_gameInputs.Any(inputs => inputs.Any(input => input.ExitInput || input.PauseInput || input.UnpauseInput)))
                {
                    //start phase 2
                    VignetteManager.StartClosePhase2(0);
                    return;
                }
            }

            if (VignetteManager.Phase == 2 && !VignetteManager.IsClosing)
            {
                _endGame();
            }
        }

        private void _endGame()
        {
            //TODO: switch this to a call like OnGameEnd and pass into our level/survival handlers
            if (_gameManager.CurrentLevel != null)
            {
                //if we're custom or campaign AND we won
                if (_gameManager.TheLevelSelectionMode != LevelTypeEnum.LevelEditor && _gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win)
                {
                    int savedScore = _gameManager.DataManager.GetLevelPlayerScore(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty);
                    int playerScoreRating = _gameManager.CurrentLevel.RateScore(_gameManager.Score);

                    if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.Campaign)
                    {
                        int levelNumber = Convert.ToInt16(_gameManager.CurrentLevel.LevelData.Name.Substring(6, 2));
                        //achievement is only for level 40 and beyond
                        if (levelNumber > 40)
                        {
                            int potentialRingsCollected = _gameManager.CurrentLevel.CalculatePotentialRingsCollected();
                            if (_gameManager.ringsCollected == potentialRingsCollected)
                            {
                                string unlockMessage = "";
                                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_CollectEveryRingInALevel, out unlockMessage))
                                {
                                    _gameManager.NotificationManager.QueueNotification(unlockMessage);
                                }
                            }
                        }
                    }

                    if (_gameManager.Score > savedScore)
                    {
                        int savedScoreRating = _gameManager.DataManager.GetLevelPlayerScoreRating(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty);

                        _gameManager.DataManager.SetLevelPlayerScore(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, _gameManager.Score);
                        _gameManager.DataManager.SetLevelPlayerScoreRating(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, playerScoreRating);

                        if (savedScoreRating > 0)
                        {
                            _gameManager.NotificationManager.QueueNotification("New High Score!");
                        }
                    }
                    else if (_gameManager.CurrentLevel.MaxScore == 0)
                    {
                        _gameManager.DataManager.SetLevelPlayerScore(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, _gameManager.Score);
                        _gameManager.DataManager.SetLevelPlayerScoreRating(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, playerScoreRating);
                    }

                    if (playerScoreRating > 0 && _gameManager.TheLevelSelectionMode == LevelTypeEnum.Campaign)
                    {
                        string unlockMessage = _gameManager.HandleUnlockingLevels();
                        if (unlockMessage.Length > 0)
                        {
                            _gameManager.NotificationManager.QueueNotification(unlockMessage);
                        }
                    }
                }
            }
            else //survival game
            {
                if (_gameManager.Score >= 1000)
                {
                    string unlockMessage = "";
                    if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_1000Points, out unlockMessage))
                    {
                        _gameManager.NotificationManager.QueueNotification(unlockMessage);
                    }
                }

                bool newLocalHighScore = _gameManager.LocalHighScore < _gameManager.Score;

                if (newLocalHighScore)
                {
                    _gameManager.LocalHighScore = _gameManager.Score;
                    _gameManager.DataManager.SetSurvivalHighScore(_gameManager.LocalHighScore);
                    _gameManager.NotificationManager.QueueNotification("New High Score!");
                }
            }

            if (_gameManager.TheLevelSelectionMode != LevelTypeEnum.LevelEditor)
            {
                if (_gameManager.Score == 0 && _gameManager.ReasonForGameEnd != ReasonForGameEndEnum.Quit)
                {
                    string unlockMessage = "";
                    if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_0Points, out unlockMessage))
                    {
                        _gameManager.NotificationManager.QueueNotification(unlockMessage);
                    }
                }
            }

            PlayerPrefs.Save();

            _gameManager.GamesPlayed++;
            _clearGame();

            _gameManager.LoadScene(SceneNames.End);
        }

        private void _setupNodeTrail(ParticleSystem particleSystem)
        {
            QualitySettingEnum qualitySetting = _gameManager.DataManager.TrailParticleQuality.Get();
            ParticleSystem.EmissionModule emission = particleSystem.emission;

            switch (qualitySetting)
            {
                default:
                case QualitySettingEnum.Off:
                    emission.rateOverDistanceMultiplier = 0f;
                    break;
                case QualitySettingEnum.Low:
                    emission.rateOverDistanceMultiplier = 75f;
                    break;
                case QualitySettingEnum.Medium:
                    emission.rateOverDistanceMultiplier = 150f;
                    break;
                case QualitySettingEnum.High:
                    emission.rateOverDistanceMultiplier = 300f;
                    break;
            }
        }

        private void _exit()
        {
            //TODO: this should match the end game stuff, no?

            _gameManager.ReasonForGameEnd = ReasonForGameEndEnum.Quit;
            _gameManager.SoundEffectManager.PlayBack();
            _clearGame();

            _setGameState(GameStateEnum.Playing);

            string unlockMessage = "";
            if (_gameManager.CurrentLevel == null)
            {
                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_Quit, out unlockMessage))
                {
                    _gameManager.NotificationManager.QueueNotification(unlockMessage);
                }

                _gameManager.LoadScene(SceneNames.RuleSetSelection);
            }
            else if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.LevelEditor)
            {
                _gameManager.LoadScene(SceneNames.LevelEditor);
            }
            else
            {
                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_Quit, out unlockMessage))
                {
                    _gameManager.NotificationManager.QueueNotification(unlockMessage);
                }

                _gameManager.LoadScene(SceneNames.Levels);
            }
        }

        private void _startResume()
        {
            if (_gameState != GameStateEnum.Paused)
            {
                return;
            }

            _resumeCountText.fontSize = _gameManager.resumeCountNormalFontSize;
            _setGameState(GameStateEnum.Resuming);
            _resumeCount = 3;
            _resumeCountText.text = "3";
            _resumeCountText.gameObject.SetActive(true);
        }

        private void _pause()
        {
            _setGameState(GameStateEnum.Paused);

            foreach (NodePairing nodePair in _nodePairs)
            {
                foreach (Node node in nodePair.Nodes)
                {
                    node.ParticleSystem.Pause();
                }
            }

            _explosionManager.Pause();
            _backgroundParticles.Pause();

            _resumeCountText.fontSize = (int)Mathf.Ceil(_gameManager.resumeCountNormalFontSize / 2f);
            _resumeCountText.text = "PAUSED";
            _resumeCountText.gameObject.SetActive(true);
        }

        private void _startGame()
        {
            _gameManager.resumeCountNormalFontSize = _resumeCountText.fontSize;
            _resumeCountText.gameObject.SetActive(false);

            _gameManager.PotentialMaxSurvivalScore = 0;
            CameraShake.ClearShake();
            _gameManager.ReasonForGameEnd = ReasonForGameEndEnum.None;

            _gameManager.SoundEffectManager.ResetVolume();
        }

        public void Shake()
        {
            CameraShake.StartShake(.34f);
        }

        public Vector3 GetCamOriginalPos()
        {
            return CameraShake.OriginalPos;
        }

        private void _clearGame()
        {
            switch (_gameManager.GameSetupInfo.GameMode)
            {
                case GameModeEnum.Survival:
                case GameModeEnum.SurvivalCoOp:
                    _survivalHandler.OnGameEnd();
                    break;
            }

            for (int i = _hitManager.ActiveGameEntities.Count - 1; i >= 0; i--)
            {
                _hitManager.DeactivateGameEntity(i);
            }

            for (int i = _noHitManager.ActiveGameEntities.Count - 1; i >= 0; i--)
            {
                _noHitManager.DeactivateGameEntity(i);
            }

            for (int i = _hitSplitManager.ActiveGameEntities.Count - 1; i >= 0; i--)
            {
                _hitSplitManager.DeactivateGameEntity(i);
            }

            _explosionManager.DeactiveExplosions();

            for (int i = _scoreJuiceManager.activeScoreJuices.Count - 1; i >= 0; i--)
            {
                _scoreJuiceManager.DeactivateScoreJuice(i);
            }

            _gameManager.SetStatistics();
            CameraShake.ClearShake();

            foreach (NodePairing nodePairing in _nodePairs)
            {
                foreach (Node node in nodePairing.Nodes)
                {
                    _gameManager.LightingManager.ReleaseLightIndex(node.LightIndex);
                    node.LightIndex = -1;
                }
            }
        }

        /// <summary>
        /// Starts ending the game or instantly ends the game.
        /// </summary>
        /// <param name="reasonForGameEnd">Reason for game end.</param>
        /// <param name="isInstant"></param>
        public void EndGame(ReasonForGameEndEnum reasonForGameEnd, bool isInstant = false)
        {
            //enable this when we want to test without dying
            //if (reasonForGameEnd != ReasonForGameEndEnum.Win && reasonForGameEnd != ReasonForGameEndEnum.Quit)
            //{
            //    return;
            //}

            if(reasonForGameEnd != ReasonForGameEndEnum.Win && reasonForGameEnd != ReasonForGameEndEnum.Quit)
            {
                _gameManager.SoundEffectManager.PlayGameOver();
            }

            _gameManager.ReasonForGameEnd = reasonForGameEnd;

            if (isInstant)
            {
                _gameState = GameStateEnum.Ended;
                _endGame();
            }
            else
            {
                _gameState = GameStateEnum.Ending;
                
            }
        }

        public void AddToScore(Vector3 position, Node node)
        {
            _gameManager.ringsCollected++;

            float screenX = Camera.main.WorldToViewportPoint(position).x;
            screenX = Mathf.Clamp(screenX, 0.1f, 1);
            int points = (int)Mathf.Round(screenX * 10); //will get 1 to 10
            _gameManager.Score += points; //TODO: multiply by time so score becomes exponential?
            string unlockMessage = "";

            if (points == 1 && position.x < node.transform.position.x)
            {
                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_AlmostMissedOne, out unlockMessage))
                    _gameManager.NotificationManager.QueueNotification(unlockMessage);
            }
            else if (points == 10)
            {
                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_H10, out unlockMessage))
                    _gameManager.NotificationManager.QueueNotification(unlockMessage);
            }

            //TODO: Have direction also include speed of node...or add one-tenth weighted random skew
            _scoreJuiceManager.SpawnScoreJuice(
                pos: position, 
                score: points,
                direction: (position - node.transform.position).normalized,
                scale: node.Scale
                );

            UpdateScoreText(_gameManager.Score);
        }

        public void AddExplosion(Vector3 position, Color color)
        {
            _explosionManager.ActivateExplosion(position, color);
        }
    }
}
