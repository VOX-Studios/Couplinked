﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class PlayerModeSelectionSceneManager : MonoBehaviour
    {
        private GameManager _gameManager;

        [SerializeField]
        private Button _singlePlayerButton;

        [SerializeField]
        private Button _multiplayerButton;
        
        void Start()
        {
            EventSystem.current.SetSelectedGameObject(_singlePlayerButton.gameObject);
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _singlePlayerButton.onClick.AddListener(_handleSinglePlayerButton);
            _multiplayerButton.onClick.AddListener(_handleMultiplayerButton);
        }

        void Update()
        {
            if (_gameManager.HandleBack())
            {
                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene(SceneNames.Start);
            }
        }

        private void _handleSinglePlayerButton()
        {
            _gameManager.GameSetupInfo.Teams = new List<Team>()
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
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.LoadScene(SceneNames.GameModeSelection);
        }

        private void _handleMultiplayerButton()
        {
            _gameManager.GameSetupInfo.Teams = null;
            _gameManager.SoundEffectManager.PlaySelect();
            _gameManager.LoadScene(SceneNames.MultiplayerControllerSelection);
        }
    }
}
