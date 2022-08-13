using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ControllerSelection
{
    [RequireComponent(typeof(Text))]
    class TextPulse : BasePulse
    {
        [SerializeField]
        private Text _text;

        protected override Color _GetBaseColor()
        {
            return _text.color;
        }

        protected override void _SetColor(Color color)
        {
            _text.color = color;
        }
    }
}
