using UnityEngine;

namespace Assets.Scripts.ControllerSelection
{
    abstract class BasePulse : MonoBehaviour
    {
        private float _pulseSpeed = 2;

        private Color _baseColor;
        private Color _transparentColor;

        private float _currentLerp = 0;
        private bool _lerpUp = true;

        private bool _shouldPulse = true;

        protected abstract Color _GetBaseColor();
        protected abstract void _SetColor(Color color);


        private void Awake()
        {
            _baseColor = _GetBaseColor();
            _transparentColor = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0);
        }

        void Update()
        {
            if(!_shouldPulse)
            {
                return;
            }

            _SetColor(Color.Lerp(_baseColor, _transparentColor, _currentLerp));

            float lerpChange = Time.deltaTime * _pulseSpeed;

            //if we're not lerping up, change to a negative value
            if (!_lerpUp)
            {
                lerpChange = -lerpChange;
            }

            //increment lerp
            _currentLerp += lerpChange;

            //if we're at the top of the lerp, start lerping down
            if(_lerpUp && _currentLerp >= 1)
            {
                _lerpUp = false;
            }
            else if(!_lerpUp && _currentLerp <= 0) //if we're at the bottom of the lerp, start lerping up
            {
                _lerpUp = true;
            }

            //clamp 0-1
            _currentLerp = Mathf.Clamp(_currentLerp, 0, 1);
        }

        public void ShouldPulse(bool shouldPulse)
        {
            _shouldPulse = shouldPulse;
            _SetColor(_baseColor);
        }
    }
}
