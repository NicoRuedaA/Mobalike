using UnityEngine;
using UnityEngine.UI;
using MobaGameplay.Core;

namespace MobaGameplay.UI
{
    public class FloatingStatusBar : MonoBehaviour
    {
        [SerializeField] private BaseEntity targetEntity;
        
        [Header("UI References")]
        [SerializeField] private Image healthFill; // Current health (front)
        [SerializeField] private Image healthRecentDamageFill; // Delayed damage chip (middle)
        [SerializeField] private Image healthBackground; // Missing health (back)
        [SerializeField] private Image manaFill;

        [Header("LoL-like Health Bar")]
        [SerializeField] private float damageDelay = 0.16f;
        [SerializeField] private float damageLerpDuration = 0.35f;
        [SerializeField] private float healLerpDuration = 0.12f;

        [Header("Health Thresholds")]
        [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.3f;
        [SerializeField, Range(0f, 1f)] private float mediumThreshold = 0.6f;

        [Header("Health Colors")]
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color mediumHealthColor = new Color(0.93f, 0.78f, 0.2f, 1f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color recentDamageColor = new Color(0.95f, 0.15f, 0.15f, 0.95f);
        [SerializeField] private Color missingHealthColor = new Color(0.33f, 0.08f, 0.08f, 0.95f);

        [Header("Tick Marks")]
        [SerializeField] private bool showTickMarks = true;
        [SerializeField] private Color tickColor = new Color(0, 0, 0, 0.7f);
        [SerializeField, Range(0.01f, 0.5f)] private float tickInterval = 0.1f;
        [SerializeField, Range(0.001f, 0.05f)] private float tickWidth = 0.005f;

        private const float Epsilon = 0.0001f;

        private float currentHealthRatio;
        private float recentDamageRatio;
        private float delayedDamageStartTime;
        private bool isInitialized;

        private Vector2 healthFillBaseAnchorMin;
        private Vector2 healthFillBaseAnchorMax;
        private Vector2 healthRecentBaseAnchorMin;
        private Vector2 healthRecentBaseAnchorMax;
        private Vector2 manaFillBaseAnchorMin;
        private Vector2 manaFillBaseAnchorMax;

        private void Awake()
        {
            ResolveReferences(true);
            ConfigureVisualSetup();
            CacheBaseAnchors();
        }

        private void OnEnable()
        {
            ResolveReferences(true);
            ConfigureVisualSetup();
            CacheBaseAnchors();

            if (targetEntity != null)
            {
                targetEntity.OnTakeDamage += HandleTakeDamage;
                targetEntity.OnManaChanged += HandleManaChanged;
            }

            ForceSyncToTarget();
        }

        private void OnDisable()
        {
            if (targetEntity != null)
            {
                targetEntity.OnTakeDamage -= HandleTakeDamage;
                targetEntity.OnManaChanged -= HandleManaChanged;
            }
        }

        private void OnValidate()
        {
            if (mediumThreshold < criticalThreshold)
            {
                mediumThreshold = criticalThreshold;
            }

            if (damageDelay < 0f) damageDelay = 0f;
            if (damageLerpDuration < 0f) damageLerpDuration = 0f;
            if (healLerpDuration < 0f) healLerpDuration = 0f;

            if (!Application.isPlaying)
            {
                // IMPORTANT: never create/reparent objects in OnValidate.
                ResolveReferences(false);
            }

            ValidateSetupWarnings();
        }

        private void Update()
        {
            if (targetEntity == null)
            {
                return;
            }

            float targetRatio = GetTargetHealthRatio();

            if (!isInitialized)
            {
                currentHealthRatio = targetRatio;
                recentDamageRatio = targetRatio;
                isInitialized = true;
            }

            if (targetRatio < currentHealthRatio - Epsilon)
            {
                currentHealthRatio = targetRatio;
                delayedDamageStartTime = Time.time + damageDelay;
            }
            else if (targetRatio > currentHealthRatio + Epsilon)
            {
                float healSpeed = healLerpDuration > 0f ? (1f / healLerpDuration) : 1000f;
                currentHealthRatio = Mathf.MoveTowards(currentHealthRatio, targetRatio, healSpeed * Time.deltaTime);
                recentDamageRatio = Mathf.MoveTowards(recentDamageRatio, currentHealthRatio, healSpeed * Time.deltaTime);
            }

            if (recentDamageRatio > currentHealthRatio + Epsilon && Time.time >= delayedDamageStartTime)
            {
                float damageChipSpeed = damageLerpDuration > 0f ? (1f / damageLerpDuration) : 1000f;
                recentDamageRatio = Mathf.MoveTowards(recentDamageRatio, currentHealthRatio, damageChipSpeed * Time.deltaTime);
            }

            ApplyBars();
        }

        private void HandleTakeDamage(MobaGameplay.Combat.DamageInfo _)
        {
            if (targetEntity == null)
            {
                return;
            }

            float targetRatio = GetTargetHealthRatio();

            // Front bar drops immediately; delayed chip follows later.
            if (targetRatio < currentHealthRatio - Epsilon)
            {
                currentHealthRatio = targetRatio;
                delayedDamageStartTime = Time.time + damageDelay;
            }

            if (!isInitialized)
            {
                recentDamageRatio = targetRatio;
                isInitialized = true;
            }

            ApplyBars();
        }

        private void HandleManaChanged(float oldMana, float newMana)
        {
            ApplyBars();
        }

        private void ApplyBars()
        {
            if (targetEntity == null)
            {
                return;
            }

            currentHealthRatio = Mathf.Clamp01(currentHealthRatio);
            recentDamageRatio = Mathf.Clamp01(Mathf.Max(currentHealthRatio, recentDamageRatio));

            if (healthFill != null)
            {
                SetHorizontalRatio(healthFill, currentHealthRatio, healthFillBaseAnchorMin, healthFillBaseAnchorMax);
                healthFill.color = GetHealthColor(currentHealthRatio);
            }

            if (healthRecentDamageFill != null)
            {
                SetHorizontalRatio(healthRecentDamageFill, recentDamageRatio, healthRecentBaseAnchorMin, healthRecentBaseAnchorMax);
                healthRecentDamageFill.color = recentDamageColor;
            }

            if (healthBackground != null)
            {
                healthBackground.color = missingHealthColor;
            }

            if (manaFill != null && targetEntity.MaxMana > 0f)
            {
                float manaRatio = Mathf.Clamp01(targetEntity.CurrentMana / targetEntity.MaxMana);
                SetHorizontalRatio(manaFill, manaRatio, manaFillBaseAnchorMin, manaFillBaseAnchorMax);
            }
        }

        private void ForceSyncToTarget()
        {
            if (targetEntity == null)
            {
                return;
            }

            float ratio = GetTargetHealthRatio();
            currentHealthRatio = ratio;
            recentDamageRatio = ratio;
            delayedDamageStartTime = Time.time;
            isInitialized = true;

            ApplyBars();
        }

        private float GetTargetHealthRatio()
        {
            if (targetEntity == null || targetEntity.MaxHealth <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(targetEntity.CurrentHealth / targetEntity.MaxHealth);
        }

        private Color GetHealthColor(float ratio)
        {
            return healthyColor;
        }

        private void ResolveReferences(bool allowStructuralChanges)
        {
            if (targetEntity == null)
            {
                targetEntity = GetComponentInParent<BaseEntity>(true);
            }

            if (healthFill == null)
            {
                Transform health = transform.Find("Background/HealthFill");
                if (health != null)
                {
                    healthFill = health.GetComponent<Image>();
                }

                if (healthFill == null)
                {
                    healthFill = FindFillByName("health", "hp");
                }
            }

            if (manaFill == null)
            {
                Transform mana = transform.Find("Background/ManaFill");
                if (mana != null)
                {
                    manaFill = mana.GetComponent<Image>();
                }

                if (manaFill == null)
                {
                    manaFill = FindFillByName("mana", "mp");
                }
            }

            if (healthFill != null)
            {
                if (healthBackground == null)
                {
                    Transform explicitBackground = transform.Find("Background/HealthBackground");
                    if (explicitBackground != null)
                    {
                        healthBackground = explicitBackground.GetComponent<Image>();
                    }
                }

                if (healthRecentDamageFill == null)
                {
                    Transform explicitRecent = transform.Find("Background/HealthRecentDamageFill");
                    if (explicitRecent != null)
                    {
                        healthRecentDamageFill = explicitRecent.GetComponent<Image>();
                    }
                }

                if (allowStructuralChanges)
                {
                    if (healthBackground == null)
                    {
                        healthBackground = CreateHealthLayer("HealthBackground", missingHealthColor, healthFill.rectTransform.GetSiblingIndex());
                    }

                    if (healthRecentDamageFill == null)
                    {
                        healthRecentDamageFill = CreateHealthLayer("HealthRecentDamageFill", recentDamageColor, healthFill.rectTransform.GetSiblingIndex());
                    }

                    EnsureHealthLayerOrder();
                }
            }
        }

        private Image FindFillByName(params string[] keywords)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                string name = images[i].name.ToLowerInvariant();
                for (int k = 0; k < keywords.Length; k++)
                {
                    if (name.Contains(keywords[k]))
                    {
                        return images[i];
                    }
                }
            }

            return null;
        }

        private void ConfigureVisualSetup()
        {
            ConfigureFillImage(healthFill);
            ConfigureFillImage(healthRecentDamageFill);
            ConfigureFillImage(manaFill);
            ConfigureBackgroundImage(healthBackground);

            if (healthBackground != null)
            {
                healthBackground.raycastTarget = false;
                healthBackground.color = missingHealthColor;
            }

            if (healthRecentDamageFill != null)
            {
                healthRecentDamageFill.raycastTarget = false;
                healthRecentDamageFill.color = recentDamageColor;
            }

            if (healthFill != null)
            {
                healthFill.raycastTarget = false;
                if (showTickMarks) ApplyTickMaterial(healthFill);
            }
        }

        private void CacheBaseAnchors()
        {
            CacheAnchors(healthFill, out healthFillBaseAnchorMin, out healthFillBaseAnchorMax);
            CacheAnchors(healthRecentDamageFill, out healthRecentBaseAnchorMin, out healthRecentBaseAnchorMax);
            CacheAnchors(manaFill, out manaFillBaseAnchorMin, out manaFillBaseAnchorMax);
        }

        private static void CacheAnchors(Image image, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            anchorMin = Vector2.zero;
            anchorMax = Vector2.right;

            if (image == null)
            {
                return;
            }

            RectTransform rect = image.rectTransform;
            anchorMin = rect.anchorMin;
            anchorMax = rect.anchorMax;
        }

        private static void SetHorizontalRatio(Image image, float ratio, Vector2 baseAnchorMin, Vector2 baseAnchorMax)
        {
            if (image == null)
            {
                return;
            }

            ratio = Mathf.Clamp01(ratio);

            // Keep fillAmount updated for compatibility, but drive width via anchors
            // so bars still shrink even when sprite/fill settings are not ideal.
            image.fillAmount = ratio;

            RectTransform rect = image.rectTransform;
            float span = Mathf.Max(0f, baseAnchorMax.x - baseAnchorMin.x);
            Vector2 newMin = baseAnchorMin;
            Vector2 newMax = baseAnchorMax;
            newMax.x = baseAnchorMin.x + (span * ratio);

            rect.anchorMin = newMin;
            rect.anchorMax = newMax;
        }

        private static void ConfigureFillImage(Image image)
        {
            if (image == null) return;

            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        private static void ConfigureBackgroundImage(Image image)
        {
            if (image == null) return;

            image.type = Image.Type.Simple;
            image.fillAmount = 1f;
        }

        private void ApplyTickMaterial(Image target)
        {
            Shader tickShader = Shader.Find("UI/HealthBarTick");
            if (tickShader == null) return;
            
            Material mat = new Material(tickShader);
            mat.SetColor("_TickColor", tickColor);
            mat.SetFloat("_TickInterval", tickInterval);
            mat.SetFloat("_TickWidth", tickWidth);
            target.material = mat;
        }

        private Image CreateHealthLayer(string layerName, Color layerColor, int siblingIndex)
        {
            RectTransform healthRect = healthFill != null ? healthFill.rectTransform : null;
            if (healthRect == null || healthRect.parent == null)
            {
                return null;
            }

            GameObject layerObject = new GameObject(layerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            layerObject.transform.SetParent(healthRect.parent, false);

            RectTransform layerRect = layerObject.GetComponent<RectTransform>();
            layerRect.anchorMin = healthRect.anchorMin;
            layerRect.anchorMax = healthRect.anchorMax;
            layerRect.anchoredPosition = healthRect.anchoredPosition;
            layerRect.sizeDelta = healthRect.sizeDelta;
            layerRect.pivot = healthRect.pivot;
            layerRect.localScale = healthRect.localScale;
            layerRect.localRotation = healthRect.localRotation;

            Image layerImage = layerObject.GetComponent<Image>();
            layerImage.raycastTarget = false;
            layerImage.color = layerColor;

            ConfigureFillImage(layerImage);

            layerObject.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, healthRect.parent.childCount - 1));

            return layerImage;
        }

        private void EnsureHealthLayerOrder()
        {
            if (healthFill == null)
            {
                return;
            }

            RectTransform healthRect = healthFill.rectTransform;
            if (healthRect == null || healthRect.parent == null)
            {
                return;
            }

            int baseIndex = healthRect.GetSiblingIndex();

            if (healthBackground != null)
            {
                healthBackground.transform.SetSiblingIndex(baseIndex);
                baseIndex = healthBackground.transform.GetSiblingIndex();
            }

            if (healthRecentDamageFill != null)
            {
                healthRecentDamageFill.transform.SetSiblingIndex(baseIndex + 1);
            }

            healthRect.SetSiblingIndex(baseIndex + 2);

            if (manaFill != null)
            {
                // Mana should remain below all HP layers in visual priority.
                manaFill.transform.SetSiblingIndex(0);
            }
        }

        private void ValidateSetupWarnings()
        {
#if UNITY_EDITOR
            // Skip warnings for prefab assets in Project view (no scene parent context).
            if (!gameObject.scene.IsValid())
            {
                return;
            }

            if (targetEntity == null)
            {
                // Avoid noise on isolated prefab roots (no parent yet): this component resolves
                // targetEntity at runtime once instantiated under a BaseEntity.
                bool hasParent = transform.parent != null;
                bool hasEntityInParents = GetComponentInParent<BaseEntity>(true) != null;

                if (hasParent && !hasEntityInParents)
                {
                    Debug.LogWarning($"[{nameof(FloatingStatusBar)}] Missing targetEntity in '{gameObject.name}'. The bar cannot track HP/MP without a BaseEntity in parent hierarchy.", this);
                }
            }

            if (healthFill == null)
            {
                Debug.LogWarning($"[{nameof(FloatingStatusBar)}] Missing healthFill reference in '{gameObject.name}'. Expected a child Image named 'HealthFill'.", this);
            }

            if (healthBackground == null)
            {
                Debug.LogWarning($"[{nameof(FloatingStatusBar)}] Missing healthBackground in '{gameObject.name}'. HP missing segment won't be visible.", this);
            }

            if (healthRecentDamageFill == null)
            {
                Debug.LogWarning($"[{nameof(FloatingStatusBar)}] Missing healthRecentDamageFill in '{gameObject.name}'. Delayed damage chip effect is disabled.", this);
            }

            if (manaFill == null)
            {
                Debug.LogWarning($"[{nameof(FloatingStatusBar)}] Missing manaFill reference in '{gameObject.name}'. Mana bar won't render changes.", this);
            }
#endif
        }
    }
}
