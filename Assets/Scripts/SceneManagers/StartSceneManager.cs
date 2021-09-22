using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class StartSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        Button _playButton;

        [SerializeField]
        Button _levelEditorButton;

        [SerializeField]
        Button _instructionsButton;

        [SerializeField]
        Button _optionsButton;

        [SerializeField]
        Button _socialsButton;

        [SerializeField]
        Button _exitButton;

        [SerializeField]
        Text _debugText;

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_playButton.gameObject);

            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _playButton.onClick.AddListener(_handlePlayButton);
            _instructionsButton.onClick.AddListener(_handleInstructionsButton);
            _levelEditorButton.onClick.AddListener(_handleLevelEditorButton);
            _optionsButton.onClick.AddListener(_handleOptionsButton);
            _socialsButton.onClick.AddListener(_handleSocialsButton);
            _exitButton.onClick.AddListener(_handleExitButton);

            _debugText.text = $"V{Application.version}";
        }

        private void _handlePlayButton()
        {
            _gameManager.LoadScene(SceneNames.PlayerModeSelection);
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleInstructionsButton()
        {
            _gameManager.LoadScene(SceneNames.Instructions);
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleLevelEditorButton()
        {
            if (_gameManager.IsTrialMode)
            {
                string message = GameManager.NotAvailableInTrailModeMessage;
                _gameManager.NotificationManager.MiddleOfSceneActivation(message);
                _gameManager.SoundEffectManager.PlayBack();
            }
            else
            {
                _gameManager.LoadScene(SceneNames.LevelEditorSelection);
                _gameManager.SoundEffectManager.PlaySelect();
            }
        }

        private void _handleOptionsButton()
        {
            _gameManager.LoadScene(SceneNames.Options);
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleSocialsButton()
        {
            _gameManager.LoadScene(SceneNames.Socials);
            _gameManager.SoundEffectManager.PlaySelect();
        }
        
        private void _handleExitButton()
        {
            Application.Quit();
        }
    }
}
