using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.SceneManagers
{
    class MultiplayerControllerSelectionSceneManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _nodePrefab;

        [SerializeField]
        private GameObject _lightningManagerPrefab;

        [SerializeField]
        private GameObject _controllerSelectionTeamPanelPrefab;

        [SerializeField]
        private RectTransform _panelsContainer;

        private GameManager _gameManager;

        private Dictionary<PlayerInput, ControllerSelectionState> _playerStates;

        [SerializeField]
        private Transform _nodeMidground;

        [SerializeField]
        private Transform _nodeForeground;

        private List<GameObject> _teamSelectionPanels;

        void Awake()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _playerStates = new Dictionary<PlayerInput, ControllerSelectionState>();
            _teamSelectionPanels = new List<GameObject>();

            //create the panels
            for (int i = 0; i < PlayerManager.MAX_PLAYERS; i++)
            {
                GameObject teamSelectionPanel = GameObject.Instantiate(_controllerSelectionTeamPanelPrefab);
                teamSelectionPanel.transform.SetParent(_panelsContainer.transform, false);
                _teamSelectionPanels.Add(teamSelectionPanel);
            }
            
            //force the panels to update
            HorizontalLayoutGroup teamPanelsLayoutGroup = _panelsContainer.GetComponent<HorizontalLayoutGroup>();
            teamPanelsLayoutGroup.CalculateLayoutInputHorizontal();
            teamPanelsLayoutGroup.CalculateLayoutInputVertical();
            teamPanelsLayoutGroup.SetLayoutHorizontal();
            teamPanelsLayoutGroup.SetLayoutVertical();

            //if we already had teams from a previous game
            if (_gameManager.GameSetupInfo.Teams != null)
            {
                foreach(Team team in _gameManager.GameSetupInfo.Teams)
                {
                    foreach (PlayerInput playerInput in team.PlayerInputs)
                    {
                        _setupNewPlayer(playerInput, team.TeamSlot);
                    }
                }
            }

            //hook into player added event
            _gameManager.PlayerManager.OnPlayerAdded += _onPlayerAdded;

            //if we're not at max number of players
            if (_gameManager.PlayerManager.Players.Count < PlayerManager.MAX_PLAYERS)
            {
                //allow joining    
                _gameManager.PlayerManager.BeginJoining();
            }
        }

        private void _onPlayerAdded(object sender, PlayerAddedEventArgs e)
        {
            _setupNewPlayer(e.PlayerAdded);

            //once we hit max players, disable joining
            if (_gameManager.PlayerManager.Players.Count == PlayerManager.MAX_PLAYERS)
            {
                _gameManager.PlayerManager.EndJoining();
            }
        }

        private void _setupNewPlayer(PlayerInput playerInput, int teamSlot = -1)
        {
            //enable lobby controls and disable game controls
            playerInput.Lobby.Enable();
            playerInput.Game.Disable();

            NodePairing nodePairing = _makeNodePair(2, playerInput.PlayerSlot);

            //second node won't be used yet so just put it off to the side
            nodePairing.Nodes[1].transform.position = new Vector3(GameManager.RightX + 5, 0);

            ControllerSelectionState selectionState = new ControllerSelectionState()
            {
                WasJustAdded = true,
                NodePairing = nodePairing,
                TeamSlot = teamSlot
            };

            if(teamSlot != -1)
            {
                selectionState.SetIsReady(true);
            }

            _playerStates[playerInput] = selectionState;

            _updateNodePositions(selectionState, -1);
        }

        private NodePairing _makeNodePair(int numberOfNodes, int playerSlot)
        {
            List<Node> nodes = new List<Node>();

            DefaultPlayerColors playerColors = _gameManager.ColorManager.DefaultPlayerColors[playerSlot];

            for (int i = 0; i < numberOfNodes; i++)
            {
                GameObject nodeGameObject = GameObject.Instantiate(_nodePrefab);
                nodeGameObject.name = $"Node {i+1}";

                nodeGameObject.transform.SetParent(_nodeForeground, false);

                DefaultNodeColors nodeColors = playerColors.NodeColors[i];

                Node node = nodeGameObject.GetComponent<Node>();
                node.SetColors(nodeColors.InsideColor, nodeColors.OutsideColor);
                node.SetParticleColor(nodeColors.ParticleColor);

                //turn off the particle system
                node.ParticleSystem.Pause();

                nodes.Add(node);
            }

            GameObject lightningManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
            LightningManager lightningManager = lightningManagerGameObject.GetComponent<LightningManager>();

            lightningManager.Initialize(_nodeMidground);

            return new NodePairing(nodes, lightningManager);
        }

        private void _updateNodePositions(ControllerSelectionState changedState, int previousTeam)
        {
            //TODO: add explosion when we move?

            //teamSelection will only be -1 if the nodes have just been created
            if (changedState.TeamSlot == -1)
            {
                changedState.NodePairing.Nodes[0].transform.position = GetTeamPanelsCenterWorldPoint();
                return;
            }

            //TODO: adjust the -1 positions?
            if (previousTeam != -1)
            {
                ControllerSelectionState[] previousTeamStates = _playerStates.Values.Where(state => state.TeamSlot == previousTeam).ToArray();
                _updateTeamNodePositions(previousTeamStates);
            }

            ControllerSelectionState[] currentTeamStates = _playerStates.Values.Where(state => state.TeamSlot == changedState.TeamSlot).ToArray();
            _updateTeamNodePositions(currentTeamStates);
        }

        private void _updateTeamNodePositions(ControllerSelectionState[] teamStates)
        {
            if (teamStates.Length == 1)
            {
                _updateSoloTeamNodePositions(teamStates[0]);
            }
            else if (teamStates.Length > 1)
            {
                _updateMultiTeamNodePositions(teamStates);
            }
        }

        private void _updateSoloTeamNodePositions(ControllerSelectionState playerState)
        {
            Vector3[] corners = new Vector3[4];
            _teamSelectionPanels[playerState.TeamSlot].GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(corners[0]);
            Vector3 topRight = Camera.main.ScreenToWorldPoint(corners[2]);

            List<Node> nodes = playerState.NodePairing.Nodes;

            float panelWidth = topRight.x - bottomLeft.x;
            float spacing = panelWidth / (nodes.Count + 1);

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].transform.position = new Vector3(bottomLeft.x + (spacing * (i + 1)), (bottomLeft.y + topRight.y) / 2);
            }
        }

        private void _updateMultiTeamNodePositions(ControllerSelectionState[] teamStates)
        {
            Vector3[] corners = new Vector3[4];
            _teamSelectionPanels[teamStates[0].TeamSlot].GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(corners[0]);
            Vector3 topRight = Camera.main.ScreenToWorldPoint(corners[2]);

            float panelWidth = topRight.x - bottomLeft.x;
            float spacing = panelWidth / (teamStates.Length + 1);

            for (int i = 0; i < teamStates.Length; i++)
            {
                teamStates[i].NodePairing.Nodes[0].transform.position = new Vector3(bottomLeft.x + (spacing * (i + 1)), (bottomLeft.y + topRight.y) / 2);

                //TODO: have this handle more than two nodes?
                teamStates[i].NodePairing.Nodes[1].transform.position = new Vector3(GameManager.RightX + 5, 0);
            }
        }

        private Vector3 GetTeamPanelsCenterWorldPoint()
        {
            Vector3[] corners = new Vector3[4];
            int centerLeftPanelIndex = Mathf.FloorToInt((PlayerManager.MAX_PLAYERS - 1) / 2f);
            int centerRightPanelIndex = Mathf.CeilToInt((PlayerManager.MAX_PLAYERS - 1) / 2f);

            _teamSelectionPanels[centerLeftPanelIndex].GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3 bottomLeft = corners[0];

            _teamSelectionPanels[centerRightPanelIndex].GetComponent<RectTransform>().GetWorldCorners(corners);

            Vector3 topRight = corners[2];

            Vector3 screenPoint = new Vector3((bottomLeft.x + topRight.x) / 2f, (bottomLeft.y + topRight.y) / 2f);
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);

            return new Vector3(worldPoint.x, worldPoint.y, 0);
        }

        void Update()
        {
            //if the back button is pressed and there are no players (this is to prevent accidental exits)
            if (_gameManager.HandleBack() && _gameManager.PlayerManager.Players.Count == 0)
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
                PlayerInput playerInput = _gameManager.PlayerManager.Players[i];

                if (_playerStates[playerInput].WasJustAdded)
                {
                    //prevent "double tap"
                    _playerStates[playerInput].WasJustAdded = false;
                    continue;
                }

                if (playerInput.Lobby.ChangeTeamInputTriggered && !_playerStates[playerInput].IsReady)
                {
                    ControllerSelectionState playerState = _playerStates[playerInput];

                    int previousTeam = playerState.TeamSlot;
                    if(playerState.TeamSlot == -1)
                    {
                        if(playerInput.Lobby.ChangeTeamInputValue < 0)
                        {
                            playerState.TeamSlot = Mathf.FloorToInt((PlayerManager.MAX_PLAYERS - 1) / 2f);
                        }
                        else
                        {
                            playerState.TeamSlot = Mathf.CeilToInt((PlayerManager.MAX_PLAYERS - 1) / 2f); ;
                        }
                    }
                    else
                    {
                        playerState.TeamSlot += playerInput.Lobby.ChangeTeamInputValue > 0 ? 1 : -1;
                        playerState.TeamSlot = (playerState.TeamSlot + (PlayerManager.MAX_PLAYERS * 2)) % PlayerManager.MAX_PLAYERS;
                    }

                    _updateNodePositions(playerState, previousTeam);
                }

                //if a player hits the ready button
                if (playerInput.Lobby.SelectInput)
                {
                    if(_playerStates[playerInput].TeamSlot != -1)
                    {
                        //if they're already ready + everyone else is ready
                        if (_playerStates[playerInput].IsReady && _shouldStartGame())
                        {
                            _startGame();
                            return;
                        }
                        else
                        {
                            //set ready state to true
                            _playerStates[playerInput].SetIsReady(true);
                        }
                    }
                }

                //if a player hits the back button
                if (playerInput.Lobby.BackInput)
                {
                    //if the player is ready
                    if (_playerStates[playerInput].IsReady)
                    {
                        //set ready state to false
                        _playerStates[playerInput].SetIsReady(false);
                    }
                    else //player was not ready so drop
                    {
                        _removePlayer(playerInput);

                        //being joining again if we had stopped previously
                        if (!_gameManager.PlayerManager.IsJoining)
                        {
                            _gameManager.PlayerManager.BeginJoining();
                        }
                    }
                }
            }
        }
        private void _removePlayer(PlayerInput player)
        {
            ControllerSelectionState controllerSelectionState = _playerStates[player];

            foreach(Node node in controllerSelectionState.NodePairing.Nodes)
            {
                Destroy(node.gameObject);
            }

            Destroy(controllerSelectionState.NodePairing.LightningManager.gameObject);
            
            _playerStates.Remove(player);
            _gameManager.PlayerManager.RemovePlayer(player);
        }

        private bool _shouldStartGame()
        {
            if (_playerStates.Count < 2)
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

            _gameManager.GameSetupInfo.Teams = _playerStates.GroupBy(kvp => kvp.Value.TeamSlot)
                .Select(group => new Team(group.Key)
                {
                    PlayerInputs = group.Select(kvp => kvp.Key).ToList()
                })
                .ToList();

            //if there's one team and only two players
            if(_gameManager.GameSetupInfo.Teams.Count == 1 && _gameManager.GameSetupInfo.Teams[0].PlayerInputs.Count == 2)
            {
                _gameManager.LoadScene(SceneNames.GameModeSelection);
            }
            else
            {
                _gameManager.LoadScene(SceneNames.MultiplayerGameModeSelection);
            }
        }
    }
}
