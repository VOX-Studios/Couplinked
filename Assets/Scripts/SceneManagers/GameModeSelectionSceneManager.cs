using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class GameModeSelectionSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Button _campaignButton;

        [SerializeField]
        private Button _survivalButton;
        
        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_campaignButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _survivalButton.onClick.AddListener(_handleSurvivalButton);
            _campaignButton.onClick.AddListener(_handleCampaignButton);
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene(SceneNames.PlayerModeSelection);
            }
        }

        private void _handleCampaignButton()
        {
            _gameManager.GameSetupInfo.GameMode = GameModeEnum.Level;
            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.TheLevelSelectionMode = LevelTypeEnum.Campaign;

            _gameManager.LoadScene("Difficulty Selection");
            
            //_gameManager.ResetLevelDisplayNumbers();
            //_gameManager.LoadScene("Levels");
        }

        private void _handleSurvivalButton()
        {
            _gameManager.GameSetupInfo.GameMode = GameModeEnum.Survival;
            _gameManager.GameDifficultyManager.ChangeDifficulty(GameDifficultyEnum.Hard);

            _gameManager.CurrentLevel = null;

            _gameManager.LoadScene(SceneNames.Game);

            _gameManager.SoundEffectManager.PlaySelect();
        }
    }
}
