using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class LevelEditorSelectionSceneManager : MonoBehaviour
    {
        GameManager _gameManager;

        [SerializeField]
        private Button _newLevelButton;

        [SerializeField]
        private Button _openLevelButton;

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_openLevelButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _newLevelButton.onClick.AddListener(_handleNewLevelButton);
            _openLevelButton.onClick.AddListener(_handleOpenLevelButton);

            _gameManager.TheLevelSelectionMode = LevelTypeEnum.LevelEditor;
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene(SceneNames.Start);
            }
        }

        private void _handleOpenLevelButton()
        {
            _gameManager.IsNewLevel = false;
            _gameManager.ResetLevelDisplayNumbers();

            _gameManager.TheLevelSelectionMode = LevelTypeEnum.LevelEditor;
            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.LoadScene(SceneNames.Levels);
        }

        private void _handleNewLevelButton()
        {
            _gameManager.IsNewLevel = true;
            _gameManager.CurrentLevel = new Level(); //TODO: change number of rows to be configurable

            _gameManager.TheLevelSelectionMode = LevelTypeEnum.LevelEditor;
            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.LoadScene(SceneNames.LevelEditor);
        }
    }
}
