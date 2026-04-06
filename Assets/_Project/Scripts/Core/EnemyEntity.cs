using UnityEngine;

namespace MobaGameplay.Core
{
    public class EnemyEntity : BaseEntity
    {
        protected override void Die()
        {
            base.Die();
            Debug.Log($"[EnemyEntity] {gameObject.name} has died.");
            // Aquí podríamos reproducir animación, soltar oro, o destruir el objeto
            Destroy(gameObject, 1f); // Destroy after 1 sec to let events fire
        }
    }
}