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
                    _gameManager.LoadScene("Level Editor");
                }
                else
                {
                    _gameManager.LoadScene("GameModeSelection");
                }
            }
        }

        private void _handleDifficultySelect(GameDifficultyEnum gameDifficulty)
        {
            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.GameDifficultyManager.ChangeDifficulty(gameDifficulty);

            if(_gameManager.TheLevelSelectionMode == LevelTypeEnum.LevelEditor)
            {
                _gameManager.LoadScene("Game");
            }
            else
            {
                //go to level select
                _gameManager.ResetLevelDisplayNumbers();
                _gameManager.LoadScene("Levels");
            }
        }
    }
}
