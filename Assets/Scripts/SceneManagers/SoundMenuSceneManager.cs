using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class SoundMenuSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Button _toggleMusicButton;

        [SerializeField]
        private Button _toggleFxButton;

        private string _musicOnText = "M U S I C  ( ON )";
        private string _musicOffText = "M U S I C  ( OFF )";

        private string _effectsOnText = "E F F E C T S  ( ON )";
        private string _effectsOffText = "E F F E C T S  ( OFF )";

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_toggleMusicButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _toggleMusicButton.onClick.AddListener(_handleToggleMusicButton);
            _toggleFxButton.onClick.AddListener(_handleToggleFxButton);
            
            if (_gameManager.MusicOn)
                _toggleMusicButton.gameObject.GetComponentInChildren<Text>().text = _musicOnText;
            else
                _toggleMusicButton.gameObject.GetComponentInChildren<Text>().text = _musicOffText;

            if (_gameManager.SfxOn)
                _toggleFxButton.gameObject.GetComponentInChildren<Text>().text = _effectsOnText;
            else
                _toggleFxButton.gameObject.GetComponentInChildren<Text>().text = _effectsOffText;
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                //just stop the music if the user selected music off
                if (_gameManager.AudioManager.IsMuted())
                    _gameManager.AudioManager.Stop();

                PlayerPrefs.Save();

                _gameManager.LoadScene("Options");
                return;
            }
        }

        private void _handleToggleMusicButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.MusicOn = !_gameManager.MusicOn;
            _gameManager.DataManager.SetMusicPreference(_gameManager.MusicOn);
            
            if (_gameManager.MusicOn)
            {
                _gameManager.AudioManager.Play();
                _toggleMusicButton.gameObject.GetComponentInChildren<Text>().text = _musicOnText;
            }
            else
            {
                _gameManager.AudioManager.Mute();
                _toggleMusicButton.gameObject.GetComponentInChildren<Text>().text = _musicOffText;
            }
        }

        private void _handleToggleFxButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.SfxOn = !_gameManager.SfxOn;

            _gameManager.DataManager.SetSfxPreference(_gameManager.SfxOn);

            if (_gameManager.SfxOn)
                _toggleFxButton.gameObject.GetComponentInChildren<Text>().text = _effectsOnText;
            else
                _toggleFxButton.gameObject.GetComponentInChildren<Text>().text = _effectsOffText;
        }
    }
}
