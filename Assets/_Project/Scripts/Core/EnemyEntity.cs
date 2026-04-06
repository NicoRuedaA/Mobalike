using UnityEngine;

namespace MobaGameplay.Core
{
    public class EnemyEntity : BaseEntity
    {
        [Header("Enemy Settings")]
        [SerializeField] private float maxHealth = 500f;
        private float currentHealth;

        protected override void Awake()
        {
            base.Awake();
            currentHealth = maxHealth;
        }

        // Método temporal para recibir daño hasta que implementemos un sistema de Vida completo
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            Debug.Log($"[EnemyEntity] {gameObject.name} took {amount} damage! Current HP: {currentHealth}");

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"[EnemyEntity] {gameObject.name} has died.");
            // Aquí podríamos reproducir animación, soltar oro, o destruir el objeto
            Destroy(gameObject);
        }
    }
}