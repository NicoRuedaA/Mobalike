using UnityEngine;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Maneja el modelo visual del héroe y su Animator.
    /// Se attacha al GameObject del héroe y referencia los hijos Visuals.
    /// </summary>
    public class HeroEntityVisuals : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Hijo 'Visuals' que contiene el Animator")]
        [SerializeField] private Transform visualsRoot;

        [Tooltip("Hijo 'Skeleton' o contenedor del esqueleto")]
        [SerializeField] private Transform skeletonRoot;

        [Tooltip("Hijo 'FloatingStatusBar' (Canvas)")]
        [SerializeField] private Canvas statusBarCanvas;

        private Animator animator;
        private GameObject currentModel;

        public Animator Animator => animator;

        void Awake()
        {
            // Auto-find children if not assigned
            if (visualsRoot == null)
                visualsRoot = transform.Find("Visuals");
            
            if (skeletonRoot == null)
                skeletonRoot = transform.Find("Visuals/Skeleton");
            
            if (statusBarCanvas == null)
            {
                var sb = transform.Find("FloatingStatusBar");
                if (sb != null)
                    statusBarCanvas = sb.GetComponent<Canvas>();
            }

            if (visualsRoot != null)
                animator = visualsRoot.GetComponent<Animator>();
        }

        /// <summary>
        /// Aplica el modelo de una clase (Modelo + Animator).
        /// </summary>
        public void ApplyModel(GameObject modelPrefab, RuntimeAnimatorController animatorController)
        {
            if (modelPrefab == null || visualsRoot == null)
            {
                Debug.LogWarning("[HeroEntityVisuals] Cannot apply model: null reference");
                return;
            }

            // Instantiate new model under Visuals/Skeleton
            if (skeletonRoot != null && modelPrefab.transform.childCount > 0)
            {
                // Get the first child (the actual mesh)
                GameObject newModel = Instantiate(
                    modelPrefab.transform.GetChild(0).gameObject, 
                    skeletonRoot.position, 
                    skeletonRoot.rotation, 
                    skeletonRoot
                );
                
                newModel.transform.localScale = Vector3.one;
                newModel.name = "ClassModel";

                // Destroy old model if exists
                foreach (Transform child in skeletonRoot)
                {
                    if (child.name != "Geometry" && child.name != "ClassModel")
                        Destroy(child.gameObject);
                }

                currentModel = newModel;
            }

            // Apply animator controller
            if (animator == null)
                animator = visualsRoot.GetComponent<Animator>();
            
            if (animator != null && animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }

            Debug.Log($"[HeroEntityVisuals] Model applied from {modelPrefab.name}");
        }

        /// <summary>
        /// Obtiene el animator para reproduciranimaciones.
        /// </summary>
        public Animator GetAnimator()
        {
            if (animator == null && visualsRoot != null)
                animator = visualsRoot.GetComponent<Animator>();
            return animator;
        }
    }
}