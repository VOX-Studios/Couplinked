using System.Collections.Generic;
using UnityEngine;

public class LightningManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _lightningBoltPrefab;

    [SerializeField]
    private LineRenderer _connectorLine;

    [SerializeField]
    private BoxCollider2D _collider;

    private List<LineRenderer> _activeBolts;
    private List<LineRenderer> _inactiveBolts;
    private int _maxBolts = 10;

    private Color _lightningColor;

    private float _scale = 1;

    public void Initialize(Transform parent, Color lightningColor)
    {
        transform.SetParent(parent, false);
        SetLightningColor(lightningColor);

        _activeBolts = new List<LineRenderer>();
        _inactiveBolts = new List<LineRenderer>();

        for (int i = 0; i < _maxBolts; i++)
        {
            GameObject lightningBolt = GameObject.Instantiate(_lightningBoltPrefab);
            lightningBolt.transform.parent = transform;
            lightningBolt.SetActive(false);
            _inactiveBolts.Add(lightningBolt.GetComponent<LineRenderer>());
        }

        _connectorLine.positionCount = 2;
        _connectorLine.name = "Connector";
        _connectorLine.tag = "Connector";
    }

    /// <summary>
    /// This should only be called once after initialize.
    /// </summary>
    /// <param name="scale"></param>
    public void SetScale(float scale)
    {
        _scale = scale;

        _connectorLine.startWidth *= scale;
        _connectorLine.endWidth *= scale;

        _collider.size = new Vector2(_collider.size.x, _collider.size.y * scale);

        foreach (LineRenderer bolt in _inactiveBolts)
        {
            bolt.startWidth *= scale;
            bolt.endWidth *= scale;
        }
    }    

    public void SetLightningColor(Color color)
    {
        _lightningColor = color;
        _connectorLine.startColor = _lightningColor;
        _connectorLine.endColor = _lightningColor;
    }

    public void Run(Vector3 pos1, Vector3 pos2)
    {
        _connectorLine.SetPosition(0, pos1);
        _connectorLine.SetPosition(1, pos2);

        //The difference between our start and end points
        Vector2 distance = pos2 - pos1;

        _collider.size = new Vector2(distance.magnitude, .04f * _scale); //y should match width in LineRenderer
        _collider.transform.position = (pos1 + pos2) / 2;

        //The angle between our start and end points
        float angleDegrees = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
        _collider.transform.rotation = Quaternion.Euler(0, 0, angleDegrees);

        DeactivateAllBolts();

        LineRenderer bolt = ActivateBolt();

        if (bolt == null)
            return;

        LightningGenerator.SetBolt(pos1, pos2, bolt, _scale);

        bolt.startColor = _lightningColor;
        bolt.endColor = _lightningColor;
    }

    public LineRenderer ActivateBolt()
    {
        int inactiveCount = _inactiveBolts.Count;
        if (inactiveCount > 0)
        {
            LineRenderer bolt = _inactiveBolts[inactiveCount - 1];
            bolt.gameObject.SetActive(true);
            _inactiveBolts.RemoveAt(inactiveCount - 1);
            _activeBolts.Add(bolt);

            return bolt;
        }

        return null;
    }

    public void DeactivateBolt(int index)
    {
        LineRenderer bolt = _activeBolts[index];
        bolt.gameObject.SetActive(false);
        _activeBolts.RemoveAt(index);
        _inactiveBolts.Add(bolt);
    }

    public void DeactivateAllBolts()
    {
        int activeLineCount = _activeBolts.Count;
        for (int i = activeLineCount - 1; i >= 0; i--)
        {
            DeactivateBolt(i);
        }
    }
}
