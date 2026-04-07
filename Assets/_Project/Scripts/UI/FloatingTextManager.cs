using UnityEngine;
using MobaGameplay.Combat;

namespace MobaGameplay.UI
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [SerializeField] private FloatingDamageText damageTextPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private float jitterRadius = 0.4f;
        [SerializeField] private float verticalOffset = 2f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Spawn(Vector3 position, float amount, DamageType type, bool isCritical = false)
        {
            if (damageTextPrefab == null) return;

            Vector3 jitter = new Vector3(
                Random.Range(-jitterRadius, jitterRadius),
                Random.Range(0f, jitterRadius * 0.5f),
                Random.Range(-jitterRadius, jitterRadius)
            );
            
            Vector3 spawnPos = position + Vector3.up * verticalOffset + jitter;
            FloatingDamageText instance = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            instance.Setup(amount, type, isCritical);
        }
    }
}
