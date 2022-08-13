using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ControllerSelection
{
    [RequireComponent(typeof(Image))]
    class ImagePulse : BasePulse
    {
        [SerializeField]
        private Image _image;

        protected override Color _GetBaseColor()
        {
            return _image.color;
        }

        protected override void _SetColor(Color color)
        {
            _image.color = color;
        }
    }
}
