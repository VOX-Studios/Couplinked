using UnityEngine;

public class LaserManager : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D _collider;

    [SerializeField]
    private LineRenderer _laser;

    private float _scale = 1;

    public void Initialize(Transform parent)
    {
        transform.SetParent(parent, false);
    }

    /// <summary>
    /// This should only be called once after initialize.
    /// </summary>
    /// <param name="scale"></param>
    public void SetScale(float scale)
    {
        _scale = scale;

        _laser.startWidth *= scale;

        _collider.size = new Vector2(_collider.size.x, _collider.size.y * scale);
    }

    public void SetLaserColor(Color color1, Color color2)
    {
        _laser.material.SetColor("_Color_1", color1);
        _laser.material.SetColor("_Color_2", color2);
    }

    public void SetLightTexture(RenderTexture renderTexture)
    {
        _laser.material.SetTexture("_Light_Texture", renderTexture);
    }

    public void Run(Vector3 pos1, Vector3 pos2)
    {
        _laser.material.SetVector("_Position_1", pos1);
        _laser.material.SetVector("_Position_2", pos2);
        _laser.SetPosition(0, pos1);
        _laser.SetPosition(1, pos2);

        //The difference between our start and end points
        Vector2 distance = pos2 - pos1;

        _collider.size = new Vector2(distance.magnitude, .04f * _scale); //y should match width in LineRenderer
        _collider.transform.position = (pos1 + pos2) / 2;

        //The angle between our start and end points
        float angleDegrees = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
        _collider.transform.rotation = Quaternion.Euler(0, 0, angleDegrees);
    }
}
