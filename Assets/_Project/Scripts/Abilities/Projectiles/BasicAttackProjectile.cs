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

        private float speed = 20f;
        private float maxDistance = 15f;
        private float collisionRadius = 0.3f;
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
                trailRenderer.time = 0.15f;
                trailRenderer.startWidth = 0.15f;
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
            maxDistance *= 1.2f; // Charged attacks travel slightly farther
            
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
                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.8f || hit.point.y < 0.1f) 
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
            impact.transform.localScale = Vector3.one * 0.5f;
            
            // Orange-red glow material
            var renderer = impact.GetComponent<Renderer>();
            renderer.material.color = chargedColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", chargedColor * 2f);
            
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
        private float fadeTimer = 0f;
        private float fadeDuration = 0.3f;
        private Color baseColor;
        private Vector3 targetScale;
        private bool initialized = false;

        public void Initialize(Color color)
        {
            baseColor = color;
            targetScale = transform.localScale * 2f;
            initialized = true;
            Destroy(gameObject, fadeDuration);
        }

        private void Update()
        {
            if (!initialized) return;
            
            fadeTimer += Time.deltaTime;
            float t = fadeTimer / fadeDuration;
            
            // Expand and fade
            transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, targetScale, t);
            
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
