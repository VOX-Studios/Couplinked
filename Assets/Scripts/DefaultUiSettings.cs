using UnityEngine;
using UnityEngine.UI;

public class DefaultUiSettings : MonoBehaviour
{
    public Text Title;
    public GridLayoutGroup ButtonsContainer;

    private void Awake()
    {
        Title.gameObject.SetActive(false);
        ButtonsContainer.gameObject.SetActive(false);
    }
}