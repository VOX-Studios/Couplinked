using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    public Text FpsText;

    private float _refreshRate = 1;
    private float _refreshCooldown = 0;

    private float _previousTime = 0;

    void Update()
    {
        _refreshCooldown -= Time.deltaTime;

        //only update at a specific interval
        if (_refreshCooldown <= 0)
        {
            float newTime = Time.smoothDeltaTime;

            //if our value has changed
            if (_previousTime != newTime)
            {
                //update our text
                FpsText.text = $"FPS: {(int)(1 / Time.smoothDeltaTime)}";
                _previousTime = newTime;
            }
            
            _refreshCooldown = _refreshRate;
        }
    }
}