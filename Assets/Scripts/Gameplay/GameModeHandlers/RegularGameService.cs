using Assets.Scripts.SceneManagers;
using UnityEngine;

class RegularGameService
{
    private GameManager _gameManager;
    private GameSceneManager _gameSceneManager;
    private HitManager _hitManager;
    private HitSplitManager _hitSplitManager;

    private GameInput[] _gameInputs;
    private NodePair[] _nodePairs;

    private TeamManager _teamManager;

    public RegularGameService(
        GameManager gameManager,
        GameSceneManager gameSceneManager,
        HitManager hitManager,
        HitSplitManager hitSplitManager,
        GameInput[] gameInputs,
        NodePair[] nodePairs,
        TeamManager teamManager
        )
    {
        _gameManager = gameManager;
        _gameSceneManager = gameSceneManager;
        _hitManager = hitManager;
        _hitSplitManager = hitSplitManager;

        _gameInputs = gameInputs;
        _nodePairs = nodePairs;

        _teamManager = teamManager;
    }

    public void Start()
    {
        _setNodeStartPositions();
    }

    public void RunInput(float deltaTime)
    {
        for (int i = 0; i < _gameInputs.Length; i++)
        {
            _runInput(_gameInputs[i], _nodePairs[i], deltaTime);
        }
    }

    private void _runInput(GameInput gameInput, NodePair nodePair, float deltaTime)
    {
        Vector2 node1Velocity = gameInput.Move1Input;
        Vector2 node2Velocity = gameInput.Move2Input;

        nodePair.Node1.transform.position += _gameManager.GameDifficultyManager.NodeSpeed * new Vector3(node1Velocity.x, node1Velocity.y, 0) * Time.deltaTime;
        nodePair.Node2.transform.position += _gameManager.GameDifficultyManager.NodeSpeed * new Vector3(node2Velocity.x, node2Velocity.y, 0) * Time.deltaTime;

        _gameManager.ClampObjectIntoView(nodePair.Node1.transform, .5f, 2.5f); //.4 for node radius
        _gameManager.ClampObjectIntoView(nodePair.Node2.transform, .5f, 2.5f); //.4 for node radius

        _gameManager.Grid.Logic.ApplyDirectedForce(node1Velocity.normalized * 6f * deltaTime * (1 + nodePair.Node1.Scale), nodePair.Node1.transform.position, .5f * nodePair.Node1.Scale);
        _gameManager.Grid.Logic.ApplyDirectedForce(node2Velocity.normalized * 6f * deltaTime * (1 + nodePair.Node2.Scale), nodePair.Node2.transform.position, .5f * nodePair.Node2.Scale);

        nodePair.LightningManager?.Run(nodePair.Node1.transform.position, nodePair.Node2.transform.position);
    }

