using Assets.Scripts.ControllerSelection;
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
        private GameObject _playerTextPrefab;

        [SerializeField]
        private RectTransform _panelsContainer;

        [SerializeField]
        private Canvas _canvas;

        private GameManager _gameManager;

        private Dictionary<PlayerInput, ControllerSelectionState> _playerStates;

        [SerializeField]
        private Transform _nodeMidground;

        [SerializeField]
        private Transform _nodeForeground;

        private List<GameObject> _teamSelectionPanels;

        private int _frameCount;

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
        }

        private void _onSecondFrame()
        {
            //if we already had teams from a previous game
            if (_gameManager.GameSetupInfo.Teams != null)
            {
                foreach (Team team in _gameManager.GameSetupInfo.Teams)
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

            GameObject playerText = GameObject.Instantiate(_playerTextPrefab);
            playerText.transform.SetParent(_canvas.transform);

            ControllerSelectionState selectionState = new ControllerSelectionState()
            {
                WasJustAdded = true,
                NodePairing = nodePairing,
                TeamSlot = teamSlot,
                PlayerText = playerText.GetComponent<PlayerText>()
            };

            selectionState.PlayerText.PlayerNumberText.text = $"P{playerInput.PlayerSlot + 1}";

            selectionState.SetIsReady(teamSlot != -1);

            _playerStates[playerInput] = selectionState;

            _updateNodePositions(selectionState, -1);
        }

        private NodePairing _makeNodePair(int numberOfNodes, int playerSlot)
        {
            List<Node> nodes = new List<Node>();
            List<LaserManager> laserManagers = new List<LaserManager>();
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

                if(i > 0)
                {
                    GameObject laserManagerGameObject = GameObject.Instantiate(_lightningManagerPrefab);
                    LaserManager laserManager = laserManagerGameObject.GetComponent<LaserManager>();

                    laserManager.Initialize(_nodeMidground);
                    laserManager.SetLaserColor(nodes[i - 1].OutsideColor, node.OutsideColor);
                    laserManagers.Add(laserManager);
                }
            }

            return new NodePairing(nodes, laserManagers);
        }

        private void _updateNodePositions(ControllerSelectionState changedState, int previousTeamSlot)
        {
            //TODO: add explosion when we move?

            //show arrows if we haven't selected a team
            changedState.PlayerText.Arrows.gameObject.SetActive(changedState.TeamSlot == -1);

            //show ready text if we've selected a team
            changedState.PlayerText.ReadyText.gameObject.SetActive(changedState.TeamSlot != -1);

            ControllerSelectionState[] currentTeamStates = _getTeamStates(changedState.TeamSlot);
            
            //TeamSlot will only be -1 if the nodes have just been created
            if (changedState.TeamSlot == -1)
            {
                _updateNoTeamNodePositions(currentTeamStates);
                return;
            }

            ControllerSelectionState[] previousTeamStates = _getTeamStates(previousTeamSlot);
            if (previousTeamSlot == -1)
            {
                _updateNoTeamNodePositions(previousTeamStates);
            }
            else
            {
                _updateTeamNodePositions(previousTeamStates);
            }

            //update the current team
            _updateTeamNodePositions(currentTeamStates);
        }

        private void _updateNoTeamNodePositions(ControllerSelectionState[] teamStates)
        {
            Vector3[] corners = new Vector3[4];
            _teamSelectionPanels[0].GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(corners[0]);
            Vector3 topRight = Camera.main.ScreenToWorldPoint(corners[2]);

            Vector3 centerPoint = GetTeamPanelsCenterWorldPoint();

            float panelHeight = topRight.y - bottomLeft.y;
            float spacing = panelHeight / (teamStates.Length + 1);

            for (int i = 0; i < teamStates.Length; i++)
            {
                ControllerSelectionState playerState = teamStates[i];
                playerState.NodePairing.Nodes[0].transform.position = new Vector3(centerPoint.x, topRight.y - (spacing * (i + 1)));

                Vector3 nodeScreenPosition = Camera.main.WorldToScreenPoint(playerState.NodePairing.Nodes[0].transform.position);
                Vector3 readyTextPosition = new Vector3(nodeScreenPosition.x, nodeScreenPosition.y, 0);
                playerState.PlayerText.transform.position = readyTextPosition;
                playerState.PlayerText.ReadyText.transform.localPosition = new Vector3(0, -100, 0);

                //move the second node off the screen
                teamStates[i].NodePairing.Nodes[1].transform.position = new Vector3(GameManager.RightX + 5, 0);
            }
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

            playerState.NodePairing.LaserManagers[0].gameObject.SetActive(true);

            List<Node> nodes = playerState.NodePairing.Nodes;

            float panelWidth = topRight.x - bottomLeft.x;
            float spacing = panelWidth / (nodes.Count + 1);

            Vector3 nodesAveragePosition = Vector3.zero;
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].transform.position = new Vector3(bottomLeft.x + (spacing * (i + 1)), (bottomLeft.y + topRight.y) / 2);
                nodesAveragePosition += nodes[i].transform.position;

                if(i > 0)
                {
                    playerState.NodePairing.LaserManagers[i - 1].SetLaserColor(nodes[i - 1].OutsideColor, nodes[i].OutsideColor);
                    playerState.NodePairing.LaserManagers[i - 1].Run(nodes[i - 1].transform.position, nodes[i].transform.position);
                }
            }
            nodesAveragePosition /= nodes.Count;

            Vector3 nodeScreenPosition = Camera.main.WorldToScreenPoint(nodesAveragePosition);
            Vector3 readyTextPosition = new Vector3(nodeScreenPosition.x, nodeScreenPosition.y, 0);
            playerState.PlayerText.transform.position = readyTextPosition;
            playerState.PlayerText.ReadyText.transform.localPosition = new Vector3(0, -110, 0);
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
                ControllerSelectionState playerState = teamStates[i];
                playerState.NodePairing.Nodes[0].transform.position = new Vector3(bottomLeft.x + (spacing * (i + 1)), (bottomLeft.y + topRight.y) / 2);

                Vector3 nodeScreenPosition = Camera.main.WorldToScreenPoint(playerState.NodePairing.Nodes[0].transform.position);
                Vector3 readyTextPosition = new Vector3(nodeScreenPosition.x, nodeScreenPosition.y, 0);
                playerState.PlayerText.transform.position = readyTextPosition;
                playerState.PlayerText.ReadyText.transform.localPosition = new Vector3(0, -100, 0);

                if (i > 0)
                {
                    LaserManager laserManager = teamStates[i - 1].NodePairing.LaserManagers[0];
                    laserManager.SetLaserColor(teamStates[i - 1].NodePairing.Nodes[0].OutsideColor, playerState.NodePairing.Nodes[0].OutsideColor);
                    laserManager.Run(teamStates[i - 1].NodePairing.Nodes[0].transform.position, playerState.NodePairing.Nodes[0].transform.position);
                    laserManager.gameObject.SetActive(true);
                }

                //move the second node off the screen
                teamStates[i].NodePairing.Nodes[1].transform.position = new Vector3(GameManager.RightX + 5, 0);
            }

            teamStates[teamStates.Length - 1].NodePairing.LaserManagers[0].gameObject.SetActive(false);
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
            if(_frameCount < 2)
            {
                _frameCount++;
                if(_frameCount == 2)
                {
                    _onSecondFrame();
                }
                else
                {
                    return;
                }
            }

            //if the back button is pressed and there are no players (this is to prevent accidental exits)
            if (_gameManager.HandleBack() && _gameManager.PlayerManager.Players.Count == 0)
            {
                _onExitScene();
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
                        playerState.TeamSlot = (playerState.TeamSlot + PlayerManager.MAX_PLAYERS) % PlayerManager.MAX_PLAYERS;
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

            //TODO: should probably reuse this stuff instead of destroying it
            foreach (Node node in controllerSelectionState.NodePairing.Nodes)
            {
                Destroy(node.gameObject);
            }

            foreach(LaserManager laserManager in controllerSelectionState.NodePairing.LaserManagers)
            {
                Destroy(laserManager.gameObject);
            }

            Destroy(controllerSelectionState.PlayerText.gameObject);
            
            _playerStates.Remove(player);

            _gameManager.PlayerManager.RemovePlayer(player);

            //update positions of the team we were previously on
            ControllerSelectionState[] previousTeamStates = _getTeamStates(controllerSelectionState.TeamSlot);

            if(controllerSelectionState.TeamSlot == -1)
            {
                _updateNoTeamNodePositions(previousTeamStates);
            }
            else
            {
                _updateTeamNodePositions(previousTeamStates);
            }
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
                    PlayerInputs = group.Select(kvp => kvp.Key).OrderBy(_orderBy).ToList()
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

        private int _orderBy(PlayerInput playerInput)
        {
            return playerInput.PlayerSlot;
        }
    
        private ControllerSelectionState[] _getTeamStates(int teamSlot)
        {
            return _playerStates.OrderBy(kvp => _orderBy(kvp.Key))
                .Where(kvp => kvp.Value.TeamSlot == teamSlot)
                .Select(kvp => kvp.Value)
                .ToArray();
        }
    }
}
