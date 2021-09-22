using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class DefaultButtonStyle : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    private GameManager _gameManager;

    private Text _text;

    [SerializeField]
    private bool _keepImageColor;

    [SerializeField]
    private bool _keepFontSize;

    [SerializeField]
    private bool _keepFontAlignment;


    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        Image image = GetComponent<Image>();
        Button button = GetComponent<Button>();
        Dropdown dropdown = GetComponent<Dropdown>();
        _text = GetComponentInChildren<Text>();

        image.enabled = true;
        image.sprite = _gameManager.DefaultButtonStyleTemplate.DefaultImage.sprite;

        if(!_keepImageColor)
            image.color = _gameManager.DefaultButtonStyleTemplate.DefaultImage.color;

        if (button != null)
        {
            button.transition = _gameManager.DefaultButtonStyleTemplate.DefaultButton.transition;
            button.colors = _gameManager.DefaultButtonStyleTemplate.DefaultButton.colors;
            button.targetGraphic = image;
        }
        else if(dropdown != null)
        {
            dropdown.transition = _gameManager.DefaultButtonStyleTemplate.DefaultButton.transition;
            dropdown.colors = _gameManager.DefaultButtonStyleTemplate.DefaultButton.colors;
            dropdown.targetGraphic = image;

            //have to toggle off and on for image to properly get picked up
            dropdown.image.enabled = false;
            dropdown.image.enabled = true;
        }

        _text.color = _gameManager.DefaultButtonStyleTemplate.DefaultColor;

        if(!_keepFontAlignment)
            _text.alignment = _gameManager.DefaultButtonStyleTemplate.DefaultText.alignment;

        if(!_keepFontSize)
            _text.fontSize = _gameManager.DefaultButtonStyleTemplate.DefaultText.fontSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(this.gameObject);

        _setHighlightText();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        GetComponent<Selectable>().OnPointerExit(null);

        _setDefaultText();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _setHighlightText();
    }

    private void _setDefaultText()
    {
        if (_text != null)
        {
            _text.color = _gameManager.DefaultButtonStyleTemplate.DefaultColor;
        }
    }

    private void _setHighlightText()
    {
        if (_text != null)
        {
            _text.color = _gameManager.DefaultButtonStyleTemplate.HighlightColor;
        }
    }
}
