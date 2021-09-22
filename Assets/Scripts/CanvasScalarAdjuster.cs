using UnityEngine;
using UnityEngine.UI;

class CanvasScalarAdjuster : MonoBehaviour
{
    private void Start()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();

        float matchWidthOrHeight = 0;
        if (Screen.width > Screen.height)
        {
            matchWidthOrHeight = 1;
        }

        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
    }
}
