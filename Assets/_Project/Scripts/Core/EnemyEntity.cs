using UnityEngine;

namespace MobaGameplay.Core
{
    public class EnemyEntity : BaseEntity
    {
        protected override void Awake()
        {
            base.Awake();
            
            // Automatically add hover outline if not present
            var hoverOutline = GetComponent<MobaGameplay.UI.Targeting.HoverOutline>();
            if (hoverOutline == null)
            {
                hoverOutline = gameObject.AddComponent<MobaGameplay.UI.Targeting.HoverOutline>();
                hoverOutline.outlineColor = Color.red;
                hoverOutline.outlineWidth = 0.02f;
            }
        }

        protected override void Die()
        {
            base.Die();
            Debug.Log($"[EnemyEntity] {gameObject.name} has died.");
            // Aquí podríamos reproducir animación, soltar oro, o destruir el objeto
            Destroy(gameObject, 1f); // Destroy after 1 sec to let events fire
        }
    }
}