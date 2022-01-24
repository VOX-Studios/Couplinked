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

        private CameraShake _cameraShake;

        [SerializeField]
        NoHitManager noHitManager;

        [SerializeField]
        HitManager hitManager;

        [SerializeField]
        HitSplitManager hitSplitManager;

        [SerializeField]
        ExplosionManager explosionManager;

        [SerializeField]
        ScoreJuiceManager scoreJuiceManager;

        //------------------------------------------
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

        private NodePair[] _nodePairs;
        private GameInput[] _gameInputs;
        private TeamManager _teamManager;
        //------------------------------------------

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            if (_gameManager.GameSetupInfo.IsSinglePlayer)
                _setupSinglePlayer();
            else
                _setupMultiplayer();

            _cameraShake = new CameraShake();

            hitManager.Initialize();
            noHitManager.Initialize();
            hitSplitManager.Initialize();
            explosionManager.Initialize(_gameManager.DataManager);
            scoreJuiceManager.start();

            _gameManager.isPaused = false;
            _gameManager.isResuming = false;
            _cameraShake.Initialize(_gameManager);

            _gameManager.score = 0;
            _gameManager.ringsCollected = 0;

            //if survival mode
            if(_gameManager.CurrentLevel == null)
            {
                _survivalHandler = new SurvivalHandler(
                    gameManager: _gameManager,
                    gameplayUtility: new GameplayUtility(_gameManager.Grid.Logic),
                    gameSceneManager: this,
                    hitManager: hitManager,
                    hitSplitManager: hitSplitManager,
                    noHitManager: noHitManager,
                    gameInputs: _gameInputs,
                    nodePairs: _nodePairs,
                    teamManager: _teamManager
                    );

                _survivalHandler.Start();
            }
            else
            {
                _levelHandler = new LevelHandler(
                    gameManager: _gameManager,
                    gameplayUtility: new GameplayUtility(_gameManager.Grid.Logic),
                    gameSceneManager: this,
                    hitManager: hitManager,
                    hitSplitManager: hitSplitManager,
                    noHitManager: noHitManager,
                    gameInputs: _gameInputs,
                    nodePairs: _nodePairs,
                    teamManager: _teamManager
                    );

                _levelHandler.Start();
            }

            _startGame();
        }

        private void _setupSinglePlayer()
        {
            int playerIndex = 0; //TODO: change this
            Team team = new Team(playerIndex);
            _teamManager = new TeamManager()
            {
                Teams = team
            };

            _nodePairs = new NodePair[1];
            NodePair nodePair = _makeNodePair(playerIndex);
            _nodePairs[0] = nodePair;

            _gameInputs = new GameInput[1];
            InputActionMap gameInputActionMap = _gameManager.InputActions.FindActionMap("Game");
            GameInput gameInput = new GameInput(gameInputActionMap);
            _gameInputs[0] = gameInput;
        }

        private void _setupMultiplayer()
        {
            Team coOpTeam = new Team(-1, new List<Team>());
            _nodePairs = new NodePair[_gameManager.PlayerManager.Players.Count];
            _gameInputs = new GameInput[_gameManager.PlayerManager.Players.Count];

            for (int i = 0; i < _gameManager.PlayerManager.Players.Count; i++)
            {
                PlayerInput playerInput = _gameManager.PlayerManager.Players[i];

                playerInput.Lobby.Disable();
                playerInput.Game.Enable();

                NodePair nodePair = _makeNodePair(i);
                _nodePairs[i] = nodePair;
                _gameInputs[i] = playerInput.Game;

                coOpTeam.SubTeams.Add(new Team(i));
            }

            _teamManager = new TeamManager()
            {
                Teams = coOpTeam
            };
        }

        private NodePair _makeNodePair(int playerIndex)
        {
            GameObject node1GameObject = GameObject.Instantiate(_nodePrefab);
            GameObject node2GameObject = GameObject.Instantiate(_nodePrefab);

            node1GameObject.name = "Node1";
            node2GameObject.name = "Node2";

            node1GameObject.transform.SetParent(_foreground, false);
            node2GameObject.transform.SetParent(_foreground, false);

            Node node1 = node1GameObject.GetComponent<Node>();
            Node node2 = node2GameObject.GetComponent<Node>();

            node1.TeamId = playerIndex;
            node2.TeamId = playerIndex;
            node1.HitType = HitTypeEnum.Hit1;
            node2.HitType = HitTypeEnum.Hit2;

            PlayerColorData playerColorData = _gameManager.DataManager.PlayerColors[playerIndex];

            node1.SetColors(playerColorData.Node1InsideColor.Get(), playerColorData.Node1OutsideColor.Get());
            node2.SetColors(playerColorData.Node2InsideColor.Get(), playerColorData.Node2OutsideColor.Get());

            node1.SetParticleColor(playerColorData.Node1ParticlesColor.Get());
            node2.SetParticleColor(playerColorData.Node2ParticlesColor.Get());

            //disable lightning manager for very easy mode
            LightningManager lightningManager = null;

            if (_gameManager.GameDifficultyManager.GameDifficulty != GameDifficultyEnum.VeryEasy)
            {

                GameObject lightningManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
                lightningManager = lightningManagerGameObject.GetComponent<LightningManager>();

                lightningManager.Initialize(_midground, playerColorData.LightningColor.Get());
            }

            NodePair nodePair = new NodePair(node1, node2, lightningManager);

            _gameManager.Grid.Logic.ApplyExplosiveForce(20f, node1.transform.position, 1f);
            _gameManager.Grid.Logic.ApplyExplosiveForce(20f, node2.transform.position, 1f);

            _setupNodeTrail(node1.ParticleSystem);
            _setupNodeTrail(node2.ParticleSystem);

            return nodePair;
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

                for(int i = 0; i < _nodePairs.Length; i++)
                {
                    NodePair nodePair = _nodePairs[i];
                    nodePair.Node1.ParticleSystem.Play();
                    nodePair.Node2.ParticleSystem.Play();
                }

                for (int i = 0; i < explosionManager.ActiveExplosions.Count; i++)
                {
                    explosionManager.ActiveExplosions[i].GetComponent<ParticleSystem>().Play();
                }
            }

            if(_gameManager.isPaused)
            {
                if (_gameInputs.Any(input => input.UnpauseInput))
                {
                    _startResume();
                    return;
                }
                else if(_gameInputs.Any(input => input.ExitInput))
                {
                    _exit();
                    return;
                }
            }
            else
            {
                if (_gameInputs.Any(input => input.PauseInput))
                {
                    _pause();
                    return;
                }
            }

            //Don't update if we're paused
            if (_gameManager.isPaused)
                return;

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
                _levelHandler.Run(Time.deltaTime);
            }
            else
            {
                _survivalHandler.Run(Time.deltaTime);
            }

            noHitManager.Run(Time.deltaTime);
            hitManager.Run(Time.deltaTime);
            hitSplitManager.Run(Time.deltaTime);
            explosionManager.Run();
            scoreJuiceManager.Run(Time.deltaTime);

            _cameraShake.Run(Time.deltaTime);

            _scoreText.text = _gameManager.score.ToString();

            if (_gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win && _gameManager.GameState == GameStateEnum.Game)
                EndGame(_gameManager.ReasonForGameEnd);
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
            HandleExitMidGame();
            _clearGame();

            _gameManager.isPaused = false;

            string unlockMessage = "";
            if (!_gameManager.GameSetupInfo.IsSinglePlayer)
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


            for (int i = 0; i < _nodePairs.Length; i++)
            {
                NodePair nodePair = _nodePairs[i];
                nodePair.Node1.ParticleSystem.Pause();
                nodePair.Node2.ParticleSystem.Pause();
            }

            for (int i = 0; i < explosionManager.ActiveExplosions.Count; i++)
            {
                explosionManager.ActiveExplosions[i].GetComponent<ParticleSystem>().Pause();
            }

            _resumeCountText.fontSize = (int)Mathf.Ceil(_gameManager.resumeCountNormalFontSize / 2f);
            _resumeCountText.text = "PAUSED";
            _resumeCountText.gameObject.SetActive(true);
        }

        void HandleExitMidGame()
        {
            
        }

        void OnApplicationQuit()
        {
            if (_gameManager.GameState == GameStateEnum.Game)
            {
                HandleExitMidGame();
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                HandleExitMidGame();
            }
        }

        private void _startGame()
        {
            _gameManager.resumeCountNormalFontSize = _resumeCountText.fontSize;
            _resumeCountText.gameObject.SetActive(false);

            _gameManager.PotentialMaxSurvivalScore = 0;
            _cameraShake.ClearShake();
            _gameManager.ReasonForGameEnd = ReasonForGameEndEnum.None;

            _gameManager.SoundEffectManager.ResetVolume();
        }

        public void Shake()
        {
            _cameraShake.StartShake(.35f);
        }

        public Vector3 GetCamOriginalPos()
        {
            return _cameraShake.OriginalPos;
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

            for (int i = hitManager.activeHits.Count - 1; i >= 0; i--)
            {
                hitManager.DeactivateHit(i);
            }

            for (int i = noHitManager.activeNoHits.Count - 1; i >= 0; i--)
            {
                noHitManager.deactivateNoHit(i);
            }

            for (int i = hitSplitManager.activeHitSplits.Count - 1; i >= 0; i--)
            {
                hitSplitManager.DeactivateHitSplit(i);
            }

            for (int i = explosionManager.ActiveExplosions.Count - 1; i >= 0; i--)
            {
                explosionManager.DeactivateExplosion(i);
            }

            for (int i = scoreJuiceManager.activeScoreJuices.Count - 1; i >= 0; i--)
            {
                scoreJuiceManager.deactivateScoreJuice(i);
            }

            _gameManager.SetStatistics();
            _cameraShake.ClearShake();
        }

        /// <summary>
        /// Ends the game if the right condistions are met.
        /// </summary>
        /// <returns><c>true</c>, if game was ended, <c>false</c> otherwise.</returns>
        /// <param name="reasonForGameEnd">Reason for game end.</param>
        public bool EndGame(ReasonForGameEndEnum reasonForGameEnd)
        {
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

        public void AddToScore(Vector3 position, Vector3 nodePosition)
        {
            _gameManager.ringsCollected++;

            float screenX = Camera.main.WorldToViewportPoint(position).x;
            screenX = Mathf.Clamp(screenX, 0.1f, 1);
            int points = (int)Mathf.Round(screenX * 10); //will get 1 to 10
            _gameManager.score += points; //TODO: multiply by time so score becomes exponential?
            string unlockMessage = "";

            if (points == 1 && position.x < nodePosition.x)
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
            scoreJuiceManager.SpawnScoreJuice(
                pos: position, 
                score: points,
                direction: (position - nodePosition).normalized
                );
        }

        public void AddExplosion(Vector3 position, Color color)
        {
            explosionManager.ActivateExplosion(position, color);
        }
    }
}
