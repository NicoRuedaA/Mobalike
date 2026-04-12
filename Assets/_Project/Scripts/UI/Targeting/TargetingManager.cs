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

        public void Initialize(Transform player)
        {
            playerTransform = player;
        }

        public void StartTargeting(BaseAbility ability)
        {
            if (ability.TargetingType == IndicatorType.None) return;
            
            currentAimingAbility = ability;
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

        public void CancelTargeting()
        {
            HideIndicator();
            currentAimingAbility = null;
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
            if (activeIndicatorObj == null || currentAimingAbility == null || playerTransform == null) return;
            if (Camera.main == null || Mouse.current == null) return;

            // Get mouse position on ground plane
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, playerTransform.position);
            
            if (groundPlane.Raycast(ray, out float hitDist))
            {
                Vector3 targetPoint = ray.GetPoint(hitDist);

                if (currentAimingAbility.TargetingType == IndicatorType.Circle)
                {
                    // Circle can be moved with mouse within cast range
                    Vector3 direction = targetPoint - playerTransform.position;
                    if (direction.magnitude > currentAimingAbility.CastRange)
                    {
                        targetPoint = playerTransform.position + direction.normalized * currentAimingAbility.CastRange;
                    }
                    activeIndicatorObj.transform.position = targetPoint + Vector3.up * 0.1f;
                }
                else if (currentAimingAbility.TargetingType == IndicatorType.Line)
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
                else if (currentAimingAbility.TargetingType == IndicatorType.Trail)
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