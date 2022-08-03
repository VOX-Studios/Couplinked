using UnityEngine;

namespace Assets.Scripts.Lighting
{
    class SideExplosion : MonoBehaviour
    {
        private const string _COLOR_1_NAME = "_Color_1";
        private const string _THICKNESS_NAME = "_Thickness";
        private const string _BASE_FLARE_NAME = "_Base_Flare";

        [SerializeField]
        private LineRenderer _lineRenderer;

        [SerializeField]
        private float _duration;

        [SerializeField]
        private AnimationCurve _xPositionCurve;

        [SerializeField]
        private AnimationCurve _thicknessCurve;

        [SerializeField]
        private AnimationCurve _baseFlareCurve;

        private float _progress;

        public void Run(float deltaTime)
        {
            _progress += deltaTime;
            _progress = Mathf.Clamp(_progress, 0, _duration);            


            Vector3 startPosition = _lineRenderer.GetPosition(0);
            float animationCurveValue = _xPositionCurve.Evaluate(_progress/_duration);
            Vector3 endPosition = startPosition;
            endPosition.x = Mathf.Lerp(startPosition.x, startPosition.x + 5, animationCurveValue);
            _lineRenderer.SetPosition(1, endPosition);
            

            float thicknessCurveValue = _thicknessCurve.Evaluate(_progress / _duration);
            thicknessCurveValue = Mathf.Clamp(thicknessCurveValue, 0, 1);

            //1-5
            thicknessCurveValue = 1 + (thicknessCurveValue * 4);
            _lineRenderer.material.SetFloat(_THICKNESS_NAME, thicknessCurveValue);


            float baseFlareCurveValue = _baseFlareCurve.Evaluate(_progress / _duration);
            baseFlareCurveValue = Mathf.Clamp(baseFlareCurveValue, 0, 1);

            //2-5
            baseFlareCurveValue = 2 + (baseFlareCurveValue * 3);
            _lineRenderer.material.SetFloat(_BASE_FLARE_NAME, baseFlareCurveValue);
        }

        public void Explode(Vector3 spawnPosition, Color color)
        {
            _progress = 0;
            _lineRenderer.material.SetColor(_COLOR_1_NAME, color);
            _lineRenderer.SetPosition(0, spawnPosition);
            _lineRenderer.SetPosition(1, spawnPosition);
        }
    }
}
