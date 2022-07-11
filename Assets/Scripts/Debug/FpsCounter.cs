using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    public Text FpsText;

    void Update()
    {
        FpsText.text = $"FPS: {(int)(1/Time.smoothDeltaTime)}";
    }
}