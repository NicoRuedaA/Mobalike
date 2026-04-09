using UnityEngine;
using MobaGameplay.Inventory;

namespace MobaGameplay.Inventory
{
    /// <summary>
    /// Componente para items droppeados en el mundo 3D.
    /// Muestra el icono del item con animación flotante y rotación.
    /// </summary>
    public class GroundItem : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _floatAmplitude = 0.3f;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _rotationSpeed = 30f;

        [Header("Data")]
        [SerializeField] private ItemData _itemData;

        // Referencias internas
        private Vector3 _startPosition;
        private float _floatOffset;

        /// <summary>
        /// Referencia pública al ItemData asociado.
        /// </summary>
        public ItemData ItemData => _itemData;

        private void Awake()
        {
            // Cachear posición inicial para animación
            _startPosition = transform.position;
            _floatOffset = Random.Range(0f, Mathf.PI * 2f);

            // Obtener SpriteRenderer si no está asignado
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            // Asegurar que tiene el tag correcto
            gameObject.tag = "GroundItem";
        }

        private void Update()
        {
            AnimateFloating();
            AnimateRotation();
        }

        /// <summary>
        /// Inicializa el GroundItem con los datos del item.
        /// </summary>
        /// <param name="data">Datos del item a mostrar.</param>
        public void Initialize(ItemData data)
        {
            _itemData = data;

            if (_itemData != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _itemData.icon;
            }

            // Resetear posición base para animación
            _startPosition = transform.position;
        }

        /// <summary>
        /// Animación flotante senoidal en el eje Y.
        /// </summary>
        private void AnimateFloating()
        {
            float yOffset = Mathf.Sin((Time.time * _floatSpeed) + _floatOffset) * _floatAmplitude;
            transform.position = _startPosition + new Vector3(0f, yOffset, 0f);
        }

        /// <summary>
        /// Rotación lenta para mejor visibilidad.
        /// </summary>
        private void AnimateRotation()
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Establece la posición base para la animación flotante.
        /// Útil cuando se instancia el objeto.
        /// </summary>
        /// <param name="position">Nueva posición base.</param>
        public void SetBasePosition(Vector3 position)
        {
            _startPosition = position;
            transform.position = position;
        }

        private void OnValidate()
        {
            // Asegurar tag correcto en editor
            if (gameObject.tag != "GroundItem")
            {
                gameObject.tag = "GroundItem";
            }
        }
    }
}
