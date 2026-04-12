using UnityEngine;
using UnityEngine.InputSystem;
using MobaGameplay.Abilities;

namespace MobaGameplay.UI.Targeting
{
    public class TargetingManager : MonoBehaviour
    {
        public static TargetingManager Instance { get; private set; }

        [Header("Indicators Prefabs")]
        [SerializeField] private GameObject circleIndicatorPrefab;
        [SerializeField] private GameObject lineIndicatorPrefab;
        [SerializeField] private GameObject trailIndicatorPrefab;
        
        [Header("Colors")]
        public Color friendlyColor = new Color(0, 1, 1, 0.5f);
        public Color enemyColor = new Color(1, 0, 0, 0.5f);

        private GameObject activeIndicatorObj;
        private CircleIndicator activeCircle;
        private LineIndicator activeLine;
        private TrailIndicator activeTrail;
        
        private BaseAbility currentAimingAbility;
        private AbilityData currentAimingData;
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Lazy initialization: if playerTransform is null, try to find the player by name
            if (playerTransform == null)
            {
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log("[TargetingManager] Auto-initialized with Player from Start");
                }
                else
                {
                    Debug.LogWarning("[TargetingManager] Could not find Player GameObject");
                }
            }
        }

        public void Initialize(Transform player)
        {
            playerTransform = player;
        }

        public void StartTargeting(BaseAbility ability)
        {
            if (ability.TargetingType == IndicatorType.None) return;
            
            currentAimingAbility = ability;
            currentAimingData = null; // Clear data reference
            HideIndicator();

            if (ability.TargetingType == IndicatorType.Circle)
            {
                activeIndicatorObj = Instantiate(circleIndicatorPrefab);
                activeCircle = activeIndicatorObj.GetComponent<CircleIndicator>();
                activeCircle.SetRadius(ability.Range);
                activeCircle.SetColor(friendlyColor);
            }
            else if (ability.TargetingType == IndicatorType.Line)
            {
                activeIndicatorObj = Instantiate(lineIndicatorPrefab);
                activeLine = activeIndicatorObj.GetComponent<LineIndicator>();
                activeLine.SetDimensions(ability.Range, ability.Width);
                activeLine.SetColor(friendlyColor);
            }
            else if (ability.TargetingType == IndicatorType.Trail)
            {
                // Use dedicated trail prefab if assigned, otherwise reuse line prefab.
                GameObject sourcePrefab = trailIndicatorPrefab != null ? trailIndicatorPrefab : lineIndicatorPrefab;
                activeIndicatorObj = Instantiate(sourcePrefab);

                activeTrail = activeIndicatorObj.GetComponent<TrailIndicator>();
                if (activeTrail == null)
                    activeTrail = activeIndicatorObj.AddComponent<TrailIndicator>();

                activeTrail.SetDimensions(ability.Range, ability.Width);
                activeTrail.SetColor(new Color(1f, 0.4f, 0f, 0.7f));
            }
        }

        /// <summary>
        /// Start targeting mode using AbilityData (data-driven system).
        /// </summary>
        public void StartTargetingForData(AbilityData data)
        {
            if (data == null || data.targetingType == IndicatorType.None) return;

            currentAimingData = data;
            currentAimingAbility = null; // Clear ability reference
            HideIndicator();

            if (data.targetingType == IndicatorType.Circle)
            {
                activeIndicatorObj = Instantiate(circleIndicatorPrefab);
                activeCircle = activeIndicatorObj.GetComponent<CircleIndicator>();
                activeCircle.SetRadius(data.range);
                activeCircle.SetColor(friendlyColor);
            }
            else if (data.targetingType == IndicatorType.Line)
            {
                activeIndicatorObj = Instantiate(lineIndicatorPrefab);
                activeLine = activeIndicatorObj.GetComponent<LineIndicator>();
                activeLine.SetDimensions(data.range, data.width);
                activeLine.SetColor(friendlyColor);
            }
            else if (data.targetingType == IndicatorType.Trail)
            {
                GameObject sourcePrefab = trailIndicatorPrefab != null ? trailIndicatorPrefab : lineIndicatorPrefab;
                activeIndicatorObj = Instantiate(sourcePrefab);

                activeTrail = activeIndicatorObj.GetComponent<TrailIndicator>();
                if (activeTrail == null)
                    activeTrail = activeIndicatorObj.AddComponent<TrailIndicator>();

                activeTrail.SetDimensions(data.range, data.width);
                activeTrail.SetColor(new Color(1f, 0.4f, 0f, 0.7f));
            }
        }

        public void CancelTargeting()
        {
            HideIndicator();
            currentAimingAbility = null;
            currentAimingData = null;
        }

        private void HideIndicator()
        {
            if (activeIndicatorObj != null)
            {
                Destroy(activeIndicatorObj);
            }
            activeCircle = null;
            activeLine = null;
            activeTrail = null;
        }

        private void Update()
        {
            if (activeIndicatorObj == null || playerTransform == null) return;
            if (currentAimingAbility == null && currentAimingData == null) return;
            if (Camera.main == null || Mouse.current == null) return;

            // Determine active targeting parameters from either system
            IndicatorType targetType;
            float targetCastRange;
            if (currentAimingData != null)
            {
                targetType = currentAimingData.targetingType;
                targetCastRange = currentAimingData.castRange;
            }
            else if (currentAimingAbility != null)
            {
                targetType = currentAimingAbility.TargetingType;
                targetCastRange = currentAimingAbility.CastRange;
            }
            else return;

            // Get mouse position on ground plane
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, playerTransform.position);
            
            if (groundPlane.Raycast(ray, out float hitDist))
            {
                Vector3 targetPoint = ray.GetPoint(hitDist);

                if (targetType == IndicatorType.Circle)
                {
                    // Circle can be moved with mouse within cast range
                    Vector3 direction = targetPoint - playerTransform.position;
                    if (direction.magnitude > targetCastRange)
                    {
                        targetPoint = playerTransform.position + direction.normalized * targetCastRange;
                    }
                    activeIndicatorObj.transform.position = targetPoint + Vector3.up * 0.1f;
                }
                else if (targetType == IndicatorType.Line)
                {
                    // Line originates from player and rotates towards mouse
                    activeIndicatorObj.transform.position = playerTransform.position + Vector3.up * 0.1f;
                    Vector3 lookDir = targetPoint - playerTransform.position;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                    {
                        activeIndicatorObj.transform.rotation = Quaternion.LookRotation(lookDir);
                    }
                }
                else if (targetType == IndicatorType.Trail)
                {
                    // Trail: same positioning as Line — originates at player, rotates to mouse
                    activeIndicatorObj.transform.position = playerTransform.position + Vector3.up * 0.1f;
                    Vector3 trailDir = targetPoint - playerTransform.position;
                    trailDir.y = 0;
                    if (trailDir != Vector3.zero)
                    {
                        activeIndicatorObj.transform.rotation = Quaternion.LookRotation(trailDir);
                    }
                }
            }
        }
    }
}