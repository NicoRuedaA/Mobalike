using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities.Projectiles
{
    public class BasicAttackProjectile : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(1f, 0.9f, 0.5f, 1f);
        [SerializeField] private Color chargedColor = new Color(1f, 0.3f, 0.1f, 1f);
        [SerializeField] private float normalEmission = 0.3f;
        [SerializeField] private float chargedEmission = 1.5f;

        [Header("Projectile Settings")]
        [SerializeField] private float speed = 20f;
        [SerializeField] private float maxDistance = 15f;
        [SerializeField] private float collisionRadius = 0.3f;

        [Header("Charge Settings")]
        [SerializeField] private float chargedRangeMultiplier = 1.2f;

        [Header("Ground Detection")]
        [SerializeField] private float groundNormalThreshold = 0.8f;
        [SerializeField] private float groundHeightThreshold = 0.1f;

        [Header("Trail Settings")]
        [SerializeField] private float trailTime = 0.15f;
        [SerializeField] private float trailStartWidth = 0.15f;

        [Header("Impact Settings")]
        [SerializeField] private float impactBaseScale = 0.5f;
        [SerializeField] private float impactEmissionMultiplier = 2f;

        // Public read-only properties
        public float Speed => speed;
        public float MaxDistance => maxDistance;
        public float ChargedRangeMultiplier => chargedRangeMultiplier;

        private LayerMask hitLayers;
        private Vector3 startPos;
        private Vector3 direction;
        private float damage;
        private DamageType damageType;
        private BaseEntity owner;
        private bool isInitialized = false;
        private bool isCharged = false;
        private TrailRenderer trailRenderer;
        private Renderer projectileRenderer;

        public void Initialize(Vector3 dir, float dmg, DamageType type, BaseEntity caster, float spd, float maxDist, LayerMask layers)
        {
            direction = dir.normalized;
            damage = dmg;
            damageType = type;
            owner = caster;
            speed = spd;
            maxDistance = maxDist;
            hitLayers = layers;
            
            startPos = transform.position;
            transform.forward = direction;
            
            // Cache renderer
            projectileRenderer = GetComponent<Renderer>();
            
            // Add Trail Renderer if not present
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                trailRenderer.time = trailTime;
                trailRenderer.startWidth = trailStartWidth;
                trailRenderer.endWidth = 0f;
                trailRenderer.startColor = new Color(1f, 0.9f, 0.5f, 0.6f);
                trailRenderer.endColor = new Color(1f, 0.9f, 0.5f, 0f);
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            // Apply normal appearance
            ApplyAppearance(normalColor, normalEmission);
            
            isInitialized = true;
        }

        /// <summary>
        /// Scales the projectile for charged attacks: larger size, faster, more damage, enhanced visuals.
        /// </summary>
        public void ApplyChargeMultiplier(float sizeMultiplier, float speedMultiplier, float damageMultiplier)
        {
            isCharged = true;
            
            collisionRadius *= sizeMultiplier;
            speed *= speedMultiplier;
            damage *= damageMultiplier;
            maxDistance *= chargedRangeMultiplier;
            
            // Scale the visual mesh
            transform.localScale *= sizeMultiplier;
            
            // Enhanced charged visuals
            ApplyAppearance(chargedColor, chargedEmission);
            
            // Enhance trail for charged attack
            if (trailRenderer != null)
            {
                trailRenderer.startWidth *= sizeMultiplier * 1.2f;
                trailRenderer.endWidth *= sizeMultiplier * 1.2f;
                trailRenderer.startColor = new Color(chargedColor.r, chargedColor.g, chargedColor.b, 0.9f);
                trailRenderer.endColor = new Color(chargedColor.r, chargedColor.g, chargedColor.b, 0f);
                trailRenderer.time *= 1.5f; // Longer trail
            }
        }

        private void ApplyAppearance(Color color, float emissionIntensity)
        {
            if (projectileRenderer != null)
            {
                // Set base color
                projectileRenderer.material.color = color;
                
                // Try to apply emission for glow effect if supported
                if (projectileRenderer.material.HasProperty("_EmissionColor"))
                {
                    projectileRenderer.material.EnableKeyword("_EMISSION");
                    Color emissionColor = color * emissionIntensity;
                    projectileRenderer.material.SetColor("_EmissionColor", emissionColor);
                }
            }
            
            // Also color the trail if it exists
            if (trailRenderer != null)
            {
                trailRenderer.startColor = new Color(color.r, color.g, color.b, 0.6f);
                trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
            }
        }

        private void Update()
        {
            if (!isInitialized || direction == Vector3.zero) return;

            float distanceToMove = speed * Time.deltaTime;
            
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, collisionRadius, direction, distanceToMove, hitLayers);
            
            foreach (var hit in hits)
            {
                if (hit.collider.isTrigger) continue; // Ignoramos triggers de visión, aggro, etc.

                BaseEntity target = hit.collider.GetComponentInParent<BaseEntity>();
                
                // Ignore self
                if (target == owner) continue; 

                if (target != null)
                {
                    if (!target.IsDead) 
                    {
                        target.TakeDamage(new DamageInfo(damage, damageType, owner));
                        HitAndDestroy();
                        return;
                    }
                }
                else 
                {
                    // Si es el suelo (normal hacia arriba) lo ignoramos para evitar chocar por error
                    if (Vector3.Dot(hit.normal, Vector3.up) > groundNormalThreshold || hit.point.y < groundHeightThreshold) 
                        continue;

                    // Es una pared u obstáculo estático
                    HitAndDestroy();
                    return;
                }
            }

            transform.position += direction * distanceToMove;

            if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private void HitAndDestroy()
        {
            // Spawn impact effect for charged shots
            if (isCharged)
            {
                SpawnChargedImpact();
            }
            Destroy(gameObject);
        }

        private void SpawnChargedImpact()
        {
            // Create a small explosion effect
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.transform.position = transform.position;
            impact.transform.localScale = Vector3.one * impactBaseScale;
            
            // Orange-red glow material
            var renderer = impact.GetComponent<Renderer>();
            renderer.material.color = chargedColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", chargedColor * impactEmissionMultiplier);
            
            // Animate and destroy
            impact.AddComponent<ChargedImpactFade>().Initialize(chargedColor);
        }

        private void OnDestroy()
        {
            // Cleanup emission keyword if supported
            if (projectileRenderer != null && projectileRenderer.material.HasProperty("_EmissionColor"))
            {
                projectileRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }

    /// <summary>
    /// Simple component to animate and fade the charged impact effect.
    /// </summary>
    public class ChargedImpactFade : MonoBehaviour
    {
        [Header("Impact Animation")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float startScale = 0.5f;
        [SerializeField] private float targetScaleMultiplier = 2f;

        private float fadeTimer = 0f;
        private Color baseColor;
        private Vector3 targetScale;
        private bool initialized = false;

        public void Initialize(Color color)
        {
            baseColor = color;
            targetScale = transform.localScale * targetScaleMultiplier;
            initialized = true;
            Destroy(gameObject, fadeDuration);
        }

        private void Update()
        {
            if (!initialized) return;
            
            fadeTimer += Time.deltaTime;
            float t = fadeTimer / fadeDuration;
            
            // Expand and fade
            transform.localScale = Vector3.Lerp(Vector3.one * startScale, targetScale, t);
            
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Color c = baseColor;
                c.a = 1f - t;
                renderer.material.color = c;
                renderer.material.SetColor("_EmissionColor", baseColor * (2f - t * 2f));
            }
        }
    }
}
