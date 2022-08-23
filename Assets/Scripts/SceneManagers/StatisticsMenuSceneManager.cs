using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class StatisticsMenuSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        private Button _showLeaderboardsButton;

        [SerializeField]
        private Button _localStatisticsButton;


        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _localStatisticsButton.onClick.AddListener(_handleLocalStatisticsButton);
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene("Options");
                return;
            }
        }


        void _handleLocalStatisticsButton()
        {
            _gameManager.LoadScene("Local Statistics");
            _gameManager.SoundEffectManager.PlaySelect();
        }
    }
}
