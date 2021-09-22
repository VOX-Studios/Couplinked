using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.SceneManagers
{
    class MultiplayerControllerSelectionSceneManager : MonoBehaviour
    {
        private readonly int _numPlayersRequired = 2;

        [SerializeField]
        private GameObject _controllerSelectionPanelPrefab;

        [SerializeField]
        private RectTransform _panelsContainer;

        private GameManager _gameManager;

        private Dictionary<PlayerInput, ControllerSelectionState> _playerStates;

        private ControllerSelectionState[] _controllerSelectionStates;

        private float _delayUpdate = .05f;

        void Awake()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _controllerSelectionStates = new ControllerSelectionState[_numPlayersRequired];

            for (int i = 0; i < _numPlayersRequired; i++)
            {
                GameObject controllerSelectionPanel = GameObject.Instantiate(_controllerSelectionPanelPrefab);
                controllerSelectionPanel.transform.SetParent(_panelsContainer.transform, false);

                ControllerSelectionState controllerSelectionState = controllerSelectionPanel.GetComponent<ControllerSelectionState>();
                _controllerSelectionStates[i] = controllerSelectionState;
            }

            _playerStates = new Dictionary<PlayerInput, ControllerSelectionState>();

            for (int i = 0; i < _gameManager.PlayerManager.Players.Count; i++)
            {
                PlayerInput player = _gameManager.PlayerManager.Players[i];
                player.Lobby.Enable();
                player.Game.Disable();

                //set state to joined and ready if we already have players in the player manager (this means we backed out of a multiplayer game where we already had players)
                ControllerSelectionState controllerSelectionState = _getFirstAvailableControllerSelectionState(_controllerSelectionStates);
                controllerSelectionState.SetIsJoined(true);
                controllerSelectionState.SetIsReady(true);

                _playerStates[player] = controllerSelectionState;
            }
        }

        private void _onPlayerAdded(object sender, PlayerAddedEventArgs e)
        {
            e.PlayerAdded.Lobby.Enable();
            e.PlayerAdded.Game.Disable();

            ControllerSelectionState controllerSelectionState = _getFirstAvailableControllerSelectionState(_controllerSelectionStates);
            controllerSelectionState.WasJustAdded = true;
            controllerSelectionState.SetIsJoined(true);

            _playerStates[e.PlayerAdded] = controllerSelectionState;

            if(_gameManager.PlayerManager.Players.Count == PlayerManager.MAX_PLAYERS)
            {
                _gameManager.PlayerManager.EndJoining();
            }
        }

        private ControllerSelectionState _getFirstAvailableControllerSelectionState(ControllerSelectionState[] controllerSelectionStates)
        {
            for (int i = 0; i < controllerSelectionStates.Length; i++)
            {
                ControllerSelectionState controllerSelectionState = controllerSelectionStates[i];

                //if not in the joined state, return this one
                if(!controllerSelectionState.IsJoined)
                {
                    return controllerSelectionState;
                }
            }

            return null;
        }

        void Update()
        {
            //delaying initial update processing because otherwise players join on enter
            if(_delayUpdate > 0)
            {
                _delayUpdate -= Time.deltaTime;

                if(_delayUpdate <= 0)
                {
                    _gameManager.PlayerManager.OnPlayerAdded += _onPlayerAdded;
                    _gameManager.PlayerManager.BeginJoining();
                }
                return;
            }

            //if the back button is pressed and none of the joined players are currently pressing the back input (this is to prevent accidental exits)
            if (_gameManager.HandleBack() && !_gameManager.PlayerManager.Players.Any(player => player.Lobby.BackInput))
            {
                _onExitScene();
                for (int i = _gameManager.PlayerManager.Players.Count - 1; i >= 0; i--)
                {
                    PlayerInput player = _gameManager.PlayerManager.Players[i];
                    _removePlayer(player);
                }

                _gameManager.SoundEffectManager.PlayBack();
                _gameManager.LoadScene(SceneNames.PlayerModeSelection);
                return;
            }

            for(int i = _gameManager.PlayerManager.Players.Count - 1; i >= 0; i--)
            {
                PlayerInput player = _gameManager.PlayerManager.Players[i];

                if(player.Lobby.SelectInput && _playerStates[player].WasJustAdded)
                {
                    _playerStates[player].WasJustAdded = false;
                    continue;
                }

                //if a player hits the ready button
                if (player.Lobby.SelectInput)
                {
                    //if they're already ready + everyone else is ready
                    if (_playerStates[player].IsReady && _shouldStartGame())
                    {
                        _startGame();
                        return;
                    }
                    else
                    {
                        //set ready state to true
                        _playerStates[player].SetIsReady(true);
                    }
                }

                //if a player hits the back button
                if (player.Lobby.BackInput)
                {
                    //if the player is ready
                    if (_playerStates[player].IsReady)
                    {
                        //set ready state to false
                        _playerStates[player].SetIsReady(false);
                    }
                    else //player was not ready so drop
                    {
                        _removePlayer(player);
                        
                        //being joining again if we had stopped previously
                        if(!_gameManager.PlayerManager.IsJoining)
                            _gameManager.PlayerManager.BeginJoining();
                    }
                }
            }
        }

        private void _removePlayer(PlayerInput player)
        {
            ControllerSelectionState controllerSelectionState = _playerStates[player];
            controllerSelectionState.SetIsJoined(false);
            
            _playerStates.Remove(player);
            _gameManager.PlayerManager.RemovePlayer(player);
        }

        private bool _shouldStartGame()
        {
            if (_playerStates.Count < _numPlayersRequired)
                return false;

            bool shouldStartGame = true;
            foreach (ControllerSelectionState controllerSelectionState in _playerStates.Values)
            {
                //if any player is NOT ready, we shouldn't be starting the game
                if (!controllerSelectionState.IsReady)
                {
                    shouldStartGame = false;
                    break;
                }
            }

            return shouldStartGame;
        }

        private void _onExitScene()
        {
            _gameManager.PlayerManager.EndJoining();
            _gameManager.PlayerManager.OnPlayerAdded -= _onPlayerAdded;
        }

        private void _startGame()
        {
            _onExitScene();

            _gameManager.SoundEffectManager.PlaySelect();

            _gameManager.LoadScene(SceneNames.MultiplayerGameModeSelection);
        }
    }
}
