using System;
using UnityEngine;

namespace Assets.Scripts.Lighting
{
    class VignetteManager : MonoBehaviour //TODO: rename "close" variables to reflect open/close
    {
        [SerializeField]
        private SpriteRenderer _vignetteRenderer;

        private Material _vignetteMaterial;

        private const string _HOLE_SIZE = "_Hole_Size";
        private const string _POSITION = "_Position";

        public bool IsClosing { get; private set; }
        public uint Phase { get; private set; }
        private float _closeDuration;
        private float _timeSpentClosing;

        private float _startSize;
        private float _size;
        private float _sizeToCloseTo;
        private Func<float, float> _easingFunction;

        public void Initialize()
        {
            _vignetteMaterial = _vignetteRenderer.material;
            _setSize(1);
        }

        public void StartClosePhase1(Vector2 worldPosition, float sizeToCloseTo)
        {
            Phase = 1;
            _closeDuration = 1f;
            _setPosition(worldPosition);
            _changeSize(sizeToCloseTo);
            _easingFunction = _easeOutBounce;
        }

        public void StartClosePhase2(float sizeToCloseTo) //TODO: set close duration and power factor here (second close is too fast)
        {
            Phase = 2;
            _closeDuration = .25f;
            _changeSize(sizeToCloseTo);
            _easingFunction = _easeOutLinear;
        }

        private void _changeSize(float sizeToChangeTo) //TODO: set close duration and power factor here (second close is too fast)
        {
            _startSize = _size;
            IsClosing = true;
            _timeSpentClosing = 0f;
            _sizeToCloseTo = sizeToChangeTo;
        }

        private void _setSize(float size)
        {
            _size = size;
            _size = Mathf.Clamp(_size, 0, 1);
            _vignetteMaterial.SetFloat(_HOLE_SIZE, _size);
        }

        private void _setPosition(Vector2 worldPosition)
        {
            Vector2 screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
            _vignetteMaterial.SetVector(_POSITION, screenPosition);
        }

        public void Run(float deltaTime)
        {
            if(IsClosing)
            {
                _timeSpentClosing += deltaTime;

                float time = Mathf.Clamp(_timeSpentClosing / _closeDuration, 0, 1);
                float sizeAdjustment = _easingFunction(time) * (_sizeToCloseTo - _startSize);

                _setSize(_startSize + sizeAdjustment);

                if(time >= 1)
                {
                    IsClosing = false;
                }    
            }
        }

        private float _easeOutLinear(float time)
        {
            return time;
        }

        private float _easeOutBounce(float time)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;

            if (time < 1 / d1)
            {
                return n1 * time * time;
            }
            else if (time < 2 / d1)
            {
                time -= 1.5f / d1;
                return n1 * time * time + 0.75f;
            }
            else if (time < 2.5 / d1)
            {
                time -= 2.25f / d1;
                return n1 * time * time + 0.9375f;
            }
            else
            {
                time -= 2.625f / d1;
                return n1 * time * time + 0.984375f;
            }
        }
    }
}
