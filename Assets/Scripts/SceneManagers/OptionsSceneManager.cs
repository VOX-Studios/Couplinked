using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class OptionsSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        private Button _statisticsButton;

        [SerializeField]
        private Button _graphicsButton;

        [SerializeField]
        private Button _soundButton;

        [SerializeField]
        private Button _achievementsButton;

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_graphicsButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            GameObject MainMenuBackButton = (GameObject)GameObject.Instantiate(_gameManager.MenuBackButtonPrefab);
            MainMenuBackButton.name = "MainMenuBackButton";

            _statisticsButton.onClick.AddListener(_handleStatisticsButton);
            _graphicsButton.onClick.AddListener(_handleGraphicsButton);
            _soundButton.onClick.AddListener(_handleSoundButton);
            _achievementsButton.onClick.AddListener(_handleAchievementsButton);
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene(SceneNames.Start);
                return;
            }
        }

        private void _handleGraphicsButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.LoadScene("Graphics");
        }

        private void _handleStatisticsButton()
        {
            if (_gameManager.IsTrialMode)
            {
                string message = GameManager.NotAvailableInTrailModeMessage;
                _gameManager.NotificationManager.MiddleOfSceneActivation(message);
                _gameManager.SoundEffectManager.PlayBack();
            }
            else
            {
                _gameManager.LoadScene("Local Statistics");
                _gameManager.SoundEffectManager.PlaySelect();
            }
        }

        private void _handleSoundButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.LoadScene("Sound");
        }

        private void _handleAchievementsButton()
        {
            if (_gameManager.IsTrialMode)
            {
                string message = GameManager.NotAvailableInTrailModeMessage;

                _gameManager.NotificationManager.MiddleOfSceneActivation(message);
                _gameManager.SoundEffectManager.PlayBack();
            }
            else
            {
                _gameManager.SoundEffectManager.PlaySelect();
                _gameManager.LoadScene("Achievements");
            }
        }
    }
}
