using Assets.Scripts.PlayerInputs;
using Assets.Scripts.SceneManagers;
using UnityEngine;

class RegularGameService
{
    private GameManager _gameManager;
    private GameSceneManager _gameSceneManager;
    private NoHitManager _noHitManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;

    private GameInput[][] _gameInputs;
    private NodePairing[] _nodePairs;

    private IGameModeHandler _gameModeHandler;

    private int _lives;

    public RegularGameService(
        GameManager gameManager,
        GameSceneManager gameSceneManager,
        NoHitManager noHitManager,
        HitManager hitManager,
        HitSplitManager hitSplitManager,
        GameInput[][] gameInputs,
        NodePairing[] nodePairs,
        IGameModeHandler gameModeHandler,
        int lives
        )
    {
        _gameManager = gameManager;
        _gameSceneManager = gameSceneManager;
        _noHitManager = noHitManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;

        _gameInputs = gameInputs;
        _nodePairs = nodePairs;

        _gameModeHandler = gameModeHandler;

        _lives = lives;
    }

    public void Start()
    {
        _setNodeStartPositions();

        _gameSceneManager.UpdateLivesText(_lives);
    }

    public void SetScale(float scale, NodePairing[] nodePairs, ExplosionManager explosionManager)
    {
        foreach (NodePairing nodePair in nodePairs)
        {
            nodePair.SetScale(scale);
        }

        _gameManager.LightingManager.SetScale(scale);
        explosionManager.SetScale(scale);
        _gameSceneManager.CameraShake.Scale = scale;
    }

    public void Run(bool isPaused, float deltaTime)
    {
        _runGameEntityManager(_noHitManager, isPaused, deltaTime);
        _runGameEntityManager(_hitManager, isPaused, deltaTime);
        _runGameEntityManager(_hitSplitManager, isPaused, deltaTime);

        if(isPaused)
        {
            return;
        }

        for (int i = 0; i < _gameInputs.Length; i++)
        {
            _runInput(_gameInputs[i], _nodePairs[i], deltaTime);
        }
    }

    private void _runGameEntityManager<T>(IGameEntityManager<T> gameEntityManager, bool isPaused, float deltaTime) where T : IGameEntity
    {
        for (int i = gameEntityManager.ActiveGameEntities.Count - 1; i >= 0; i--)
        {
            T gameEntity = gameEntityManager.ActiveGameEntities[i];

            if (!isPaused)
            {
                gameEntity.Move(deltaTime);
            }

            _gameManager.LightingManager.SetLightPosition(gameEntity.LightIndex, gameEntity.Transform.position);

            if (!isPaused)
            {
                if (gameEntity.Transform.position.x < GameManager.LeftX - gameEntity.Radius)
                {
                    if (!gameEntity.IsOffScreenLeft)
                    {
                        gameEntity.IsOffScreenLeft = true;
                        OnGameEntityOffScreen(gameEntity);
                    }

                    if (gameEntity.Transform.position.x < GameManager.LeftX - 2.5f)
                    {
                        gameEntityManager.DeactivateGameEntity(i);
                    }
                }
            }
        }
    }

    private void _runInput(GameInput[] gameInputs, NodePairing nodePair, float deltaTime)
    {
        //one input is 1 player for node pairing
        if (gameInputs.Length == 1)
        {
            GameInput gameInput = gameInputs[0];

            for (int i = 0; i < nodePair.Nodes.Count; i++)
            {
                float boostValue = Mathf.Max(1, gameInput.BoostInput(i) * 1.5f);
                Vector2 nodeVelocity = gameInput.MoveInput(i) * nodePair.Nodes[i].Scale * boostValue;

                _updateNode(nodeVelocity, nodePair.Nodes[i], deltaTime);

                //int nextNodeInLoop = ((i + 1) + (nodePair.Nodes.Count * 2)) % nodePair.Nodes.Count; //this would only be used if we want circular connections

                if (i < nodePair.Nodes.Count - 1 && nodePair.LaserManagers != null)
                {
                    nodePair.LaserManagers[i].Run(nodePair.Nodes[i].transform.position, nodePair.Nodes[i + 1].transform.position);
                }
            }
        }
        else
        {
            for(int i = 0; i < gameInputs.Length; i++)
            {
                GameInput gameInput = gameInputs[i];
                float boostValue = Mathf.Max(1, gameInput.BoostInput((int)BoostInputEnum.BoostInputCombined) * 1.5f);
                Vector2 nodeVelocity = gameInput.MoveInput((int)MoveInputEnum.MoveInputCombined) * nodePair.Nodes[i].Scale * boostValue;

                _updateNode(nodeVelocity, nodePair.Nodes[i], deltaTime);

                //int nextNodeInLoop = ((i + 1) + (nodePair.Nodes.Count * 2)) % nodePair.Nodes.Count; //this would only be used if we want circular connections

                if (i < nodePair.Nodes.Count - 1 && nodePair.LaserManagers != null)
                {
                    nodePair.LaserManagers[i].Run(nodePair.Nodes[i].transform.position, nodePair.Nodes[i + 1].transform.position);
                }
            }
        }
    }

