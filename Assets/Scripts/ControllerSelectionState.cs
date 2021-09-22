using UnityEngine;
using UnityEngine.UI;

class ControllerSelectionState : MonoBehaviour
{
    private bool _isJoined = false;
    private bool _isReady = false;

    public bool IsJoined => _isJoined;
    public bool IsReady => _isReady;

    public bool WasJustAdded = false;

    [SerializeField]
    private Image _controllerImage;

    [SerializeField]
    private Text _text;

    private void Awake()
    {
        SetIsJoined(false);
    }


    public void SetIsJoined(bool isJoined)
    {
        //if we change joined state, set ready to false
        SetIsReady(false);

        _isJoined = isJoined;

        if(!_isJoined)
        {
            _text.rectTransform.localPosition = Vector3.zero;
            _text.fontSize = 120;
            _text.text = "WAITING\nFOR PLAYER";
            _controllerImage.enabled = false;
        }
    }

    public void SetIsReady(bool isReady)
    {
        _isReady = isReady;

        _text.rectTransform.localPosition = new Vector3(0, -128);
        _text.fontSize = 160;
        _controllerImage.enabled = true;

        if (isReady)
        {
            _text.text = "READY";
        }
        else
        {
            _text.text = "JOINED";
        }
    }
}
