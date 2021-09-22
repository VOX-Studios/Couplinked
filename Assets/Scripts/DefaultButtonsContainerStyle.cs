using UnityEngine;
using UnityEngine.UI;

class DefaultButtonsContainerStyle : MonoBehaviour
{
    private void Start()
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        GridLayoutGroup defaultButtonsContainer = gameManager.DefaultUiSettings.ButtonsContainer;

        GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();

        gridLayoutGroup.cellSize = defaultButtonsContainer.cellSize;
        gridLayoutGroup.spacing = defaultButtonsContainer.spacing;

        gridLayoutGroup.startCorner = gridLayoutGroup.startCorner;
        gridLayoutGroup.startAxis = defaultButtonsContainer.startAxis;
        gridLayoutGroup.childAlignment = defaultButtonsContainer.childAlignment;
        gridLayoutGroup.constraint = defaultButtonsContainer.constraint;
        gridLayoutGroup.constraintCount = defaultButtonsContainer.constraintCount;


        RectTransform defaultRectTransform = gameManager.DefaultUiSettings.ButtonsContainer.GetComponent<RectTransform>();
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = defaultRectTransform.anchorMin;
        rectTransform.anchorMax = defaultRectTransform.anchorMax;
        rectTransform.pivot = defaultRectTransform.pivot;
        rectTransform.sizeDelta = defaultRectTransform.sizeDelta;
        rectTransform.anchoredPosition = defaultRectTransform.anchoredPosition;
    }
}
