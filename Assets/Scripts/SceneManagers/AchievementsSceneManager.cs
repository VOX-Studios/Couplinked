using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class AchievementsSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        private Button _achievementsLeftButton;

        [SerializeField]
        private Button _achievementsRightButton;

        [SerializeField]
        private AchievementInfo _achievementInfo;

        private int _currentAchievementDisplayed;

        private int _numberOfChallenges = -1;

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _achievementsLeftButton.onClick.AddListener(_handleAchievementsLeftButton);
            _achievementsRightButton.onClick.AddListener(_handleAchievementsRightButton);
            _achievementInfo.Button.onClick.AddListener(_handleAchievementInfoButton);

            _currentAchievementDisplayed = 0;
            _displayCurrentAchievement();
        }

        private void _displayCurrentAchievement()
        {
            _gameManager.Challenges.LoadChallenges();
            if (_gameManager.Challenges.ChallengesArray.Length == 0)
            {
                _achievementInfo.gameObject.SetActive(false);
                return;
            }

            _numberOfChallenges = _gameManager.Challenges.ChallengesArray.Length;

            if (_currentAchievementDisplayed < 0)
                _currentAchievementDisplayed = 0;
            else if (_currentAchievementDisplayed > _gameManager.Challenges.ChallengesArray.Length - 1)
                _currentAchievementDisplayed = _gameManager.Challenges.ChallengesArray.Length - 1;

            Challenge c = _gameManager.Challenges.ChallengesArray[_currentAchievementDisplayed];
            _achievementInfo.TitleText.text = c.DisplayText;
            _achievementInfo.DescriptionText.text = c.Description;
            _achievementInfo.SetCompletedStatusAndTexture(c.LoadCompletedStatus());
            _achievementInfo.ID = c.Id;
            
            _achievementsLeftButton.gameObject.SetActive(true);
            _achievementsRightButton.gameObject.SetActive(true);

            if (_currentAchievementDisplayed == 0)
                _achievementsLeftButton.gameObject.SetActive(false);
            else if (_currentAchievementDisplayed == _gameManager.Challenges.ChallengesArray.Length - 1)
                _achievementsRightButton.gameObject.SetActive(false);

            _gameManager.Challenges.Clear();
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.Challenges.Clear();
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene("Options");
                return;
            }

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                _handleAchievementsLeftButton();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                _handleAchievementsRightButton();
            }
        }

        private void _handleAchievementInfoButton()
        {
            if (_achievementInfo.GetComponent<AchievementInfo>().IsCompleted)
            {
                string empty = "";
                _gameManager.Challenges.HandleUnlockingChallenge(_achievementInfo.GetComponent<AchievementInfo>().ID, out empty, true);
                _gameManager.SoundEffectManager.PlaySelect();
            }
            else
            {
                _gameManager.SoundEffectManager.PlayBack();
            }
        }

        private void _handleAchievementsLeftButton()
        {
            if (_currentAchievementDisplayed == 0)
            {
                _gameManager.SoundEffectManager.PlayBack();
                return;
            }

            _currentAchievementDisplayed--;

            _gameManager.SoundEffectManager.PlaySelect();
            _displayCurrentAchievement();
        }

        private void _handleAchievementsRightButton()
        {
            if (_currentAchievementDisplayed == _numberOfChallenges - 1)
            {
                _gameManager.SoundEffectManager.PlayBack();
                return;
            }

            _currentAchievementDisplayed++;

            _gameManager.SoundEffectManager.PlaySelect();
            _displayCurrentAchievement();
        }
    }
}
