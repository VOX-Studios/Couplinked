using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class SocialsSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        Button _twitchButton;

        [SerializeField]
        Button _discordButton;

        [SerializeField]
        Button _twitterButton;

        [SerializeField]
        Button _instagramButton;

        [SerializeField]
        Button _tikTokButton;

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_twitchButton.gameObject);

            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _twitchButton.onClick.AddListener(_handleTwitchButton);
            _twitterButton.onClick.AddListener(_handleTwitterButton);
            _discordButton.onClick.AddListener(_handleDiscordButton);
            _instagramButton.onClick.AddListener(_handleInstagramButton);
            _tikTokButton.onClick.AddListener(_handleTikTokButton);
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

        private void _handleTwitchButton()
        {
            Application.OpenURL("https://www.twitch.tv/voxindie");
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleTwitterButton()
        {
            Application.OpenURL("https://twitter.com/VOX_Studios");
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleDiscordButton()
        {
            Application.OpenURL("https://discord.com/invite/QUXYtVr");
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleInstagramButton()
        {
            Application.OpenURL("https://www.instagram.com/ttv.voxindie");
            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleTikTokButton()
        {
            Application.OpenURL("https://www.tiktok.com/@voxindie");
            _gameManager.SoundEffectManager.PlaySelect();
        }
    }
}
