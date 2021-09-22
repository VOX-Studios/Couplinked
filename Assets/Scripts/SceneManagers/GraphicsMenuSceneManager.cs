using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class GraphicsMenuSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Button _colorSelectionButton;

        [SerializeField]
        private Dropdown _gridDensityDropdown;

        [SerializeField]
        private Dropdown _explosionParticlesDropdown;

        [SerializeField]
        private Dropdown _trailParticlesDropdown;

        [SerializeField]
        private Dropdown _screenShakeDropdown;

        [SerializeField]
        private Dropdown _resolutionDropdown;

        [SerializeField]
        private Dropdown _windowedModeDropdown;

        [SerializeField]
        private Dropdown _vSyncDropdown;

        private QualitySettingEnum _gridDensity;
        private QualitySettingEnum _explosionParticles;
        private QualitySettingEnum _trailParticles;
        private QualitySettingEnum _screenShake;

        private string _resolution;
        private FullScreenMode _windowedMode;
        private VSyncCountEnum _vSyncCount;

        private List<KeyValuePair<FullScreenMode, string>> _supportedWindowedModes = new List<KeyValuePair<FullScreenMode, string>>()
        {
            new KeyValuePair<FullScreenMode, string>(FullScreenMode.ExclusiveFullScreen, "Fullscreen"),
            new KeyValuePair<FullScreenMode, string>(FullScreenMode.FullScreenWindow, "Fullscreen Borderless"),
            new KeyValuePair<FullScreenMode, string>(FullScreenMode.Windowed, "Windowed"),
        };


        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_colorSelectionButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            GameObject MainMenuBackButton = (GameObject)GameObject.Instantiate(_gameManager.MenuBackButtonPrefab);
            MainMenuBackButton.name = "MainMenuBackButton";

            _windowedMode = _gameManager.DataManager.WindowedModePreference.Get();
            _vSyncCount = _gameManager.DataManager.VSyncCountPreference.Get();

            _colorSelectionButton.onClick.AddListener(_handleColorSelectionButton);

            _setupQualitySettingDropdown(
                dropdown: _gridDensityDropdown,
                repo: _gameManager.DataManager.GridDensity,
                setQualitySettingValue: (qualitySettingSelected) => _gridDensity = qualitySettingSelected,
                afterOnValueChange: () => _gameManager.Grid.Create()
                ); ;

            _setupQualitySettingDropdown(
                dropdown: _explosionParticlesDropdown,
                repo: _gameManager.DataManager.ExplosionParticleQuality,
                setQualitySettingValue: (qualitySettingSelected) => _explosionParticles = qualitySettingSelected
                );

            _setupQualitySettingDropdown(
                dropdown: _trailParticlesDropdown,
                repo: _gameManager.DataManager.TrailParticleQuality,
                setQualitySettingValue: (qualitySettingSelected) => _trailParticles = qualitySettingSelected
                );

            _setupQualitySettingDropdown(
                dropdown: _screenShakeDropdown,
                repo: _gameManager.DataManager.ScreenShakeStrength,
                setQualitySettingValue: (qualitySettingSelected) => _screenShake = qualitySettingSelected
                );

            _setupResolutionDropdown();
            _setupWindowedModeDropdown();
            _setupVSyncDropdown();
        }

        private void _setupQualitySettingDropdown(Dropdown dropdown, EnumData<QualitySettingEnum> repo, Action<QualitySettingEnum> setQualitySettingValue, Action afterOnValueChange = null)
        {
            _setupQualitySettingDropdown(
                dropdown: dropdown,
                repo: repo,
                listener: (index) => {
                    _handleQualitySettingDropdown(
                        dropdown: dropdown,
                        index: index,
                        repo: repo,
                        setQualitySettingValue: setQualitySettingValue
                        );

                    if(afterOnValueChange != null)
                        afterOnValueChange();
                },
                setQualitySettingValue: setQualitySettingValue
                );
        }

        private void _setupQualitySettingDropdown(Dropdown dropdown, EnumData<QualitySettingEnum> repo, UnityAction<int> listener, Action<QualitySettingEnum> setQualitySettingValue)
        {
            //clear drop down
            dropdown.ClearOptions();

            //format options
            List<Dropdown.OptionData> options = Enum.GetValues(typeof(QualitySettingEnum)).Cast<QualitySettingEnum>().Select(q => new Dropdown.OptionData()
            {
                text = q.ToString()
            }).ToList();

            //add options
            dropdown.AddOptions(options);

            //get preferred resolution
            QualitySettingEnum qualitySetting = repo.Get();

            setQualitySettingValue(qualitySetting);

            string valueString = qualitySetting.ToString();

            //set dropdown to selected value
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == valueString)
                    dropdown.value = i;
            }

            dropdown.onValueChanged.AddListener(listener);
        }

        private void _setupResolutionDropdown()
        {
            //clear drop down
            _resolutionDropdown.ClearOptions();

            //format options
            List<Dropdown.OptionData> options = Screen.resolutions.Select(r => new Dropdown.OptionData()
            {
                text = $"{r.width}x{r.height}"
            }).ToList();

            //add options
            _resolutionDropdown.AddOptions(options);

            //get preferred resolution
            _resolution = _gameManager.DataManager.GetResolutionPreference();

            //default to full screen if no preference found
            if (string.IsNullOrWhiteSpace(_resolution))
                _resolution = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";

            //set dropdown to selected value
            for (int i = 0; i < _resolutionDropdown.options.Count; i++)
            {
                if (_resolutionDropdown.options[i].text == _resolution)
                    _resolutionDropdown.value = i;
            }

            _resolutionDropdown.onValueChanged.AddListener(_handleResolutionSelection);
        }

        private void _setupWindowedModeDropdown()
        {
            //clear drop down
            _windowedModeDropdown.ClearOptions();

            //format options
            List<Dropdown.OptionData> options = _supportedWindowedModes.Select(mode => new Dropdown.OptionData()
            {
                text = mode.Value
            }).ToList();

            //add options
            _windowedModeDropdown.AddOptions(options);

            //get preferred mode
            string currentModeDisplayText = null;

            try
            {
                currentModeDisplayText = _supportedWindowedModes.First(mode => mode.Key == Screen.fullScreenMode).Value;
            }
            catch
            {

            }

            if (currentModeDisplayText == null)
                currentModeDisplayText = _supportedWindowedModes.First(mode => mode.Key == FullScreenMode.FullScreenWindow).Value;

            //set dropdown to selected value
            for (int i = 0; i < _windowedModeDropdown.options.Count; i++)
            {
                if (_windowedModeDropdown.options[i].text == currentModeDisplayText)
                    _windowedModeDropdown.value = i;
            }

            _windowedModeDropdown.onValueChanged.AddListener(_handleWindowedModeSelection);
        }

        private void _setupVSyncDropdown()
        {
            //clear drop down
            _vSyncDropdown.ClearOptions();

            int currentRefreshRate = Screen.currentResolution.refreshRate;
            
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>()
            {
                new Dropdown.OptionData()
                {
                    text = "Off"
                }
            };

            //make other options
            for (VSyncCountEnum i = VSyncCountEnum.EveryVBlank; i <= VSyncCountEnum.EveryFourVBlanks; i++)
            {
                options.Add(new Dropdown.OptionData()
                {
                    text = $"{currentRefreshRate / (int)i} Hz"
                });
            }

            //add options
            _vSyncDropdown.AddOptions(options);

            //set dropdown to preferred value
            _vSyncDropdown.value = (int)_vSyncCount;

            _vSyncDropdown.onValueChanged.AddListener(_handleVSyncCountSelection);
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

        private void _handleColorSelectionButton()
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
                _gameManager.LoadScene(SceneNames.ColorSelection);
            }
        }

        private void _handleQualitySettingDropdown(Dropdown dropdown, int index, EnumData<QualitySettingEnum> repo, Action<QualitySettingEnum> setQualitySettingValue)
        {
            string selectedText = dropdown.options[index].text;

            if (Enum.TryParse(selectedText, out QualitySettingEnum selectedQualitySetting))
            {
                setQualitySettingValue(selectedQualitySetting);
                repo.Set(selectedQualitySetting);
            }
        }

        private void _handleResolutionSelection(int index)
        {
            _resolution = _resolutionDropdown.options[index].text;

            _gameManager.DataManager.SetResolutionPreference(_resolution);

            _setResolution(_resolution, _windowedMode);
        }
        
        private void _handleWindowedModeSelection(int index)
        {
            string selectedWindowedModeText = _windowedModeDropdown.options[index].text;

            try
            {
                _windowedMode = _supportedWindowedModes.First(mode => mode.Value == selectedWindowedModeText).Key;
            }
            catch
            {
                return;
            }

            _gameManager.DataManager.WindowedModePreference.Set(_windowedMode);

            _setResolution(_resolution, _windowedMode);
        }

        private void _handleVSyncCountSelection(int index)
        {
            _gameManager.DataManager.VSyncCountPreference.Set((VSyncCountEnum)index);
            QualitySettings.vSyncCount = index;
        }

        private void _setResolution(string resolution, FullScreenMode windowedMode)
        {
            string[] splitResolution = resolution.Split('x');
            Screen.SetResolution(int.Parse(splitResolution[0]), int.Parse(splitResolution[1]), windowedMode);

            //need to update vsync dropdown when we change resolution because that's where we get Hz from
            _setupVSyncDropdown();
        }
    }
}
