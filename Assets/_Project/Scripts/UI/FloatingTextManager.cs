using UnityEngine;
using MobaGameplay.Combat;
using TMPro;

namespace MobaGameplay.UI
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [SerializeField] private FloatingDamageText _damageTextPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Spawn(Vector3 position, float amount, DamageType type)
        {
            if (_damageTextPrefab == null) 
            {
                Debug.LogWarning("FloatingTextManager: No DamageTextPrefab assigned!");
                return;
            }

            // Small jitter so numbers don't overlap perfectly
            Vector3 randomJitter = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.5f), Random.Range(-0.5f, 0.5f));
            
            FloatingDamageText textInstance = Instantiate(_damageTextPrefab, position + randomJitter, Quaternion.identity);
            textInstance.Setup(amount, type);
        }
    }
}