using Assets.Scripts.SceneManagers;
using UnityEngine;

namespace Assets.Scripts.Instructions
{
    class InstructionsAnimationEventHandler : MonoBehaviour
    {
        [SerializeField]
        private InstructionsSceneManager _instructionsSceneManager;
        public void InstructionsTransitionCompleteEvent()
        {
            _instructionsSceneManager.InstructionsTransitionCompleteEvent();
        }
    }
}
