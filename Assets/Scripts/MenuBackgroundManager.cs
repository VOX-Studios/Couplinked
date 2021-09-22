using UnityEngine;

public class MenuBackgroundManager : MonoBehaviour
{
    [SerializeField]
    private MenuBackgroundNodeParticle _nodeParticles1;

    [SerializeField]
    private MenuBackgroundNodeParticle _nodeParticles2;

    void Start()
    {
        _nodeParticles1.Initialize(
            position: new Vector2(.5f, .5f),
            velocity: new Vector2(1, .5f),
            bounds: 0
            );

        _nodeParticles2.Initialize(
            position: new Vector2(.5f, .5f),
            velocity: new Vector2(-1, -.5f),
            bounds: 0
            );
    }

    void Update()
    {
        _nodeParticles1.Run(Time.deltaTime);
        _nodeParticles2.Run(Time.deltaTime);
    }
}