using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class MultiplayerGameModeSelectionSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Button _versusButton;

        [SerializeField]
        private Button _coopButton;
        
        void Start()
        {
            //EventSystem.current.SetSelectedGameObject(_versusButton.gameObject);
            EventSystem.current.SetSelectedGameObject(_coopButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _versusButton.onClick.AddListener(_handleVersusButton);
            _coopButton.onClick.AddListener(_handleCoopButton);
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene(SceneNames.MultiplayerControllerSelection);
            }
        }

        private void _handleVersusButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _tempStartSurvival();
        }

        private void _handleCoopButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.GameSetupInfo.GameMode = GameModeEnum.SurvivalCoOp;
            _tempStartSurvival();
        }

        private void _tempStartSurvival()
        {
            _gameManager.GameDifficultyManager.ChangeDifficulty(GameDifficultyEnum.Hard);
            _gameManager.CurrentLevel = null;
            _gameManager.LoadScene(SceneNames.Game);
        }
    }
}
