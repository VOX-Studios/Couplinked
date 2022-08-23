using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class LocalStatisticsSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Text _hit1HitCountText;

        [SerializeField]
        private Text _hit2HitCountText;

        [SerializeField]
        private Text _hitSplit1HitCountText;

        [SerializeField]
        private Text _hitSplit2HitCountText;

        [SerializeField]
        private Text _noHitHitCountText;

        [SerializeField]
        private Text _timePlayedText;

        [SerializeField]
        private Text _gamesPlayedText;

        [SerializeField]
        private Text _highScoreText; //TODO: remove this shit...or make it survival only?


        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _hit1HitCountText.text = _gameManager.Hit1HitCount.ToString();
            _hit2HitCountText.text = _gameManager.Hit2HitCount.ToString();
            _hitSplit1HitCountText.text = _gameManager.HitSplit1HitCount.ToString();
            _hitSplit2HitCountText.text = _gameManager.HitSplit2HitCount.ToString();
            _noHitHitCountText.text = _gameManager.NoHitHitCount.ToString();
            _timePlayedText.text = _gameManager.TimePlayedHours.ToString("00") + ":" + _gameManager.TimePlayedMinutes.ToString("00") + ":" + _gameManager.TimePlayedSeconds.ToString("00") + "." + _gameManager.TimePlayedMilliseconds.ToString("000");
            _gamesPlayedText.text = _gameManager.GamesPlayed.ToString();
            _highScoreText.text = _gameManager.LocalHighScore.ToString();
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
    }
}
