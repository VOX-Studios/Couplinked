using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class EndScreenSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Button _nextLevelButton;

        [SerializeField]
        private Button _playAgainButton;

        [SerializeField]
        private Button _backButton;

        [SerializeField]
        private Text _title;

        [SerializeField]
        private LevelInfo _ratingLevelInfo;

        [SerializeField]
        private Text _finalScoreText;

        [SerializeField]
        private Text _reasonForGameEndText;

        private InputAction _nextLevelInputAction;
        private InputAction _playAgainInputAction;

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_nextLevelButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            InputActionMap menuInputActionMap = _gameManager.InputActions.FindActionMap("Menu");

            _nextLevelInputAction = menuInputActionMap.FindAction("Next Level");
            _playAgainInputAction = menuInputActionMap.FindAction("Play Again");

            GameObject MainMenuBackButton = (GameObject)GameObject.Instantiate(_gameManager.MenuBackButtonPrefab);
            MainMenuBackButton.name = "MainMenuBackButton";

            _nextLevelButton.onClick.AddListener(_handleNextLevelButton);
            _playAgainButton.onClick.AddListener(_handlePlayAgainButton);
            _backButton.onClick.AddListener(_handleBackButton);

            if (_gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win)
            {
                _title.text = "VICTORY";
                _ratingLevelInfo.SetTextureBasedOnRating(_gameManager.CurrentLevel.RateScore(_gameManager.score));
            }
            else
            {
                //Set to incomplete rating
                _ratingLevelInfo.SetTextureBasedOnRating(0);
            }

            if (_gameManager.CurrentLevel == null)
            {
                _rearrangeEndScreenButtons();

                _ratingLevelInfo.SetTextureBasedOnRating(Level.RateEfficiencyScore(_gameManager.score, _gameManager.PotentialMaxSurvivalScore));
            }
            else
            {
                if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.Campaign)
                {
                    //Figure out if we can even go to the next level
                    int levelNumber = Convert.ToInt16(_gameManager.CurrentLevel.LevelData.Name.Substring(6, 2));

                    //if their is no next level OR the next level is locked
                    if (levelNumber == _gameManager.NumberOfLevels || _gameManager.DataManager.IsCampaignLevelLocked(levelNumber, _gameManager.GameDifficultyManager.GameDifficulty))
                    {
                        _rearrangeEndScreenButtons();
                    }
                }
                else
                {
                    _rearrangeEndScreenButtons();
                }

                if (_gameManager.ReasonForGameEnd == ReasonForGameEndEnum.Win)
                {
                    _title.text = "VICTORY";
                    _ratingLevelInfo.SetTextureBasedOnRating(_gameManager.CurrentLevel.RateScore(_gameManager.score));
                }
                else
                {
                    //Set to incomplete rating
                    _ratingLevelInfo.SetTextureBasedOnRating(0);
                }
            }

            _finalScoreText.GetComponent<Text>().text = _gameManager.score.ToString();
            _reasonForGameEndText.text = _gameManager.MapReasonsForLossToString(_gameManager.ReasonForGameEnd);
        }

        private void _rearrangeEndScreenButtons()
        {
            EventSystem.current.SetSelectedGameObject(_playAgainButton.gameObject);

            //Hide Next Level Button
            _nextLevelButton.gameObject.SetActive(false);

            _playAgainButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = _backButton,
                selectOnDown = _backButton
            };

            _backButton.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = _playAgainButton,
                selectOnDown = _playAgainButton
            };
        }


        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _handleBackButton();
                _gameManager.SoundEffectManager.PlayBack();

                return;
            }
            
            if (_nextLevelInputAction.triggered)
            {
                _handleNextLevelButton();
            }
            else if (_playAgainInputAction.triggered)
            {
                _handlePlayAgainButton();
            }
        }

        private void _handleNextLevelButton()
        {
            int levelNumber = Convert.ToInt16(_gameManager.CurrentLevel.LevelData.Name.Substring(6, 2));

            if (_gameManager.IsTrialMode && levelNumber + 1 > 10)
            {
                string message = GameManager.NotAvailableInTrailModeMessage;

                _gameManager.NotificationManager.MiddleOfSceneActivation(message);
                _gameManager.SoundEffectManager.PlayBack();
            }
            else
            {
                bool isLevelLocked = _gameManager.DataManager.IsCampaignLevelLocked(levelNumber, _gameManager.GameDifficultyManager.GameDifficulty);

                //if the next level is locked, don't allow it to happen
                if (isLevelLocked)
                {
                    _gameManager.SoundEffectManager.PlayBack();
                    return;
                }

                //load the next level
                _gameManager.CurrentLevel = new Level();
                _gameManager.CurrentLevel.LoadFromTextFile($"CampaignLevelData/Level {(levelNumber + 1):00}");

                if (levelNumber % _gameManager.LevelsPerPage == 0)
                {
                    _gameManager.LevelsDisplayingStart = levelNumber;
                    _gameManager.LevelsDisplayingEnd = _gameManager.LevelsDisplayingStart + _gameManager.LevelsPerPage;
                }
                _gameManager.LoadScene("Game");
                _gameManager.SoundEffectManager.PlaySelect();
            }
        }

        private void _handlePlayAgainButton()
        {
            _gameManager.RestartGame();
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleBackButton()
        {
            _gameManager.SoundEffectManager.PlayBack();

            if (!_gameManager.GameSetupInfo.IsSinglePlayer)
                _gameManager.LoadScene(SceneNames.MultiplayerGameModeSelection);
            else if (_gameManager.CurrentLevel == null)
                _gameManager.LoadScene(SceneNames.GameModeSelection);
            else if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.LevelEditor)
                _gameManager.LoadScene(SceneNames.LevelEditor);
            else
                _gameManager.LoadScene(SceneNames.Levels);
        }
    }
}
