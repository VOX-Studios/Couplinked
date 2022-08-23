using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class LevelEditorSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        private Button _playButton;

        [SerializeField]
        private Button _saveButton;

        [SerializeField]
        private Button _snapButton;

        [SerializeField]
        private Button _exportButton;

        [SerializeField]
        private LevelEditorManager _levelEditorManager;

        [SerializeField]
        private Text _levelNameText;

        [SerializeField]
        private GameObject _savePrompt;

        [SerializeField]
        private InputField _levelNameInputField;

        [SerializeField]
        private Button _confirmNameButton;

        [SerializeField]
        private Button _cancelNameButton;

        [SerializeField]
        private GameObject _placeholderPanel;

        public bool ShouldSnap = true;

        private InputAction _backInputAction;

        private bool _isNamePromptComplete;
        private bool _isPromptingForSaveName;
        private string _saveName = "";

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            InputActionMap levelEditorInputActionMap = _gameManager.InputActions.FindActionMap("Level Editor");

            _backInputAction = levelEditorInputActionMap.FindAction("Back");

            _playButton.onClick.AddListener(_handlePlayButton);
            _saveButton.onClick.AddListener(_handleSaveButton);
            _confirmNameButton.onClick.AddListener(_handleConfirmNameButton);
            _cancelNameButton.onClick.AddListener(_handleCancelNameButton);
            _snapButton.onClick.AddListener(_handleSnapButton);
            _exportButton.onClick.AddListener(_handleExportButton);

            List<Transform> placeholderPositions = new List<Transform>();
            int children = _placeholderPanel.transform.childCount;
            for (int i = 0; i < children; ++i)
            {
                placeholderPositions.Add(_placeholderPanel.transform.GetChild(i));
            }

            _levelEditorManager.Initialize(placeholderPositions);

            if (_gameManager.IsNewLevel)
            {
                _saveName = "";
                _levelNameText.text = "New Level";
            }
            else
            {
                _saveName = _gameManager.CurrentLevel.LevelData.Name;
                _levelNameText.text = _saveName;
            }

            //TODO: make this configurable...it was only being used for team selection...this doesn't allow multiplayer maps
            _gameManager.GameSetupInfo.Teams = new List<Team>
            {
                new Team(0)
                {
                    PlayerInputs = new List<PlayerInput>()
                    {
                        new PlayerInput(
                            inputActionAsset: _gameManager.InputActions,
                            inputUser: InputUser.CreateUserWithoutPairedDevices(), 
                            playerSlot: 0
                            )
                    }
                }
            };
        }

        void Update()
        {
            if (_backInputAction.triggered)
            {
                _gameManager.SoundEffectManager.PlayBack();

                if (_isPromptingForSaveName) //Should only happen on WP8!...idk about that anymore...should probably look into this
                {
                    _saveName = "";
                    _isPromptingForSaveName = false;
                    _savePromptSetActive(false);
                }
                else
                {
                    bool shouldExit = _levelEditorManager.OnExitButtonPressed(_backInputAction);
                    if (shouldExit)
                    {
                        _gameManager.LoadScene("LevelEditorSelection");
                        return;
                    }
                }
            }

            if (_isPromptingForSaveName)
                return;

            _levelEditorManager.Run();
        }

        public bool _validateLevelEditorSaveAttempt()
        {
            //check if there are any objects in the level, if there are none we're not going to save
            if (_gameManager.CurrentLevel.Data.Count <= 0)
            {
                _gameManager.NotificationManager.MiddleOfSceneActivation("Level must contain at least\n 1 object.");
            }
            else if (_saveName.Length > 9)
            {
                _gameManager.NotificationManager.MiddleOfSceneActivation("Level name must be shorter than\n 10 characters.");
                _saveName = "";
            }
            else if (_saveName.Length == 0)
            {
                _gameManager.NotificationManager.MiddleOfSceneActivation("Level name must contain at least\n 1 character.");
            }
            else if (_saveName != System.Text.RegularExpressions.Regex.Replace(_saveName, @"(\r\n|\r|\n)", ""))
            {
                _gameManager.NotificationManager.MiddleOfSceneActivation("Level name must not contain\n multiple lines.");
            }
            else
            {
                _gameManager.SoundEffectManager.PlaySelect();
                _isPromptingForSaveName = false;
                return true;
            }

            _gameManager.SoundEffectManager.PlayBack();
            return false;
        }

        private void _deactivateActivePlaceableObjects()
        {
            //Deactivate any leftover placeable objects so they don't accidentally get placed
            PlaceableObject[] POs = FindObjectsOfType<PlaceableObject>() as PlaceableObject[];
            for (int i = POs.Length - 1; i >= 0; i--)
            {
                POs[i].transform.parent.gameObject.GetComponent<PlaceableObjectSpawner>().PlaceableObjectsPool.DeactivateObject(POs[i].gameObject);
            }
        }

        private void _handlePlayButton()
        {
            if (_isPromptingForSaveName)
                return;

            //Deactivate any leftover placeable objects so they don't accidentally get placed
            _deactivateActivePlaceableObjects();

            if (_gameManager.CurrentLevel.Data.Count > 0)
            {
                _gameManager.CurrentLevel.CalculateMaxScore();
                _gameManager.LoadScene(SceneNames.DifficultySelection);
                return;
            }

            //TODO: Show message saying we can't play because no objects
            _gameManager.SoundEffectManager.PlayBack();
        }

        private void _handleSnapButton()
        {
            if (_isPromptingForSaveName)
                return;

            ShouldSnap = !ShouldSnap;

            if (ShouldSnap)
            {
                _snapButton.GetComponent<Image>().color = Color.white;
                _gameManager.SoundEffectManager.PlaySelect();
            }
            else
            {
                _snapButton.GetComponent<Image>().color = new Color(1, 1, 1, .125f);
                _gameManager.SoundEffectManager.PlayBack();
            }
        }

        private void _handleSaveButton()
        {
            if(_gameManager.IsNewLevel && !_isPromptingForSaveName && !_isNamePromptComplete)
            {
                _isPromptingForSaveName = true;
                _gameManager.SoundEffectManager.PlaySelect();

                _levelNameInputField.text = _saveName;

                _savePromptSetActive(true);

                EventSystem.current.SetSelectedGameObject(_levelNameInputField.gameObject);
                return;
            }

            if (_isPromptingForSaveName)
                return;

            if (!_validateLevelEditorSaveAttempt())
            {
                _isNamePromptComplete = false;
                return;
            }

            //Deactivate any leftover placeable objects so they don't accidentally get placed
            _deactivateActivePlaceableObjects();

            _gameManager.SoundEffectManager.PlaySelect();

            //if it's a new level
            if (_gameManager.IsNewLevel)
            {
                _gameManager.CurrentLevel.LevelData.Name = _saveName;
                _gameManager.CurrentLevel.LevelData.Id = Guid.NewGuid().ToString();

                _gameManager.CurrentLevel.Save(_gameManager.DataManager);
                _gameManager.IsNewLevel = false;

                _levelNameText.text = _gameManager.CurrentLevel.LevelData.Name;
            }
            else
            {
                _gameManager.CurrentLevel.Save(_gameManager.DataManager);
            }
        }

        private void _savePromptSetActive(bool setActive)
        {
            _savePrompt.SetActive(setActive);
        }

        private void _handleConfirmNameButton()
        {
            if (!_isPromptingForSaveName)
                return;

            //TODO: support keys and shit for saving
            /*if (e.type == EventType.KeyDown
                    && (e.keyCode == KeyCode.Return
                    || e.keyCode == KeyCode.KeypadEnter))*/

            _savePromptSetActive(false);

            _isNamePromptComplete = true;
            _isPromptingForSaveName = false;

            _saveName = _levelNameInputField.text;
            _levelNameText.text = _saveName;
            _handleSaveButton();
        }

        private void _handleCancelNameButton()
        {
            _isPromptingForSaveName = false;

            _savePromptSetActive(false);

            _gameManager.SoundEffectManager.PlaySelect();
        }

        private void _handleExportButton()
        {
            //Deactivate any leftover placeable objects so they don't accidentally get placed
            _deactivateActivePlaceableObjects();

            if (_gameManager.CurrentLevel.Data.Count <= 0)
            {
                _gameManager.NotificationManager.MiddleOfSceneActivation("Level must contain at least\n 1 object.");
                _gameManager.SoundEffectManager.PlayBack();

                return;
            }

            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.CurrentLevel.ExportCampaignLevel();
        }
    }
}