    public void OnHitCollision(Hit hit, Collider2D other)
    {
        if (other.tag != "Node")
            return;

        Node node = other.GetComponent<Node>();

        bool isOnTeam = _teamManager.IsOnTeam(hit.TeamId, node.TeamId);

        //if the appropriate node was hit
        if (isOnTeam && node.HitType == hit.HitType)
        {
            _gameManager.PlayExplosionSound(hit.ExplosionPitch);

            _gameSceneManager.AddToScore(hit.transform.position, node);
            _gameSceneManager.AddExplosion(hit.transform.position, _getExplosionColor(hit.HitType, hit.TeamId));
            _hitManager.DeactivateHit(hit.gameObject);
            _gameSceneManager.Shake();

            switch (hit.HitType)
            {
                case HitTypeEnum.Hit1:
                    _gameManager.Hit1HitCount++;
                    break;
                case HitTypeEnum.Hit2:
                    _gameManager.Hit2HitCount++;
                    break;
            }
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hit.Scale, hit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hit.Scale);
        }
        else //hit the wrong node
        {
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hit.Scale, hit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hit.Scale);
            _endGame(ReasonForGameEndEnum.Mismatch);
        }
    }

    public void OnHitSplitCollision(HitSplit hitSplit, Collider2D other)
    {
        if (other.tag != "Node")
            return;

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
        bool isOnTeam = _teamManager.IsOnTeam(hitSplit.FirstHitTeamId, node.TeamId);

        //if the appropriate node was hit
        if (isOnTeam && node.HitType == hitSplit.HitSplitFirstType)
        {
            _gameManager.PlayExplosionSound(hitSplit.ExplosionPitch);

            switch (hitSplit.HitSplitFirstType)
            {
                case HitTypeEnum.Hit1:
                    _gameManager.HitSplit1HitCount++;
                    break;
                case HitTypeEnum.Hit2:
                    _gameManager.HitSplit2HitCount++;
                    break;
            }

            hitSplit.WasHitOnce = true;
            _gameSceneManager.AddToScore(hitSplit.transform.position, node);
            _gameSceneManager.AddExplosion(hitSplit.transform.position, _getExplosionColor(hitSplit.HitSplitFirstType, hitSplit.FirstHitTeamId));
            _gameSceneManager.Shake();

            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);

            hitSplit.SetColors(_getHitSplitColor(hitSplit.HitSplitSecondType, hitSplit.SecondHitTeamId));
        }
        else //hit the wrong node first
        {
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);
            _endGame(ReasonForGameEndEnum.Mismatch);
        }
    }

    private void _onHitSplitSecondHit(HitSplit hitSplit, Node node)
    {
        bool isOnTeam = _teamManager.IsOnTeam(hitSplit.SecondHitTeamId, node.TeamId);
        bool isNodeOnFirstHitTeam = _teamManager.IsOnTeam(hitSplit.FirstHitTeamId, node.TeamId);

        //if the appropriate second node was hit
        if (isOnTeam && node.HitType == hitSplit.HitSplitSecondType)
        {
            _gameManager.PlayExplosionSound(hitSplit.ExplosionPitch);
            switch (hitSplit.HitSplitFirstType)
            {
                case HitTypeEnum.Hit1:
                    _gameManager.Hit1HitCount++;
                    break;
                case HitTypeEnum.Hit2:
                    string unlockMessage = "";
                    if (_gameManager.Challenges.HandleUnlockingChallenge(Challenges.ID_Twisted, out unlockMessage))
                        _gameManager.NotificationManager.QueueNotification(unlockMessage);

                    _gameManager.Hit2HitCount++;
                    break;
            }

            hitSplit.WasHitTwice = true;
            _gameSceneManager.AddToScore(hitSplit.transform.position, node);
            _gameSceneManager.AddExplosion(hitSplit.transform.position, _getExplosionColor(hitSplit.HitSplitSecondType, hitSplit.SecondHitTeamId));
            _hitSplitManager.DeactivateHitSplit(hitSplit.gameObject);
            _gameSceneManager.Shake();

            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);
        }
        else if(isNodeOnFirstHitTeam && node.HitType == hitSplit.HitSplitFirstType) //it's the same node as the first hit
        {
            //DO NOTHING
        }
        else //hit the wrong node
        {
            _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * hitSplit.Scale, hitSplit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * hitSplit.Scale);
            _endGame(ReasonForGameEndEnum.Mismatch);
        }
    }

    public void OnNoHitCollision(NoHit noHit, Collider2D other)
    {
        _gameManager.NoHitHitCount++;
        _gameManager.Grid.Logic.ApplyExplosiveForce(GameplayUtility.EXPLOSIVE_FORCE * noHit.Scale, noHit.transform.position, GameplayUtility.EXPLOSIVE_RADIUS * noHit.Scale);

        if (other.gameObject.tag == "Connector")
        {
            _endGame(ReasonForGameEndEnum.NoHitContactWithElectricity);
        }
        else
        {
            //TODO: put another pulse on the node that hit?
            _endGame(ReasonForGameEndEnum.NoHitContactWithNode);
        }
    }

    private void _endGame(ReasonForGameEndEnum reasonForGameEnd)
    {
        _gameSceneManager.EndGame(reasonForGameEnd);
    }

    private void _setNodeStartPositions()
    {
        for (int i = 0; i < _nodePairs.Length; i++)
        {
            NodePair nodePair = _nodePairs[i];
            nodePair.Node1.transform.position = new Vector3(-3, 0, 0);
            nodePair.Node2.transform.position = new Vector3(3, 0, 0);
        }
    }

    private Color _getHitSplitColor(HitTypeEnum hitType, int teamId)
    {
        switch (hitType)
        {
            default:
            case HitTypeEnum.Hit1:
                return _gameManager.DataManager.PlayerColors[teamId].Node1OutsideColor.Get(); //TODO: don't do this per spawn
            case HitTypeEnum.Hit2:
                return _gameManager.DataManager.PlayerColors[teamId].Node2OutsideColor.Get(); //TODO: don't do this per spawn
        }
    }

    private Color _getExplosionColor(HitTypeEnum hitType, int teamId)
    {
        switch (hitType)
        {
            default:
            case HitTypeEnum.Hit1:
                return _gameManager.ColorManager.NodeColors[teamId].ParticleColor1; //TODO: update this to use DataManager
            case HitTypeEnum.Hit2:
                return _gameManager.ColorManager.NodeColors[teamId].ParticleColor2; //TODO: update this to use DataManager
        }
    }
}
