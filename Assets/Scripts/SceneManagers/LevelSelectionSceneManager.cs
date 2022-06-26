using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class LevelSelectionSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Text _titleText;

        [SerializeField]
        private Button _modeLeftButton;

        [SerializeField]
        private Button _modeRightButton;

        [SerializeField]
        private Button _levelsLeftButton;

        [SerializeField]
        private Button _levelsRightButton;

        [SerializeField]
        private Button _garbageBinButton;

        [SerializeField]
        private TextAsset[] _campaignLevelData;

        private int _lastLevelIndex = 0;

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _garbageBinButton.onClick.AddListener(_handleGarbageBinButton);
            _modeLeftButton.onClick.AddListener(_toggleBetweenCampaignAndCustomLevels);
            _modeRightButton.onClick.AddListener(_toggleBetweenCampaignAndCustomLevels);

            _levelsLeftButton.onClick.AddListener(_handleLevelsLeftButton);
            _levelsRightButton.onClick.AddListener(_handleLevelsRightButton);

            switch (_gameManager.TheLevelSelectionMode)
            {
                case LevelTypeEnum.LevelEditor:
                    _titleText.text = "LEVEL EDITOR";
                    _modeLeftButton.gameObject.SetActive(false);
                    _modeRightButton.gameObject.SetActive(false);
                    break;
                case LevelTypeEnum.Campaign:
                    //unlock the first level
                    _gameManager.DataManager.UnlockCampaignLevel(0, _gameManager.GameDifficultyManager.GameDifficulty);
                    _garbageBinButton.gameObject.SetActive(false);
                    _titleText.text = "CAMPAIGN";
                    break;
                case LevelTypeEnum.Custom:
                    _titleText.text = "CUSTOM";
                    break;
            }

            _loadAndDisplayLevels();

            GameObject levels = GameObject.Find("Levels");
            if(levels.transform.childCount > 0)
                EventSystem.current.SetSelectedGameObject(levels.transform.GetChild(0).gameObject);
            else
                EventSystem.current.SetSelectedGameObject(_levelsLeftButton.gameObject);
        }

        void Update()
        {
            /*
#if UNITY_EDITOR
		isNewLevel = true;
		currentLevel = new Level();
		currentLevel.LoadFromTextFile("CampaignLevelData/" + "Level 51");
		LoadScene("Level Editor");
		return;
#endif
*/
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();

                switch (_gameManager.TheLevelSelectionMode)
                {
                    case LevelTypeEnum.LevelEditor:
                        _gameManager.LoadScene(SceneNames.LevelEditorSelection);
                        break;
                    case LevelTypeEnum.Campaign:
                    case LevelTypeEnum.Custom:
                        _gameManager.LoadScene(SceneNames.DifficultySelection);
                        break;
                }
            }
        }

        private void _handleGarbageBinButton()
        {
            _garbageBinButton.gameObject.GetComponent<GarbageBin>().Toggle();
        }

        private void _loadAndDisplayLevels()
        {
            GameObject parent = GameObject.Find("Levels");

            //get rid of old levels
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.transform.GetChild(i).gameObject);
            }

            switch (_gameManager.TheLevelSelectionMode)
            {
                case LevelTypeEnum.LevelEditor:
                case LevelTypeEnum.Custom:
                    string[] levelIDsList = _gameManager.DataManager.GetCustomLevelIds();
                    _lastLevelIndex = levelIDsList.Length;

                    List<string> levelNamesList = new List<string>();

                    if (_gameManager.LevelsDisplayingEnd > levelIDsList.Length)
                    {
                        _gameManager.LevelsDisplayingEnd = levelIDsList.Length;
                        _gameManager.LevelsDisplayingStart = _gameManager.LevelsDisplayingEnd - _gameManager.LevelsPerPage;

                        if (_gameManager.LevelsDisplayingStart < 0)
                            _gameManager.LevelsDisplayingStart = 0;
                    }

                    for (int i = _gameManager.LevelsDisplayingStart; i < _gameManager.LevelsDisplayingEnd; i++)
                    {
                        levelNamesList.Add(_gameManager.DataManager.GetCustomLevelName(levelIDsList[i]));
                    }

                    for (int i = 0; i < levelNamesList.Count; i++)
                    {
                        GameObject levelInfo = (GameObject)Instantiate(_gameManager.LevelInfoPrefab, Vector3.zero, Quaternion.identity);

                        levelInfo.GetComponent<Button>().onClick.AddListener(() => _handleLevelSelected(levelInfo));

                        levelInfo.transform.parent = parent.transform;
                        levelInfo.transform.localScale = Vector3.one;

                        LevelInfo levelInfoComponent = levelInfo.GetComponent<LevelInfo>();
                        levelInfoComponent.Id = levelIDsList[i + _gameManager.LevelsDisplayingStart];
                        levelInfoComponent.Name = levelNamesList[i];
                        levelInfoComponent.levelType = LevelInfo.LevelTypes.Custom;

                        levelInfo.GetComponentInChildren<Text>().text = levelInfoComponent.Name;

                        //custom levels are never locked
                        levelInfoComponent.IsLocked = false;

                        //only get excess stats if we're playing a custom game
                        if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.Custom)
                        {
                            levelInfoComponent.MaxScore = _gameManager.DataManager.GetLevelMaxScore(levelInfoComponent.Id);
                            levelInfoComponent.PlayerScore = _gameManager.DataManager.GetLevelPlayerScore(levelInfoComponent.Id, _gameManager.GameDifficultyManager.GameDifficulty);
                            levelInfoComponent.PlayerScoreRating = _gameManager.DataManager.GetLevelPlayerScoreRating(levelInfoComponent.Id, _gameManager.GameDifficultyManager.GameDifficulty);
                            levelInfoComponent.SetTextureBasedOnRating(levelInfoComponent.PlayerScoreRating);
                        }
                    }

                    break;
                case LevelTypeEnum.Campaign:
                    _lastLevelIndex = _campaignLevelData.Length;

                    if (_gameManager.LevelsDisplayingEnd > _campaignLevelData.Length)
                    {
                        _gameManager.LevelsDisplayingEnd = _campaignLevelData.Length;
                        _gameManager.LevelsDisplayingStart = _gameManager.LevelsDisplayingEnd - 1;
                        if (_gameManager.LevelsDisplayingStart < 0)
                            _gameManager.LevelsDisplayingStart = 0;
                    }

                    for (int i = _gameManager.LevelsDisplayingStart; i < _gameManager.LevelsDisplayingEnd; i++)
                    {
                        GameObject levelInfo = (GameObject)Instantiate(_gameManager.LevelInfoPrefab, Vector3.zero, Quaternion.identity);

                        levelInfo.GetComponent<Button>().onClick.AddListener(() => _handleLevelSelected(levelInfo));

                        levelInfo.transform.parent = parent.transform;
                        levelInfo.transform.localScale = Vector3.one;

                        LevelInfo levelInfoComponent = levelInfo.GetComponent<LevelInfo>();
                        levelInfoComponent.Id = "";
                        levelInfoComponent.Name = _campaignLevelData[i].name;
                        levelInfoComponent.levelType = LevelInfo.LevelTypes.Campaign;

                        levelInfo.GetComponentInChildren<Text>().text = levelInfoComponent.Name;

                        bool isLevelLocked = _gameManager.DataManager.IsCampaignLevelLocked(i, _gameManager.GameDifficultyManager.GameDifficulty);

                        if (isLevelLocked)
                        {
                            //set level info to be locked
                            levelInfoComponent.IsLocked = true;

                            //switch out graphic
                            levelInfo.GetComponent<Image>().sprite = levelInfoComponent.LockedTexture;
                        }
                        else
                        {
                            //set level info to be unlocked
                            levelInfoComponent.IsLocked = false;

                            //campaign level IDs match their name
                            levelInfoComponent.MaxScore = _gameManager.DataManager.GetLevelMaxScore(_campaignLevelData[i].name);
                            levelInfoComponent.PlayerScore = _gameManager.DataManager.GetLevelPlayerScore(_campaignLevelData[i].name, _gameManager.GameDifficultyManager.GameDifficulty);
                            levelInfoComponent.PlayerScoreRating = _gameManager.DataManager.GetLevelPlayerScoreRating(_campaignLevelData[i].name, _gameManager.GameDifficultyManager.GameDifficulty);
                            levelInfoComponent.SetTextureBasedOnRating(levelInfoComponent.PlayerScoreRating);
                        }
                    }

                    break;
            }
        }

        private void _handleLevelsLeftButton()
        {
            if(_gameManager.LevelsDisplayingStart == 0)
            {
                _gameManager.SoundEffectManager.PlayBack();
                return;
            }

            _gameManager.SoundEffectManager.PlaySelect();

            if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.Campaign && _gameManager.LevelsDisplayingEnd == _gameManager.NumberOfLevels)
            {
                _gameManager.LevelsDisplayingStart -= _gameManager.LevelsPerPage;
                _gameManager.LevelsDisplayingEnd--;
            }
            else
            {
                _gameManager.LevelsDisplayingStart -= _gameManager.LevelsPerPage;
                _gameManager.LevelsDisplayingEnd -= _gameManager.LevelsPerPage;
            }


            if (_gameManager.LevelsDisplayingStart < 0)
            {
                _gameManager.LevelsDisplayingStart = 0;
                _gameManager.LevelsDisplayingEnd = _gameManager.LevelsPerPage;
                //> count overflow is handled in update gui function
            }

            _loadAndDisplayLevels();
        }


        private void _handleLevelsRightButton()
        {
            if (_gameManager.LevelsDisplayingEnd == _lastLevelIndex)
            {
                _gameManager.SoundEffectManager.PlayBack();
                return;
            }

            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.LevelsDisplayingStart += _gameManager.LevelsPerPage;
            _gameManager.LevelsDisplayingEnd += _gameManager.LevelsPerPage;
            //> count overflow is handled in update gui function

            _loadAndDisplayLevels();
        }

        private void _handleLevelSelected(GameObject level)
        {
            int highestLevelTierUnlocked = _gameManager.DataManager.GetHighestCampaignLevelTierUnlocked(_gameManager.GameDifficultyManager.GameDifficulty);

            LevelInfo levelInfo = level.GetComponent<LevelInfo>();

            //don't load locked level
            if (levelInfo.IsLocked)
            {
                int levelNumber = System.Convert.ToInt16(levelInfo.Name.Substring(6, 2));
                int tensPlace = System.Convert.ToInt16(levelInfo.Name.Substring(6, 1));

                //if they selected a level in a future tier
                if (levelNumber > 10 && levelNumber > (highestLevelTierUnlocked + 1) * 10)
                {
                    int tier = tensPlace;
                    if (levelNumber % 10 == 0)
                        tier--;

                    _gameManager.NotificationManager.MiddleOfSceneActivation($"You need a { _gameManager.GetWordFromNumber(tier) } star rating\non every previous level \nto unlock Tier { tier.ToString("00") }.");
                }
                else //same tier
                {
                    _gameManager.NotificationManager.MiddleOfSceneActivation("Complete Level "
                                                                + (levelNumber - 1).ToString("00") + " to\n"
                                                                + "unlock Level " + levelNumber.ToString("00")
                                                                + ".");
                }

                _gameManager.SoundEffectManager.PlayBack();
                return;
            }

            if (levelInfo.levelType == LevelInfo.LevelTypes.Custom)
            {
                _gameManager.CurrentLevel = new Level(levelInfo.Id);
            }
            else //campaign
            {
                _gameManager.CurrentLevel = new Level();
                _gameManager.CurrentLevel.LoadFromTextFile("CampaignLevelData/" + levelInfo.Name);
            }


            switch (_gameManager.TheLevelSelectionMode)
            {
                case LevelTypeEnum.LevelEditor:
                    if (_garbageBinButton.GetComponent<GarbageBin>().DeleteIsActive)
                    {
                        /*
                        NotificationManager.MiddleOfSceneActivation("Are you sure you want to delete\n" + levelInfo.name + "?"
                                                                    + "\nTap to delete, back to cancel.");
                        */
                        _deleteLevel();
                    }
                    else
                    {
                        _gameManager.IsNewLevel = false;
                        _gameManager.LoadScene(SceneNames.LevelEditor);
                    }
                    break;
                case LevelTypeEnum.Campaign:
                    if (_gameManager.IsTrialMode)
                    {
                        int levelNumber = System.Convert.ToInt16(_gameManager.CurrentLevel.LevelData.Name.Substring(6, 2));

                        if (levelNumber > 10)
                        {
                            string message = GameManager.NotAvailableInTrailModeMessage;
                            _gameManager.NotificationManager.MiddleOfSceneActivation(message);
                            _gameManager.SoundEffectManager.PlayBack();
                            return;
                        }
                    }
                    _gameManager.LoadScene(SceneNames.Game);
                    break;
                case LevelTypeEnum.Custom:
                    if (_garbageBinButton.GetComponent<GarbageBin>().DeleteIsActive)
                    {
                        /*
                        NotificationManager.MiddleOfSceneActivation("Are you sure you want to delete\n" + levelInfo.name + "?"
                                                                    + "\nTap to delete, back to cancel.");
                        */
                        _deleteLevel();
                    }
                    else
                    {
                        _gameManager.LoadScene(SceneNames.Game);
                    }
                    break;
            }

            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _deleteLevel()
        {
            _gameManager.DataManager.DeleteCustomLevel(_gameManager.CurrentLevel.LevelData.Id);
            _gameManager.CurrentLevel = null;
            _loadAndDisplayLevels();
        }

        private void _toggleBetweenCampaignAndCustomLevels()
        {
            if (_gameManager.TheLevelSelectionMode == LevelTypeEnum.Custom)
            {
                _gameManager.TheLevelSelectionMode = LevelTypeEnum.Campaign;
                _titleText.text = "CAMPAIGN";

                if (_garbageBinButton.GetComponent<GarbageBin>().DeleteIsActive)
                    _garbageBinButton.GetComponent<GarbageBin>().Toggle();

                _garbageBinButton.gameObject.SetActive(false);
            }
            else
            {
                _garbageBinButton.gameObject.SetActive(true);
                _gameManager.TheLevelSelectionMode = LevelTypeEnum.Custom;

                _titleText.text = "CUSTOM";
            }

            _gameManager.ResetLevelDisplayNumbers();

            _loadAndDisplayLevels();
        }
    }
}
