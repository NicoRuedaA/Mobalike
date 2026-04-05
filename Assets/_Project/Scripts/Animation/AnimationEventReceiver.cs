using UnityEngine;

namespace MobaGameplay.Animation
{
    /// <summary>
    /// Este script debe ir en el MISMO GameObject que tiene el componente Animator.
    /// Su única función es atrapar los eventos de animación de los StarterAssets para evitar errores.
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        public void OnFootstep(AnimationEvent animationEvent)
        {
            // Silencia el error de OnFootstep
            // if (animationEvent.animatorClipInfo.weight > 0.5f) { Debug.Log("Paso!"); }
        }

        public void OnLand(AnimationEvent animationEvent)
        {
            // Silencia el error de OnLand
        }
    }
}
