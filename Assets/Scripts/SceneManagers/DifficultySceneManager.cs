using Assets.Scripts.RuleSets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class DifficultySceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        Button _hardButton;

        [SerializeField]
        Button _easyButton;

        [SerializeField]
        Button _veryEasyButton;

        [SerializeField]
        Text _descriptionText;

        private const string _HARD_DESCRIPTION= "The way the game was meant to be played.";
        private const string _EASY_DESCRIPTION = "For casual gamers struggling with hard mode.  Everything is slower here.";
        private const string _VERY_EASY_DESCRIPTION = "Easy mode but the laser is disabled.";

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_hardButton.gameObject);

            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _hardButton.onClick.AddListener(() => _handleDifficultySelect(GameDifficultyEnum.Hard));
            _easyButton.onClick.AddListener(() => _handleDifficultySelect(GameDifficultyEnum.Easy));
            _veryEasyButton.onClick.AddListener(() => _handleDifficultySelect(GameDifficultyEnum.VeryEasy));
        }

        private void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();

                if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.LevelEditor)
                {
                    _gameManager.LoadScene(SceneNames.LevelEditor);
                }
                else
                {
                    _gameManager.LoadScene(SceneNames.GameModeSelection);
                }
            }

            if (EventSystem.current.currentSelectedGameObject == _hardButton.gameObject)
            {
                if (_descriptionText.text != _HARD_DESCRIPTION)
                {
                    _descriptionText.text = _HARD_DESCRIPTION;
                }
            }
            else if(EventSystem.current.currentSelectedGameObject == _easyButton.gameObject)
            {
                if (_descriptionText.text != _EASY_DESCRIPTION)
                {
                    _descriptionText.text = _EASY_DESCRIPTION;
                }
            }
            else if (EventSystem.current.currentSelectedGameObject == _veryEasyButton.gameObject)
            {
                if (_descriptionText.text != _VERY_EASY_DESCRIPTION)
                {
                    _descriptionText.text = _VERY_EASY_DESCRIPTION;
                }
            }
            else
            {
                _descriptionText.text = null;
            }
                
        }

        private void _handleDifficultySelect(GameDifficultyEnum gameDifficulty)
        {
            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.GameDifficultyManager.ChangeDifficulty(gameDifficulty);


            if (_gameManager.GameSetupInfo.GameMode == GameModeEnum.Level)
            {
                _gameManager.GameSetupInfo.RuleSet = new RuleSet()
                {
                    GameSpeed = gameDifficulty,
                    AreLasersOn = gameDifficulty != GameDifficultyEnum.VeryEasy
                };
            }

            if(_gameManager.TheLevelSelectionMode == LevelTypeEnum.LevelEditor)
            {
                _gameManager.LoadScene(SceneNames.Game);
            }
            else
            {
                //go to level select
                _gameManager.ResetLevelDisplayNumbers();
                _gameManager.LoadScene(SceneNames.Levels);
            }
        }
    }
}
