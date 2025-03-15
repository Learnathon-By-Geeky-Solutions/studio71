using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MachineGunAI
{
// Main Machine Gunner AI Controller
    public class MachineGunnerAI : MonoBehaviour
    {
        // Current state
        private IMachineGunnerState currentState;

        // State instances
        [HideInInspector] public IdleState idleState;
        [HideInInspector] public AlertState alertState;
        [HideInInspector] public SuppressiveFireState suppressiveFireState;
        [HideInInspector] public PrecisionFireState precisionFireState;
        [HideInInspector] public ReloadState reloadState;
        [HideInInspector] public OverheatedState overheatedState;

        // Detection and targeting
        [Header("Detection")] [SerializeField] private float alertDetectionRange = 40f;
        [SerializeField] private float suppressiveFireRange = 30f;
        [SerializeField] private float precisionFireRange = 20f;
        [SerializeField] private float firingArc = 120f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private Transform gunPivot;
        [SerializeField] private float rotationSpeed = 3f;

        // Weapon parameters
        [Header("Weapon")] [SerializeField] private int magazineSize = 100;
        [SerializeField] private float reloadTime = 4.5f;
        [SerializeField] private float heatGeneration = 1.0f;
        [SerializeField] private float heatThreshold = 50f;
        [SerializeField] private float coolingRate = 10f;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource gunAudioSource;
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip overheatedSound;

        // Scan pattern for idle state
        [Header("Scan Pattern")] [SerializeField]
        private float scanAngle = 60f;

        [SerializeField] private float scanSpeed = 20f;

        // Debug visualization
        [Header("Debug")] [SerializeField] private bool showDebugGizmos = true;

        // Internal state tracking
        private Transform targetTransform;
        private int currentAmmo;
        private float currentHeat = 0f;
        private Vector3 lastKnownTargetPosition;
        private float timeSinceTargetSeen = 0f;
        private float memoryDuration = 5f;

        // Properties
        public Transform Target => targetTransform;
        public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;
        public int CurrentAmmo => currentAmmo;
        public float CurrentHeat => currentHeat;
        public float HeatThreshold => heatThreshold;
        public float ReloadTime => reloadTime;

        // Expose parameters for states to use
        public float ScanAngle => scanAngle;
        public float ScanSpeed => scanSpeed;
        public Transform GunPivot => gunPivot;
        public Transform MuzzlePoint => muzzlePoint;
        public ParticleSystem MuzzleFlash => muzzleFlash;
        public AudioSource GunAudioSource => gunAudioSource;
        public AudioClip FireSound => fireSound;
        public AudioClip ReloadSound => reloadSound;
        public AudioClip OverheatedSound => overheatedSound;

        private void Awake()
        {
            // Initialize all state instances
            idleState = new IdleState(this);
            alertState = new AlertState(this);
            suppressiveFireState = new SuppressiveFireState(this);
            precisionFireState = new PrecisionFireState(this);
            reloadState = new ReloadState(this);
            overheatedState = new OverheatedState(this);
        }

        private void Start()
        {
            // Set initial ammo
            currentAmmo = magazineSize;

            // Start in idle state
            TransitionToState(idleState);
        }

        private void Update()
        {
            // Update current state
            currentState?.UpdateState();

            // Cool down gun when not actively firing
            if (!(currentState is SuppressiveFireState || currentState is PrecisionFireState))
            {
                currentHeat = Mathf.Max(0, currentHeat - coolingRate * Time.deltaTime);
            }

            // Update target memory
            if (targetTransform != null)
            {
                timeSinceTargetSeen = 0f;
                lastKnownTargetPosition = targetTransform.position;
            }
            else if (timeSinceTargetSeen < memoryDuration)
            {
                timeSinceTargetSeen += Time.deltaTime;
            }
        }

        // Method to transition between states
        public void TransitionToState(IMachineGunnerState newState)
        {
            // Exit current state
            currentState?.OnExit();

            // Update current state
            currentState = newState;

            // Debug log for state transitions
            Debug.Log($"{gameObject.name} transitioning to {newState.GetType().Name}");

            // Enter new state
            currentState.OnEnter();
        }

        // Detection methods
        public bool DetectTargetInAlertRange()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, alertDetectionRange, targetLayers);
            foreach (Collider hit in hits)
            {
                Vector3 directionToTarget = hit.transform.position - transform.position;
                float angle = Vector3.Angle(gunPivot.forward, directionToTarget);

                // Check if within firing arc
                if (angle <= firingArc / 2)
                {
                    // Check line of sight
                    if (!Physics.Raycast(muzzlePoint.position, directionToTarget.normalized,
                            directionToTarget.magnitude, obstacleLayers))
                    {
                        SetTarget(hit.transform);
                        return true;
                    }
                }
            }

            return false;
        }

        // Check if target is in suppressive fire range
        public bool IsTargetInSuppressiveRange()
        {
            if (targetTransform == null) return false;

            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            return distanceToTarget <= suppressiveFireRange;
        }

        // Check if target is in precision fire range
        public bool IsTargetInPrecisionRange()
        {
            if (targetTransform == null) return false;

            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            return distanceToTarget <= precisionFireRange;
        }

        // Check if target is in firing arc
        public bool IsTargetInFiringArc()
        {
            if (targetTransform == null) return false;

            Vector3 directionToTarget = targetTransform.position - transform.position;
            float angle = Vector3.Angle(gunPivot.forward, directionToTarget);
            return angle <= firingArc / 2;
        }

        // Check if we have direct line of sight to target
        public bool HasLineOfSightToTarget()
        {
            if (targetTransform == null) return false;

            Vector3 directionToTarget = targetTransform.position - muzzlePoint.position;
            if (Physics.Raycast(muzzlePoint.position, directionToTarget.normalized,
                    out RaycastHit hit, suppressiveFireRange, obstacleLayers | targetLayers))
            {
                return hit.transform == targetTransform;
            }

            return false;
        }

        // Rotate toward a target position
        public void RotateToward(Vector3 targetPosition, float speedMultiplier = 1f)
        {
            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0; // Keep rotation on y-axis only if needed

            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                gunPivot.rotation = Quaternion.Slerp(
                    gunPivot.rotation,
                    targetRotation,
                    rotationSpeed * speedMultiplier * Time.deltaTime
                );
            }
        }

        // Firing methods
        public void FireBullet(float accuracy)
        {
            if (currentAmmo <= 0)
            {
                TransitionToState(reloadState);
                return;
            }

            // Decrease ammo
            currentAmmo--;

            // Increase heat
            currentHeat += heatGeneration;
            if (currentHeat >= heatThreshold)
            {
                TransitionToState(overheatedState);
                return;
            }

            // Calculate spread based on accuracy (0-1 value, higher is more accurate)
            float spread = (1f - accuracy) * 5f;

            // Apply spread to direction
            Vector3 spreadDirection = muzzlePoint.forward;
            spreadDirection += new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread)
            ).normalized * spread;

            // Create raycast for bullet
            if (Physics.Raycast(muzzlePoint.position, spreadDirection, out RaycastHit hit, 100f))
            {
                // Handle hit - apply damage if it's a valid target
                if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Enemy"))
                {
                    // Apply damage to target
                    IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                    damageable?.TakeDamage(5); // Example damage value
                }

                // Visual feedback for hit location
                Debug.DrawLine(muzzlePoint.position, hit.point, Color.red, 0.2f);
            }
            else
            {
                // Visual feedback for miss
                Debug.DrawRay(muzzlePoint.position, spreadDirection * 100f, Color.blue, 0.1f);
            }

            // Play effects
            muzzleFlash.Play();

            // Play sound
            if (gunAudioSource && fireSound)
            {
                gunAudioSource.PlayOneShot(fireSound);
            }
        }

        // Reload method
        public void ReloadMagazine()
        {
            currentAmmo = magazineSize;

            // Play reload sound
            if (gunAudioSource && reloadSound)
            {
                gunAudioSource.PlayOneShot(reloadSound);
            }
        }

        // Set target method
        public void SetTarget(Transform newTarget)
        {
            targetTransform = newTarget;
            if (newTarget != null)
            {
                lastKnownTargetPosition = newTarget.position;
                timeSinceTargetSeen = 0f;
            }
        }

        // Clear target method
        public void ClearTarget()
        {
            targetTransform = null;
        }

        // Visualization gizmos for debugging
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Draw detection ranges
            Gizmos.color = Color.yellow;
            DrawArc(transform.position, alertDetectionRange, firingArc);

            Gizmos.color = Color.red;
            DrawArc(transform.position, suppressiveFireRange, firingArc);

            Gizmos.color = Color.magenta;
            DrawArc(transform.position, precisionFireRange, firingArc);

            // Draw target connection if available
            if (targetTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(muzzlePoint.position, targetTransform.position);
            }
        }

        // Helper to draw arcs for gizmos
        private void DrawArc(Vector3 position, float radius, float angle)
        {
            Vector3 forward = gunPivot != null ? gunPivot.forward : transform.forward;
            float halfAngle = angle / 2;
            int segments = 20;
            Vector3 previous = position + GetPointOnArc(forward, radius, -halfAngle);

            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (i * angle / segments);
                Vector3 current = position + GetPointOnArc(forward, radius, currentAngle);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }

            // Draw two lines from center to arc edges
            Vector3 leftPoint = position + GetPointOnArc(forward, radius, -halfAngle);
            Vector3 rightPoint = position + GetPointOnArc(forward, radius, halfAngle);
            Gizmos.DrawLine(position, leftPoint);
            Gizmos.DrawLine(position, rightPoint);
        }

        // Helper to get a point on an arc
        private Vector3 GetPointOnArc(Vector3 forward, float radius, float angle)
        {
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            return forward * radius * Mathf.Cos(angle * Mathf.Deg2Rad) +
                   right * radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        }
    }
}