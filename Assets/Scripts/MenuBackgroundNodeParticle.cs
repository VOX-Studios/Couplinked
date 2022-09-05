using UnityEngine;

public class MenuBackgroundNodeParticle : MonoBehaviour
{
    private GameManager _gameManager;

    public ParticleSystem Particles;

    private Vector2 _position = new Vector2(.5f, .5f);
    private Vector2 _velocity = new Vector2(1, .5f);

    private float _minBound = 0f;
    private float _maxBound = 1f;

    public bool IsActive { get; private set; }

    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        IsActive = true;
    }

    public void Initialize(Vector2 position, Vector2 velocity, float bounds)
    {
        _position = position;
        _velocity = velocity;

        _minBound = bounds;
        _maxBound = 1 - bounds;
    }

    public void SetNodeActive(bool isActive)
    {
        IsActive = isActive;
        Particles.gameObject.SetActive(isActive);
    }

    public void Run(float deltaTime)
    {
        if(!IsActive)
        {
            return;
        }

        if (_position.x >= _maxBound)
        {
            _position.x = _maxBound;
            _velocity.x = -1f;
        }

        if (_position.y >= _maxBound)
        {
            _position.y = _maxBound;
            _velocity.y = -1f;
        }

        if (_position.x <= _minBound)
        {
            _position.x = _minBound;
            _velocity.x = 1f;
        }

        if (_position.y <= _minBound)
        {
            _position.y = _minBound;
            _velocity.y = 1f;
        }


        Vector3 worldPos = _gameManager.Cam.ViewportToWorldPoint(new Vector3(_position.x, _position.y, -Camera.main.transform.position.z));
        _gameManager.Grid.Logic.ApplyDirectedForce(_velocity.normalized * .05f, worldPos, .4f);
        Particles.transform.position = worldPos;

        _position += _velocity * deltaTime / 5;
    }
}