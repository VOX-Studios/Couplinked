using Assets.Scripts.Gameplay;
using Assets.Scripts.RuleSets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class RuleSetSelectionSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private GameObject _buttonsContainer;

        [SerializeField]
        private Button _playButton;

        [SerializeField]
        private Dropdown _gameSpeedDropdown;

        [SerializeField]
        private Dropdown _rowsDropdown;

        [SerializeField]
        private Dropdown _livesDropdown;

        [SerializeField]
        private Dropdown _lasersDropdown;


        private Dictionary<string, GameDifficultyEnum> _gameSpeedMapping = new Dictionary<string, GameDifficultyEnum>()
        {
            { "FAST", GameDifficultyEnum.Hard },
            { "SLOW", GameDifficultyEnum.Easy }
        };

        private Dictionary<string, int> _rowsMapping = new Dictionary<string, int>()
        {
            { "3", 3 },
            { "4", 4 },
            { "5", 5 },
            { "6", 6 },
            { "7", 7 }
        };

        private Dictionary<string, int> _livesMapping = new Dictionary<string, int>()
        {
            { "1", 1 },
            { "2", 2 },
            { "3", 3 },
            { "4", 4 },
            { "5", 5 }
        };

        private Dictionary<string, bool> _boolMapping = new Dictionary<string, bool>()
        {
            { "ON", true },
            { "OFF", false }
        };

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_playButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            //if the rule set is null (this should only happen the first time we run singleplayer survival)
            if (_gameManager.GameSetupInfo.RuleSet == null)
            {
                _gameManager.GameSetupInfo.RuleSet = new RuleSet()
                {
                    GameSpeed = GameDifficultyEnum.Hard,
                    NumberOfRows = 3,
                    NumberOfLives = 1,
                    AreLasersOn = true
                };
            }

            _setupNavigation();

            _playButton.onClick.AddListener(_handlePlayButton);
            _setupDropdown(_gameSpeedDropdown, _gameManager.GameSetupInfo.RuleSet.GameSpeed, _gameSpeedMapping, gameSpeed => _gameManager.GameSetupInfo.RuleSet.GameSpeed = gameSpeed);
            _setupDropdown(_rowsDropdown, _gameManager.GameSetupInfo.RuleSet.NumberOfRows, _rowsMapping, numberOfRows => _gameManager.GameSetupInfo.RuleSet.NumberOfRows = numberOfRows);
            _setupDropdown(_livesDropdown, _gameManager.GameSetupInfo.RuleSet.NumberOfLives, _livesMapping, lives => _gameManager.GameSetupInfo.RuleSet.NumberOfLives = lives);
            _setupDropdown(_lasersDropdown, _gameManager.GameSetupInfo.RuleSet.AreLasersOn, _boolMapping, areLasersOn => _gameManager.GameSetupInfo.RuleSet.AreLasersOn = areLasersOn);
        }

        private void _setupNavigation()
        {
            Selectable[] navigationComponents = _buttonsContainer.GetComponentsInChildren<Selectable>();

            for (int i = 0; i < navigationComponents.Length; i++)
            {
                int previous = ((i - 1) + navigationComponents.Length) % navigationComponents.Length;
                int next = ((i + 1) + navigationComponents.Length) % navigationComponents.Length;

                navigationComponents[i].navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = navigationComponents[previous],
                    selectOnDown = navigationComponents[next]
                };
            }
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                if ( //dropdown with a child count of 3 is closed
                    _gameSpeedDropdown.transform.childCount == 3
                    && _rowsDropdown.transform.childCount == 3
                    && _livesDropdown.transform.childCount == 3
                    && _lasersDropdown.transform.childCount == 3
                    )
                {

                    _gameManager.SoundEffectManager.PlayBack();

                    if (_gameManager.GameSetupInfo.Teams.Count > 1 || _gameManager.GameSetupInfo.Teams[0].PlayerInputs.Count > 2)
                    {
                        _gameManager.LoadScene(SceneNames.MultiplayerGameModeSelection);
                    }
                    else
                    {
                        _gameManager.LoadScene(SceneNames.GameModeSelection);
                    }

                    return;
                }
            }
        }

        private void _handlePlayButton()
        {
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.LoadScene(SceneNames.Game);
        }

        private void _setupDropdown<T>(Dropdown dropdown, T defaultValue, Dictionary<string, T> mapping, Action<T> handleChange)
        {
            //clear drop down
            dropdown.ClearOptions();

            int currentRefreshRate = Screen.currentResolution.refreshRate;

            List<Dropdown.OptionData> options = mapping.Select(kvp => new Dropdown.OptionData()
            {
                text = kvp.Key
            }).ToList();

            //add options
            dropdown.AddOptions(options);

            //get the text the default value would have
            string defaultText = mapping.First(kvp => kvp.Value.Equals(defaultValue)).Key;

            //set default value
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == defaultText)
                {
                    dropdown.value = i;
                    break;
                }
            }

            dropdown.onValueChanged.AddListener((index) =>
            {
                _handleDropdownSelection(dropdown, mapping, index, handleChange);
            });
        }

        private void _handleDropdownSelection<T>(Dropdown dropdown, Dictionary<string, T> mapping, int index, Action<T> handleChange)
        {
            string text = dropdown.options[index].text;
            T value = mapping[text];

            handleChange(value);
        }
    }
}