    private void _updateNode(Vector2 nodeVelocity, Node node, float deltaTime)
    {
        node.transform.position += _gameManager.GameDifficultyManager.NodeSpeed * new Vector3(nodeVelocity.x, nodeVelocity.y, 0) * Time.deltaTime;

        _gameManager.ClampObjectIntoView(node.transform);

        _gameManager.Grid.Logic.ApplyDirectedForce(nodeVelocity.normalized * 6f * deltaTime * (1 + node.Scale), node.transform.position, .5f * node.Scale);

        //_gameManager.Grid.Logic.ApplyImplosiveForce(1 * node.Scale, node.transform.position, 1 * node.Scale);
    }

    private void OnGameEntityOffScreen(IGameEntity gameEntity)
    {
        _gameModeHandler.OnGameEntityOffScreen(gameEntity);
    }

    public void OnHitCollision(Hit hit, Collider2D other)
    {
        if (other.tag != "Node")
        {
            return;
        }

        Node node = other.GetComponent<Node>();

        //if the appropriate node was hit
        if (hit.TeamId == node.TeamId && node.NodeId == hit.NodeId)
        {
            _gameManager.PlayExplosionSound(hit.ExplosionPitch);

            _gameSceneManager.AddToScore(hit.transform.position, node);
            _gameSceneManager.AddExplosion(hit.transform.position, _getExplosionColor(hit.TeamId, hit.NodeId));
            _hitManager.DeactivateGameEntity(hit);
            _gameSceneManager.Shake();

            switch (hit.NodeId)
            {
                case 0:
                    _gameManager.Hit1HitCount++;
                    break;
                case 1:
                    _gameManager.Hit2HitCount++;
                    break;
            }
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hit.Scale, hit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hit.Scale);
        }
        else //hit the wrong node
        {
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hit.Scale, hit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hit.Scale);
            SubtractLife(ReasonForGameEndEnum.Mismatch, hit.transform.position);
        }
    }

    public void OnHitSplitCollision(HitSplit hitSplit, Collider2D other)
    {
        if (other.tag != "Node")
        {
            return;
        }

        Node node = other.GetComponent<Node>();

        //if we haven't hit the first node yet
        if (!hitSplit.WasHitOnce)
        {
            _onHitSplitFirstHit(hitSplit, node);
        }
        else if (!hitSplit.WasHitTwice) //if we haven't hit the second node yet
        {
            _onHitSplitSecondHit(hitSplit, node);
        }
    }

    private void _onHitSplitFirstHit(HitSplit hitSplit, Node node)
    {
        //if the appropriate node was hit
        if (hitSplit.FirstHitTeamId == node.TeamId && node.NodeId == hitSplit.HitSplitFirstType)
        {
            _gameManager.PlayExplosionSound(hitSplit.ExplosionPitch);

            switch (hitSplit.HitSplitFirstType)
            {
                case 0:
                    _gameManager.HitSplit1HitCount++;
                    break;
                case 1:
                    _gameManager.HitSplit2HitCount++;
                    break;
            }

            hitSplit.WasHitOnce = true;
            _gameSceneManager.AddToScore(hitSplit.transform.position, node);
            _gameSceneManager.AddExplosion(hitSplit.transform.position, _getExplosionColor(hitSplit.FirstHitTeamId, hitSplit.HitSplitFirstType));
            _gameSceneManager.Shake();

            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);

            hitSplit.SetColors(
                outsideColor: hitSplit.InsideColor
                );

            _gameManager.LightingManager.SetLightColor(hitSplit.LightIndex, hitSplit.OutsideColor);
        }
        else //hit the wrong node first
        {
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);
            SubtractLife(ReasonForGameEndEnum.Mismatch, hitSplit.transform.position);
        }
    }

    private void _onHitSplitSecondHit(HitSplit hitSplit, Node node)
    {
        //if the appropriate second node was hit
        if (hitSplit.SecondHitTeamId == node.TeamId && node.NodeId == hitSplit.HitSplitSecondType)
        {
            _gameManager.PlayExplosionSound(hitSplit.ExplosionPitch);
            switch (hitSplit.HitSplitFirstType)
            {
                case 0:
                    _gameManager.Hit1HitCount++;
                    break;
                case 1:
                    string unlockMessage = "";
                    if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_Twisted, out unlockMessage))
                        _gameManager.NotificationManager.QueueNotification(unlockMessage);

                    _gameManager.Hit2HitCount++;
                    break;
            }

            hitSplit.WasHitTwice = true;
            _gameSceneManager.AddToScore(hitSplit.transform.position, node);
            _gameSceneManager.AddExplosion(hitSplit.transform.position, _getExplosionColor(hitSplit.SecondHitTeamId, hitSplit.HitSplitSecondType));
            _hitSplitManager.DeactivateGameEntity(hitSplit);
            _gameSceneManager.Shake();

            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);
        }
        else if(hitSplit.FirstHitTeamId == node.TeamId && node.NodeId == hitSplit.HitSplitFirstType) //it's the same node as the first hit
        {
            //DO NOTHING
        }
        else //hit the wrong node
        {
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);
            SubtractLife(ReasonForGameEndEnum.Mismatch, hitSplit.transform.position);
        }
    }

    public void OnNoHitCollision(NoHit noHit, Collider2D other)
    {
        _gameManager.NoHitHitCount++;
        _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * noHit.Scale, noHit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * noHit.Scale);

        if (other.gameObject.tag == "Connector")
        {
            SubtractLife(ReasonForGameEndEnum.NoHitContactWithLaser, noHit.transform.position);
        }
        else
        {
            //TODO: put another explosive force on the node that hit?
            SubtractLife(ReasonForGameEndEnum.NoHitContactWithNode, noHit.transform.position);
        }
    }

    public void SubtractLife(ReasonForGameEndEnum reasonForGameEnd, Vector2 position)
    {
        _lives--;

        _gameSceneManager.UpdateLivesText(_lives);

        //TODO: have a different sound depending on if we lose a life vs. lose the game
        _gameManager.SoundEffectManager.PlayGameOver();

        if (_lives <= 0)
        {
            _gameSceneManager.EndGame(reasonForGameEnd);
            _gameSceneManager.VignetteManager.StartClosePhase1(position, .1f);
        }
    }

    private void _setNodeStartPositions()
    {
        float width = (GameManager.RightXWithClamp - GameManager.LeftXWithClamp);
        float height = (GameManager.TopYWithClamp - GameManager.BotYWithClamp);

        float ySpacing = height / (_nodePairs.Length + 1);

        for (int i = 0; i < _nodePairs.Length; i++)
        {
            NodePairing nodePair = _nodePairs[i];

            for(int j = 0; j < nodePair.Nodes.Count; j++)
            {
                float xSpacing = width / (nodePair.Nodes.Count + 1);

                nodePair.Nodes[j].transform.position = new Vector3(GameManager.LeftXWithClamp + (xSpacing * (j + 1)), GameManager.BotYWithClamp + (ySpacing * (i + 1)), 0);
            }
        }
    }

    private Color _getExplosionColor(int teamId, int nodeId)
    {
        return _nodePairs[teamId].Nodes[nodeId].ParticleColor;
    }
}
