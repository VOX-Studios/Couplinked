using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers.ColorSelection
{
    class ColorSelectionSceneManager : MonoBehaviour, IHitCollisionHandler
    {
        private GameManager _gameManager;

        private ColorSelectionSceneManagerStateEnum _state;
        private ColorSettingEnum _currentColorSetting;

        [SerializeField]
        NoHitManager noHitManager;

        [SerializeField]
        HitManager hitManager;

        [SerializeField]
        HitSplitManager hitSplitManager;

        [SerializeField]
        private ExplosionManager _explosionManager;

        [SerializeField]
        private GameObject _nodePrefab;

        [SerializeField]
        private GameObject _lightningManagerPrefab;

        [SerializeField]
        private GameObject _hitPrefab;

        [SerializeField]
        private Transform _midground;

        [SerializeField]
        private Transform _foreground;

        [SerializeField]
        private Transform _node1StartingPosition;

        [SerializeField]
        private Transform _node2StartingPosition;

        [SerializeField]
        private Transform _hit1StartingPosition;

        [SerializeField]
        private Transform _hit2StartingPosition;

        private NodePairing _nodePair;
        private Hit _hit1;
        private Hit _hit2;

        private float _hitRespawnInterval = 1;
        private float _hit1RespawnTimer;
        private float _hit2RespawnTimer;

        [SerializeField]
        private GameObject _menuPanel;

        [SerializeField]
        private Button _node1InsideButton;

        [SerializeField]
        private Button _node1OutsideButton;

        [SerializeField]
        private Button _node1ParticlesButton;

        [SerializeField]
        private Button _node2InsideButton;

        [SerializeField]
        private Button _node2OutsideButton;

        [SerializeField]
        private Button _node2ParticlesButton;

        [SerializeField]
        private Button _lightningButton;

        [SerializeField]
        private Button _gridButton;


        [SerializeField]
        private Image _node1InsideColorPreviewImage;

        [SerializeField]
        private Image _node1OutsideColorPreviewImage;

        [SerializeField]
        private Image _node1ParticlesColorPreviewImage;

        [SerializeField]
        private Image _node2InsideColorPreviewImage;

        [SerializeField]
        private Image _node2OutsideColorPreviewImage;

        [SerializeField]
        private Image _node2ParticlesColorPreviewImage;

        [SerializeField]
        private Image _lightningColorPreviewImage;

        [SerializeField]
        private Image _gridColorPreviewImage;


        [SerializeField]
        private GameObject _slidersPanel;

        [SerializeField]
        private Text _currentSettingText;

        [SerializeField]
        private Slider _rSlider;

        [SerializeField]
        private Slider _gSlider;

        [SerializeField]
        private Slider _bSlider;

        private Color _currentColor = Color.white;

        private int _playerIndex = 0;
        private Color _node1InsideColor;
        private Color _node1OutsideColor;
        private Color _node1ParticlesColor;
        private Color _node2InsideColor;
        private Color _node2OutsideColor;
        private Color _node2ParticlesColor;
        private Color _lightningColor;
        private Color _gridColor;

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            _state = ColorSelectionSceneManagerStateEnum.Menu;

            EventSystem.current.SetSelectedGameObject(_node1InsideButton.gameObject);

            _setupSinglePlayer(0);

            _slidersPanel.SetActive(false);

            _node1InsideButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Node1Inside));
            _node1OutsideButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Node1Outside));
            _node1ParticlesButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Node1Particles));
            _node2InsideButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Node2Inside));
            _node2OutsideButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Node2Outside));
            _node2ParticlesButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Node2Particles));
            _lightningButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Lightning));
            _gridButton.onClick.AddListener(() => _handleMenuSelection(ColorSettingEnum.Grid));

            _rSlider.onValueChanged.AddListener(_handleRSliderChange);
            _gSlider.onValueChanged.AddListener(_handleGSliderChange);
            _bSlider.onValueChanged.AddListener(_handleBSliderChange);

            /*
            hitManager.Initialize();
            noHitManager.Initialize();
            hitSplitManager.Initialize();
            */
            _explosionManager.Initialize(_gameManager.DataManager);

            CustomPlayerColorData playerColors = _gameManager.DataManager.PlayerColors[_playerIndex];
            _node1InsideColor = playerColors.NodeColors[0].InsideColor.Get();
            _node1OutsideColor = playerColors.NodeColors[0].OutsideColor.Get();
            _node1ParticlesColor = playerColors.NodeColors[0].ParticleColor.Get();
            _node2InsideColor = playerColors.NodeColors[1].InsideColor.Get();
            _node2OutsideColor = playerColors.NodeColors[1].OutsideColor.Get();
            _node2ParticlesColor = playerColors.NodeColors[1].ParticleColor.Get();
            _lightningColor = playerColors.LightningColor.Get();
            _gridColor = playerColors.GridColor.Get();

            _node1InsideColorPreviewImage.color = _node1InsideColor;
            _node1OutsideColorPreviewImage.color = _node1OutsideColor;
            _node1ParticlesColorPreviewImage.color = _node1ParticlesColor;
            _node2InsideColorPreviewImage.color =_node2InsideColor;
            _node2OutsideColorPreviewImage.color = _node2OutsideColor;
            _node2ParticlesColorPreviewImage.color = _node2ParticlesColor;
            _lightningColorPreviewImage.color = _lightningColor;
            _gridColorPreviewImage.color = _gridColor;
        }

        private void _handleMenuSelection(ColorSettingEnum colorSetting)
        {
            _gameManager.SoundEffectManager.PlaySelect();

            _currentColorSetting = colorSetting;

            switch (colorSetting)
            {
                case ColorSettingEnum.Node1Inside:
                    _currentSettingText.text = "NODE 1 INSIDE";
                    break;
                case ColorSettingEnum.Node1Outside:
                    _currentSettingText.text = "NODE 1 OUTSIDE";
                    break;
                case ColorSettingEnum.Node1Particles:
                    _currentSettingText.text = "NODE 1 PARTICLES";
                    break;
                case ColorSettingEnum.Node2Inside:
                    _currentSettingText.text = "NODE 2 INSIDE";
                    break;
                case ColorSettingEnum.Node2Outside:
                    _currentSettingText.text = "NODE 2 OUTSIDE";
                    break;
                case ColorSettingEnum.Node2Particles:
                    _currentSettingText.text = "NODE 2 PARTICLES";
                    break;
                case ColorSettingEnum.Lightning:
                    _currentSettingText.text = "LIGHTNING";
                    break;
                case ColorSettingEnum.Grid:
                    _currentSettingText.text = "GRID";
                    break;
            }

            _transitionToColorSelection();
        }

        private void _handleRSliderChange(float value)
        {
            _currentColor = new Color(value / 255, _currentColor.g, _currentColor.b);
            _setColorOnObjects();
        }

        private void _handleGSliderChange(float value)
        {
            _currentColor = new Color(_currentColor.r, value / 255, _currentColor.b);
            _setColorOnObjects();
        }

        private void _handleBSliderChange(float value)
        {
            _currentColor = new Color(_currentColor.r, _currentColor.g, value / 255);
            _setColorOnObjects();
        }

        private void _setColorOnObjects()
        {
            switch (_currentColorSetting)
            {
                case ColorSettingEnum.Node1Inside:
                    _node1InsideColor = _currentColor;
                    _nodePair.Nodes[0].SetColors(_node1InsideColor, _node1OutsideColor);
                    break;
                case ColorSettingEnum.Node1Outside:
                    _node1OutsideColor = _currentColor;
                    _nodePair.Nodes[0].SetColors(_node1InsideColor, _node1OutsideColor);
                    _hit1.SetColor(_node1OutsideColor);
                    break;
                case ColorSettingEnum.Node1Particles:
                    _node1ParticlesColor = _currentColor;
                    _nodePair.Nodes[0].SetParticleColor(_node1ParticlesColor);
                    break;
                case ColorSettingEnum.Node2Inside:
                    _node2InsideColor = _currentColor;
                    _nodePair.Nodes[1].SetColors(_node2InsideColor, _node2OutsideColor);
                    break;
                case ColorSettingEnum.Node2Outside:
                    _node2OutsideColor = _currentColor;
                    _nodePair.Nodes[1].SetColors(_node2InsideColor, _node2OutsideColor);
                    _hit2.SetColor(_node2OutsideColor);
                    break;
                case ColorSettingEnum.Node2Particles:
                    _node2ParticlesColor = _currentColor;
                    _nodePair.Nodes[1].SetParticleColor(_node2ParticlesColor);
                    break;
                case ColorSettingEnum.Lightning:
                    _lightningColor = _currentColor;
                    _nodePair.LightningManager.SetLightningColor(_lightningColor);
                    break;
                case ColorSettingEnum.Grid:
                    _gridColor = _currentColor;
                    _gameManager.Grid.ColorManager.SetColor(_gridColor);
                    break;
            }
        }

        private void _setupSinglePlayer(int playerIndex)
        {
            _nodePair = _makeNodePair(playerIndex);
            
            _nodePair.Nodes[0].transform.position = _convertFromPlaceholderPosition(_node1StartingPosition.position);
            _nodePair.Nodes[1].transform.position = _convertFromPlaceholderPosition(_node2StartingPosition.position);

            _hit1 = _makeHit(playerIndex, 0);
            _hit1.transform.position = _convertFromPlaceholderPosition(_hit1StartingPosition.position);

            _hit2 = _makeHit(playerIndex, 1);
            _hit2.transform.position = _convertFromPlaceholderPosition(_hit2StartingPosition.position);
        }

        private Vector3 _convertFromPlaceholderPosition(Vector3 placeholderPosition)
        {
            Vector3 rawConverted = Camera.main.ScreenToWorldPoint(placeholderPosition);
            return new Vector3(rawConverted.x, rawConverted.y);
        }

        public void OnHitCollision(Hit hit, Collider2D other)
        {
            if (hit == _hit1)
            {
                _explosionManager.ActivateExplosion(_hit1.transform.position, _node1ParticlesColor);
                _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE + _hit1.Scale, _hit1.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * _hit1.Scale);
                _hit1.gameObject.SetActive(false);
                _hit1RespawnTimer = _hitRespawnInterval;
            }
            else if (hit == _hit2)
            {
                _explosionManager.ActivateExplosion(_hit2.transform.position, _node2ParticlesColor);
                _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * _hit2.Scale, _hit2.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * _hit2.Scale);
                _hit2.gameObject.SetActive(false);
                _hit2RespawnTimer = _hitRespawnInterval;
            }
        }

        private Hit _makeHit(int playerIndex, int hitType)
        {
            GameObject hitGameObject = GameObject.Instantiate(_hitPrefab);
            hitGameObject.transform.SetParent(_foreground, false);

            Hit hitComponent = hitGameObject.GetComponent<Hit>();
            hitComponent.Initialize(this);

            CustomPlayerColorData playerColorData = _gameManager.DataManager.PlayerColors[playerIndex];

            hitComponent.SetColor(playerColorData.NodeColors[hitType].OutsideColor.Get());

            return hitComponent;
        }

        private NodePairing _makeNodePair(int playerIndex)
        {
            GameObject node1GameObject = GameObject.Instantiate(_nodePrefab);
            GameObject node2GameObject = GameObject.Instantiate(_nodePrefab);

            node1GameObject.name = "Node1";
            node2GameObject.name = "Node2";

            node1GameObject.transform.SetParent(_foreground, false);
            node2GameObject.transform.SetParent(_foreground, false);

            Node node1 = node1GameObject.GetComponent<Node>();
            Node node2 = node2GameObject.GetComponent<Node>();

            node1.TeamId = playerIndex;
            node2.TeamId = playerIndex;
            node1.NodeId = 0;
            node2.NodeId = 1;

            CustomPlayerColorData playerColorData = _gameManager.DataManager.PlayerColors[playerIndex];
           
            node1.SetColors(playerColorData.NodeColors[0].InsideColor.Get(), playerColorData.NodeColors[0].OutsideColor.Get());
            node2.SetColors(playerColorData.NodeColors[1].InsideColor.Get(), playerColorData.NodeColors[1].OutsideColor.Get());

            node1.SetParticleColor(playerColorData.NodeColors[0].ParticleColor.Get());
            node2.SetParticleColor(playerColorData.NodeColors[1].ParticleColor.Get());

            GameObject lightningManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
            LightningManager lightningManager = lightningManagerGameObject.GetComponent<LightningManager>();

            lightningManager.Initialize(_midground);
            lightningManager.SetLightningColor(playerColorData.LightningColor.Get());

            NodePairing nodePair = new NodePairing(new List<Node>() { node1, node2 }, lightningManager);

            _setupNodeTrail(node1.ParticleSystem);
            _setupNodeTrail(node2.ParticleSystem);

            return nodePair;
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                if (_handleBack())
                    return;
            }

            noHitManager.Run(Time.deltaTime);
            hitManager.Run(Time.deltaTime);
            hitSplitManager.Run(Time.deltaTime);
            _explosionManager.Run();

            _nodePair.LightningManager.Run(_nodePair.Nodes[0].transform.position, _nodePair.Nodes[1].transform.position);

            _handleParticles(Time.deltaTime);
        }

        private void _handleParticles(float deltaTime)
        {
            //if selecting node 1 particle color and hit 1 is active
            if (_state == ColorSelectionSceneManagerStateEnum.ColorSelection && _currentColorSetting == ColorSettingEnum.Node1Particles && _hit1.gameObject.activeInHierarchy)
            {
                //move towards hit 1
                _nodePair.Nodes[0].transform.position = Vector3.Lerp(_nodePair.Nodes[0].transform.position, _hit1.transform.position, deltaTime * 5);
            }
            else
            {
                //move towards starting position
                _nodePair.Nodes[0].transform.position = Vector3.Lerp(_nodePair.Nodes[0].transform.position, _convertFromPlaceholderPosition(_node1StartingPosition.position), deltaTime * 5);
                _hit1RespawnTimer -= deltaTime;

                if (_hit1RespawnTimer <= 0)
                    _hit1.gameObject.SetActive(true);
            }


            //if selecting node 2 particle color and hit 2 is active
            if (_state == ColorSelectionSceneManagerStateEnum.ColorSelection && _currentColorSetting == ColorSettingEnum.Node2Particles && _hit2.gameObject.activeInHierarchy)
            {
                //move towards hit 1
                _nodePair.Nodes[1].transform.position = Vector3.Lerp(_nodePair.Nodes[1].transform.position, _hit2.transform.position, deltaTime * 5);
            }
            else
            {
                //move towards starting position
                _nodePair.Nodes[1].transform.position = Vector3.Lerp(_nodePair.Nodes[1].transform.position, _convertFromPlaceholderPosition(_node2StartingPosition.position), deltaTime * 5);
                _hit2RespawnTimer -= deltaTime;

                if (_hit2RespawnTimer <= 0)
                    _hit2.gameObject.SetActive(true);
            }
        }    

        private bool _handleBack()
        {
            _gameManager.SoundEffectManager.PlayBack();
            switch (_state)
            {
                default:
                case ColorSelectionSceneManagerStateEnum.Menu:
                    _gameManager.LoadScene(SceneNames.Graphics);
                    return true;
                case ColorSelectionSceneManagerStateEnum.ColorSelection:
                    _transitionToMenu();
                    return false;
            }
        }

        private void _transitionToColorSelection()
        {
            switch (_currentColorSetting)
            {
                case ColorSettingEnum.Node1Inside:
                    _currentColor = _node1InsideColor;
                    break;
                case ColorSettingEnum.Node1Outside:
                    _currentColor = _node1OutsideColor;
                    break;
                case ColorSettingEnum.Node1Particles:
                    _currentColor = _node1ParticlesColor;
                    break;
                case ColorSettingEnum.Node2Inside:
                    _currentColor = _node2InsideColor;
                    break;
                case ColorSettingEnum.Node2Outside:
                    _currentColor = _node2OutsideColor;
                    break;
                case ColorSettingEnum.Node2Particles:
                    _currentColor = _node2ParticlesColor;
                    break;
                case ColorSettingEnum.Lightning:
                    _currentColor = _lightningColor;
                    break;
                case ColorSettingEnum.Grid:
                    _currentColor = _gridColor;
                    break;
            }

            _rSlider.value = _currentColor.r * 255;
            _gSlider.value = _currentColor.g * 255;
            _bSlider.value = _currentColor.b * 255;
            _menuPanel.SetActive(false);
            _slidersPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_rSlider.gameObject);

            _state = ColorSelectionSceneManagerStateEnum.ColorSelection;
        }

        private void _transitionToMenu()
        {
            _slidersPanel.SetActive(false);
            _menuPanel.SetActive(true);

            _saveCurrentColor();

            GameObject gameObjectToSelect;
            switch (_currentColorSetting)
            {
                default:
                case ColorSettingEnum.Node1Inside:
                    gameObjectToSelect = _node1InsideButton.gameObject;
                    _node1InsideColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Node1Outside:
                    gameObjectToSelect = _node1OutsideButton.gameObject;
                    _node1OutsideColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Node1Particles:
                    gameObjectToSelect = _node1ParticlesButton.gameObject;
                    _node1ParticlesColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Node2Inside:
                    gameObjectToSelect = _node2InsideButton.gameObject;
                    _node2InsideColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Node2Outside:
                    gameObjectToSelect = _node2OutsideButton.gameObject;
                    _node2OutsideColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Node2Particles:
                    gameObjectToSelect = _node2ParticlesButton.gameObject;
                    _node2ParticlesColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Lightning:
                    gameObjectToSelect = _lightningButton.gameObject;
                    _lightningColorPreviewImage.color = _currentColor;
                    break;
                case ColorSettingEnum.Grid:
                    gameObjectToSelect = _gridButton.gameObject;
                    _gridColorPreviewImage.color = _currentColor;
                    break;

            }
            EventSystem.current.SetSelectedGameObject(gameObjectToSelect);
            _state = ColorSelectionSceneManagerStateEnum.Menu;
        }

        private void _saveCurrentColor()
        {
            switch (_currentColorSetting)
            {
                default:
                case ColorSettingEnum.Node1Inside:
                    _gameManager.DataManager.PlayerColors[_playerIndex].NodeColors[0].InsideColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Node1Outside:
                    _gameManager.DataManager.PlayerColors[_playerIndex].NodeColors[0].OutsideColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Node1Particles:
                    _gameManager.DataManager.PlayerColors[_playerIndex].NodeColors[0].ParticleColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Node2Inside:
                    _gameManager.DataManager.PlayerColors[_playerIndex].NodeColors[1].InsideColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Node2Outside:
                    _gameManager.DataManager.PlayerColors[_playerIndex].NodeColors[1].OutsideColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Node2Particles:
                    _gameManager.DataManager.PlayerColors[_playerIndex].NodeColors[1].ParticleColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Lightning:
                    _gameManager.DataManager.PlayerColors[_playerIndex].LightningColor.Set(_currentColor);
                    break;
                case ColorSettingEnum.Grid:
                    _gameManager.DataManager.PlayerColors[_playerIndex].GridColor.Set(_currentColor);
                    break;
            }
        }

        private void _setupNodeTrail(ParticleSystem particleSystem)
        {
            QualitySettingEnum qualitySetting = _gameManager.DataManager.TrailParticleQuality.Get();
            ParticleSystem.EmissionModule emission = particleSystem.emission;

            switch (qualitySetting)
            {
                default:
                case QualitySettingEnum.Off:
                    emission.rateOverDistanceMultiplier = 0f;
                    break;
                case QualitySettingEnum.Low:
                    emission.rateOverDistanceMultiplier = 75f;
                    break;
                case QualitySettingEnum.Medium:
                    emission.rateOverDistanceMultiplier = 150f;
                    break;
                case QualitySettingEnum.High:
                    emission.rateOverDistanceMultiplier = 300f;
                    break;
            }
        }

        private void _clearGame()
        {
            for (int i = hitManager.activeHits.Count - 1; i >= 0; i--)
            {
                hitManager.DeactivateHit(i);
            }

            for (int i = noHitManager.activeNoHits.Count - 1; i >= 0; i--)
            {
                noHitManager.DeactivateNoHit(i);
            }

            for (int i = hitSplitManager.activeHitSplits.Count - 1; i >= 0; i--)
            {
                hitSplitManager.DeactivateHitSplit(i);
            }

            _explosionManager.DeactiveExplosions();
        }
    }
}
