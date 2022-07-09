using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class GameSceneManager : MonoBehaviour, IHitCollisionHandler
    {
        private GameManager _gameManager;

        [SerializeField]
        private Text _resumeCountText;

        [SerializeField]
        private Text _scoreText;

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

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

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
                    node.LightIndex = _gameManager.Grid.ColorManager.GetLightIndex();
                    _gameManager.Grid.ColorManager.SetLightColor(node.LightIndex, node.OutsideColor);
                }
            }

            CameraShake = new CameraShake();

            _hitManager.Initialize();
            _noHitManager.Initialize();
            _hitSplitManager.Initialize();
            _explosionManager.Initialize(_gameManager.DataManager);
            _scoreJuiceManager.Initialize();

            _gameManager.isPaused = false;
            _gameManager.isResuming = false;
            CameraShake.Initialize(_gameManager);

            _gameManager.score = 0;
            _gameManager.ringsCollected = 0;

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
                    explosionManager: _explosionManager
                    );

                _survivalHandler.Initialize();
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
                    nodePairs: _nodePairs
                    );

                _levelHandler.Initialize();
            }

            _startGame();
        }

        private void _setupSinglePlayer()
        {
            int teamIndex = 0;

            _nodePairs = new NodePairing[1];
            NodePairing nodePair = _makeNodePair(teamIndex, 2);
            _setNodeColors(nodePair, 0);
            _nodePairs[0] = nodePair;

            //TODO: pass player input

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

            //disable lightning manager for very easy mode
            LightningManager lightningManager = null; //TODO: add more lightning managers

            if (_gameManager.GameDifficultyManager.GameDifficulty != GameDifficultyEnum.VeryEasy)
            {
                GameObject lightningManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
                lightningManager = lightningManagerGameObject.GetComponent<LightningManager>();

                lightningManager.Initialize(_midground);
            }

            NodePairing nodePair = new NodePairing(nodes, lightningManager);

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
            }

            nodePairing.LightningManager.SetLightningColor(playerColorData.LightningColor.Get());
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
                }

                return;
            }

            for(int i = 0; i < nodePairing.Nodes.Count; i++)
            {
                DefaultNodeColors nodeColors = _gameManager.ColorManager.DefaultPlayerColors[playerInputs[i].PlayerSlot].NodeColors[0];

                Node node = nodePairing.Nodes[i];
                node.SetColors(nodeColors.InsideColor, nodeColors.OutsideColor);
                node.SetParticleColor(nodeColors.ParticleColor);
            }
        }

        public void OnHitCollision(Hit hit, Collider2D other)
        {
            //if we've got a level loaded (not survival)
            if (_gameManager.CurrentLevel != null)
            {
                _levelHandler.OnHitCollision(hit, other);
            }
            else
            {
                _survivalHandler.OnHitCollision(hit, other);
            }
        }

        public void OnHitSplitCollision(HitSplit hitSplit, Collider2D other)
        {
            //if we've got a level loaded (not survival)
            if (_gameManager.CurrentLevel != null)
            {
                _levelHandler.OnHitSplitCollision(hitSplit, other);
            }
            else
            {
                _survivalHandler.OnHitSplitCollision(hitSplit, other);
            }
        }

        public void OnNoHitCollision(NoHit noHit, Collider2D other)
        {
            //if we've got a level loaded (not survival)
            if (_gameManager.CurrentLevel != null)
            {
                _levelHandler.OnNoHitCollision(noHit, other);
            }
            else
            {
                _survivalHandler.OnNoHitCollision(noHit, other);
            }
        }

        void Update()
        {
            if (_gameManager.isResuming)
            {
                _resumeCount -= Time.deltaTime;
                _resumeCountText.text = Mathf.Ceil(_resumeCount).ToString();

                if (_resumeCount > 0)
                    return;

                _resumeCountText.gameObject.SetActive(false);
                _gameManager.isResuming = false;
                _gameManager.isPaused = false;

                foreach(NodePairing nodePair in _nodePairs)
                {
                    foreach(Node node in nodePair.Nodes)
                    {
                        node.ParticleSystem.Play();
                    }
                }

                _explosionManager.Play();
            }

            if(_gameManager.isPaused)
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
                if (_gameInputs.Any(inputs => inputs.Any(input => input.PauseInput)))
                {
                    _pause();
                    return;
                }
            }

            foreach (NodePairing nodePairing in _nodePairs)
            {
                foreach (Node node in nodePairing.Nodes)
                {
                    _gameManager.Grid.ColorManager.SetLightPosition(node.LightIndex, node.transform.position);
                }
            }

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

            _noHitManager.Run(_gameManager.isPaused, Time.deltaTime);
            _hitManager.Run(_gameManager.isPaused, Time.deltaTime);
            _hitSplitManager.Run(_gameManager.isPaused, Time.deltaTime);

            //nothing left to do if we're paused
            if (_gameManager.isPaused)
                return;

            //if we've got a level loaded (not survival)
            if (_gameManager.CurrentLevel != null)
            {
                _levelHandler.Run(Time.deltaTime);
            }
            else
            {
                _survivalHandler.Run(Time.deltaTime);
            }

            
            _explosionManager.Run();
            _scoreJuiceManager.Run(Time.deltaTime);

            CameraShake.Run(Time.deltaTime);

            _scoreText.text = _gameManager.score.ToString();

            if (_gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win && _gameManager.GameState == GameStateEnum.Game)
            {
                EndGame(_gameManager.ReasonForGameEnd);
            }
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
            _gameManager.ReasonForGameEnd = ReasonForGameEndEnum.Quit;
            _gameManager.SoundEffectManager.PlayBack();
            _clearGame();

            _gameManager.isPaused = false;

            string unlockMessage = "";
            if (_gameManager.GameSetupInfo.Teams.Count > 1)
            {
                _gameManager.LoadScene(SceneNames.MultiplayerGameModeSelection);
            }
            else if (_gameManager.CurrentLevel == null)
            {
                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_Quit, out unlockMessage))
                    _gameManager.NotificationManager.QueueNotification(unlockMessage);

                _gameManager.LoadScene(SceneNames.GameModeSelection);
            }
            else if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.LevelEditor)
            {
                _gameManager.LoadScene(SceneNames.LevelEditor);
            }
            else
            {
                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_Quit, out unlockMessage))
                    _gameManager.NotificationManager.QueueNotification(unlockMessage);

                _gameManager.LoadScene(SceneNames.Levels);
            }
        }

        private void _startResume()
        {
            if (_gameManager.isPaused && !_gameManager.isResuming)
            {
                _resumeCountText.fontSize = _gameManager.resumeCountNormalFontSize;
                _gameManager.isResuming = true;
                _resumeCount = 3;
                _resumeCountText.text = "3";
                _resumeCountText.gameObject.SetActive(true);
            }
        }

        private void _pause()
        {
            _gameManager.isPaused = true;

            foreach (NodePairing nodePair in _nodePairs)
            {
                foreach (Node node in nodePair.Nodes)
                {
                    node.ParticleSystem.Pause();
                }
            }

            _explosionManager.Pause();

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
            CameraShake.StartShake(.35f);
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

            for (int i = _hitManager.activeHits.Count - 1; i >= 0; i--)
            {
                _hitManager.DeactivateHit(i);
            }

            for (int i = _noHitManager.activeNoHits.Count - 1; i >= 0; i--)
            {
                _noHitManager.DeactivateNoHit(i);
            }

            for (int i = _hitSplitManager.activeHitSplits.Count - 1; i >= 0; i--)
            {
                _hitSplitManager.DeactivateHitSplit(i);
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
                    _gameManager.Grid.ColorManager.ReleaseLightIndex(node.LightIndex);
                    node.LightIndex = -1;
                }
            }
        }

        /// <summary>
        /// Ends the game if the right condistions are met.
        /// </summary>
        /// <returns><c>true</c>, if game was ended, <c>false</c> otherwise.</returns>
        /// <param name="reasonForGameEnd">Reason for game end.</param>
        public bool EndGame(ReasonForGameEndEnum reasonForGameEnd)
        {
            //TODO: reenable this when we're done testing
            if (reasonForGameEnd != ReasonForGameEndEnum.Win)
            {
                return false;
            }            

            _gameManager.ReasonForGameEnd = reasonForGameEnd;

            if (_gameManager.CurrentLevel != null)
            {
                switch (reasonForGameEnd)
                {
                    case ReasonForGameEndEnum.HitOffScreen:
                    case ReasonForGameEndEnum.HitSplitOffScreen:
                        return false; //ignore off screen losses for levels
                }

                //if we're custom or campaign AND we won
                if (_gameManager.TheLevelSelectionMode != LevelTypeEnum.LevelEditor && _gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win)
                {
                    int savedScore = _gameManager.DataManager.GetLevelPlayerScore(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty);
                    int playerScoreRating = _gameManager.CurrentLevel.RateScore(_gameManager.score);

                    if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.Campaign)
                    {
                        int levelNumber = Convert.ToInt16(_gameManager.CurrentLevel.LevelData.Name.Substring(6, 2));
                        if (levelNumber > 40)
                        {
                            int potentialRingsCollected = _gameManager.CurrentLevel.CalculatePotentialRingsCollected();
                            if (_gameManager.ringsCollected == potentialRingsCollected)
                            {
                                string unlockMessage = "";
                                if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_CollectEveryRingInALevel, out unlockMessage))
                                    _gameManager.NotificationManager.QueueNotification(unlockMessage);
                            }
                        }
                    }

                    if (_gameManager.score > savedScore)
                    {
                        int savedScoreRating = _gameManager.DataManager.GetLevelPlayerScoreRating(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty);

                        _gameManager.DataManager.SetLevelPlayerScore(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, _gameManager.score);
                        _gameManager.DataManager.SetLevelPlayerScoreRating(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, playerScoreRating);

                        if (savedScoreRating > 0)
                            _gameManager.NotificationManager.QueueNotification("New High Score!");
                    }
                    else if (_gameManager.CurrentLevel.MaxScore == 0)
                    {
                        _gameManager.DataManager.SetLevelPlayerScore(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, _gameManager.score);
                        _gameManager.DataManager.SetLevelPlayerScoreRating(_gameManager.CurrentLevel.LevelData.Id, _gameManager.GameDifficultyManager.GameDifficulty, playerScoreRating);
                    }


                    if (playerScoreRating > 0 && _gameManager.TheLevelSelectionMode == LevelTypeEnum.Campaign)
                    {
                        string unlockMessage = _gameManager.HandleUnlockingLevels();
                        if (unlockMessage.Length > 0)
                            _gameManager.NotificationManager.QueueNotification(unlockMessage);
                    }
                }
            }
            else //survival game
            {
                if (_gameManager.score >= 1000)
                {
                    string unlockMessage = "";
                    if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_1000Points, out unlockMessage))
                        _gameManager.NotificationManager.QueueNotification(unlockMessage);
                }

                bool newLocalHighScore = _gameManager.LocalHighScore < _gameManager.score;

                if (newLocalHighScore)
                {
                    _gameManager.LocalHighScore = _gameManager.score;
                    _gameManager.DataManager.SetSurvivalHighScore(_gameManager.LocalHighScore);
                    _gameManager.NotificationManager.QueueNotification("New High Score!");
                }
            }

            if (_gameManager.TheLevelSelectionMode != LevelTypeEnum.LevelEditor)
            {
                if (_gameManager.score == 0 && _gameManager.ReasonForGameEnd != ReasonForGameEndEnum.Quit)
                {
                    string unlockMessage = "";
                    if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_0Points, out unlockMessage))
                        _gameManager.NotificationManager.QueueNotification(unlockMessage);
                }
            }

            if (_gameManager.ReasonForGameEnd != ReasonForGameEndEnum.Win)
                _gameManager.SoundEffectManager.PlayGameOver();

            PlayerPrefs.Save();

            _gameManager.GamesPlayed++;
            _clearGame();

            _gameManager.LoadScene("End");
            return true;
        }

        public void AddToScore(Vector3 position, Node node)
        {
            _gameManager.ringsCollected++;

            float screenX = Camera.main.WorldToViewportPoint(position).x;
            screenX = Mathf.Clamp(screenX, 0.1f, 1);
            int points = (int)Mathf.Round(screenX * 10); //will get 1 to 10
            _gameManager.score += points; //TODO: multiply by time so score becomes exponential?
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
        }

        public void AddExplosion(Vector3 position, Color color)
        {
            _explosionManager.ActivateExplosion(position, color);
        }
    }
}
