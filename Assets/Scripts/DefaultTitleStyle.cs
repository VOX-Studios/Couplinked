using UnityEngine;
using UnityEngine.UI;

class DefaultTitleStyle : MonoBehaviour
{

    private void Start()
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Text defaultTitleSettings = gameManager.DefaultUiSettings.Title;

        Text text = GetComponent<Text>();

        text.rectTransform.anchorMin = defaultTitleSettings.rectTransform.anchorMin;
        text.rectTransform.anchorMax = defaultTitleSettings.rectTransform.anchorMax;
        text.rectTransform.pivot = defaultTitleSettings.rectTransform.pivot;
        text.rectTransform.sizeDelta = defaultTitleSettings.rectTransform.sizeDelta;
        text.rectTransform.anchoredPosition = defaultTitleSettings.rectTransform.anchoredPosition;

        text.fontSize = defaultTitleSettings.fontSize;
        text.alignment = defaultTitleSettings.alignment;
        text.resizeTextForBestFit = defaultTitleSettings.resizeTextForBestFit;
    }
}
