using UnityEngine;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Representa un drop de oro visual que se mueve hacia el jugador cuando está cerca.
    /// Se auto-destruye después de 3 segundos si no se recoge.
    /// </summary>
    public class GoldDrop : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Velocidad de movimiento hacia el jugador.")]
        [SerializeField] private float moveSpeed = 8f;
        
        [Tooltip("Rango de detección para empezar a moverse hacia el jugador.")]
        [SerializeField] private float pickupRange = 5f;
        
        [Tooltip("Distancia para considerar que fue recogido.")]
        [SerializeField] private float collectionDistance = 1.5f;

        [Header("Lifetime")]
        [Tooltip("Tiempo antes de auto-destruirse si no se recoge (segundos).")]
        [SerializeField] private float lifetime = 3f;

        [Header("Reward")]
        [Tooltip("Cantidad de oro que otorga al ser recogido.")]
        [SerializeField] private int goldAmount = 5;

        // Referencia al héroe (caché)
        private HeroEntity _hero;
        private float _lifetimeTimer;
        private bool _isMoving = false;

        private void Start()
        {
            _lifetimeTimer = lifetime;
            
            // Buscar el héroe en la escena
            _hero = FindObjectOfType<HeroEntity>();
            
            if (_hero == null)
            {
                Debug.LogWarning("[GoldDrop] No HeroEntity found in scene.");
            }
        }

        private void Update()
        {
            // Manejar lifetime
            _lifetimeTimer -= Time.deltaTime;
            if (_lifetimeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // Si no hay héroe, no hacer nada
            if (_hero == null) return;

            float distanceToHero = Vector3.Distance(transform.position, _hero.transform.position);

            // Si está en rango de pickup, moverse hacia el héroe
            if (distanceToHero <= pickupRange)
            {
                _isMoving = true;
            }

            if (_isMoving)
            {
                // Mover hacia el héroe
                Vector3 direction = (_hero.transform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                // Verificar si fue recogido
                if (distanceToHero <= collectionDistance)
                {
                    OnCollected();
                }
            }
        }

        /// <summary>
        /// Llamado cuando el drop es recogido por el jugador.
        /// Otorga oro al héroe y se destruye.
        /// </summary>
        private void OnCollected()
        {
            // Otorgar oro al héroe
            if (_hero != null)
            {
                _hero.AddGold(goldAmount);
                Debug.Log($"[GoldDrop] Collected by player! +{goldAmount} gold");
            }
            
            // Destruir el drop
            Destroy(gameObject);
        }
    }
}
