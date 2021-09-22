using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class InstructionsSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private GameObject InstructionsCircle1;

        [SerializeField]
        private GameObject InstructionsCircle2;

        private GameObject _currentPage;
        private GameObject _nextPage;

        [SerializeField]
        private GameObject _instructionsPages;

        private int _instructionsPageNum = 1;
        private int _lastInstructionsPageNum;

        [SerializeField]
        private Button _instructionsLeft;

        [SerializeField]
        private Button _instructionsRight;

        private bool _isTransitioning;

        [SerializeField]
        private GameObject _lightningManagerPrefab;
        private LightningManager _lightningManager;

        [SerializeField]
        private Transform _lightningHolder;

        void Start()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _instructionsLeft.onClick.AddListener(_handleInstructionsLeftButton);
            _instructionsRight.onClick.AddListener(_handleInstructionsRightButton);

            _isTransitioning = false;
            _currentPage = null;

            _nextPage = null;

            _instructionsPageNum = 1;

            Transform containerTransform = GameObject.Find("Page 2").transform.Find("Graphics").transform.Find("Container");

            PlayerColorData playerColorData = _gameManager.DataManager.PlayerColors[0];

            GameObject lightningManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
            _lightningManager = lightningManagerGameObject.GetComponent<LightningManager>();

            _lightningManager.Initialize(_lightningHolder, playerColorData.LightningColor.Get());
            _lastInstructionsPageNum = _instructionsPages.transform.childCount;

            for (int i = 1; i < _lastInstructionsPageNum; i++)
            {
                Transform pageTransform = _instructionsPages.transform.GetChild(i);
                pageTransform.GetComponent<Animator>().enabled = false;
                pageTransform.localPosition = new Vector3(5120, pageTransform.localPosition.y, pageTransform.localPosition.z);
            }
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();

                _gameManager.LoadScene(SceneNames.Start);
                return;
            }

            if (_instructionsPageNum == 1)
                _instructionsLeft.GetComponent<Image>().enabled = false;
            else
                _instructionsLeft.GetComponent<Image>().enabled = true;

            if (_instructionsPageNum == _lastInstructionsPageNum)
                _instructionsRight.GetComponent<Image>().enabled = false;
            else
                _instructionsRight.GetComponent<Image>().enabled = true;

            
            if (_lightningManager.gameObject.activeInHierarchy)
            {
                _lightningManager.Run(_convertFromPlaceholderPosition(InstructionsCircle1.transform.position), _convertFromPlaceholderPosition(InstructionsCircle2.transform.position));
            }

            if (!_isTransitioning)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                    _handleInstructionsLeftButton();
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                    _handleInstructionsRightButton();
            }
        }
        private Vector3 _convertFromPlaceholderPosition(Vector3 placeholderPosition)
        {
            Vector3 rawConverted = Camera.main.ScreenToWorldPoint(placeholderPosition);
            return new Vector3(rawConverted.x, rawConverted.y);
        }

        public void InstructionsTransitionCompleteEvent()
        {
            _currentPage = null;
            _nextPage = null;

            _isTransitioning = false;
        }

        private void _handleInstructionsLeftButton()
        {
            if (_isTransitioning)
                return;

            if (_instructionsPageNum == 1)
            {
                _gameManager.SoundEffectManager.PlayBack();
            }
            else
            {
                _gameManager.SoundEffectManager.PlaySelect();
                _changeInstructionsPage(false);
            }
        }

        private void _handleInstructionsRightButton()
        {
            if (_isTransitioning)
                return;

            if (_instructionsPageNum == _lastInstructionsPageNum)
                _gameManager.SoundEffectManager.PlayBack();
            else
            {
                _gameManager.SoundEffectManager.PlaySelect();
                _changeInstructionsPage(true);
            }
        }
        
        private void _changeInstructionsPage(bool forward)
        {
            _isTransitioning = true;

            _currentPage = _instructionsPages.transform.GetChild(_instructionsPageNum - 1).gameObject;

            if (forward)
            {
                _currentPage.GetComponent<Animator>().SetTrigger("Move Off To Left");

                //If we were on the last page
                if (_instructionsPageNum == _lastInstructionsPageNum)
                    _instructionsPageNum = 1;
                else
                    _instructionsPageNum++;

                _nextPage = _instructionsPages.transform.GetChild(_instructionsPageNum - 1).gameObject;

                _nextPage.GetComponent<Animator>().enabled = true;
                _nextPage.GetComponent<Animator>().SetTrigger("Move In From Right");
            }
            else
            {
                _currentPage.GetComponent<Animator>().SetTrigger("Move Off To Right");

                //If we were on the first page
                if (_instructionsPageNum == 1)
                    _instructionsPageNum = _lastInstructionsPageNum;
                else
                    _instructionsPageNum--;

                _nextPage = _instructionsPages.transform.GetChild(_instructionsPageNum - 1).gameObject;

                _nextPage.GetComponent<Animator>().SetTrigger("Move In From Left");
            }
        }
    }
}
