using UnityEngine;
using UnityEngine.UI;

class DefaultTextStyle : MonoBehaviour
{
    private GameManager _gameManager;

    [SerializeField]
    private bool _keepFontSize;

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Text text = GetComponent<Text>();

        text.color = _gameManager.DefaultButtonStyleTemplate.DefaultColor;
        text.alignment = _gameManager.DefaultButtonStyleTemplate.DefaultText.alignment;

        if(!_keepFontSize)
            text.fontSize = _gameManager.DefaultButtonStyleTemplate.DefaultText.fontSize;
    }
}
