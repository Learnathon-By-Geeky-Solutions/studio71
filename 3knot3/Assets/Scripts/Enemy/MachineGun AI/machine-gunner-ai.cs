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
        [Header("Detection")]
        [SerializeField] private float alertDetectionRange = 40f;
        [SerializeField] private float suppressiveFireRange = 30f;
        [SerializeField] private float precisionFireRange = 20f;
        [SerializeField] private float firingArc = 120f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private Transform gunPivot;
        [SerializeField] private float rotationSpeed = 3f;
        // Weapon parameters
        [Header("Weapon")]
        [SerializeField] private int magazineSize = 100;
        [SerializeField] private float reloadTime = 4.5f;
        [SerializeField] private float heatGeneration = 1.0f;
        [SerializeField] private float heatThreshold = 50f;
        [SerializeField] private float coolingRate = 10f;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private GameObject bulletPrefab; // Bullet prefab to instantiate
        [SerializeField] private float bulletSpeed = 100f; // Speed of the bullet
        // Scan pattern for idle state
        [Header("Scan Pattern")]
        [SerializeField] private float scanAngle = 60f;
        [SerializeField] private float scanSpeed = 20f;

        // Debug visualization
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        // Internal state tracking
        private Transform targetTransform;
        private int currentAmmo;
        private float currentHeat = 0f;
        private Vector3 lastKnownTargetPosition;
        private float timeSinceTargetSeen = 0f;
        private float memoryDuration = 5f;
        private IMachineGunnerState previousCombatState; // Track previous combat state

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
        public float AlertDetectionRange => alertDetectionRange; // Expose alert range

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

            // Store the previous combat state (if applicable)
            if (newState != idleState && newState != reloadState && newState != overheatedState)
            {
                previousCombatState = currentState;
            }

            // Update current state
            currentState = newState;
            // Debug log for state transitions
            Debug.Log($"{gameObject.name} transitioning to {newState.GetType().Name}");
            // Enter new state
            newState.OnEnter();
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

        // Check if target is in alert range
        public bool IsTargetInAlertRange()
        {
            if (targetTransform == null) return false;
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            return distanceToTarget <= alertDetectionRange;
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
                0
            );
            spreadDirection.Normalize();

            // Instantiate bullet and set its velocity
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.linearVelocity = spreadDirection * bulletSpeed;

            // Optional: Play firing sound or animation
            // gunAudioSource.PlayOneShot(fireSound);
            // gunAnimator.SetTrigger("Fire");
        }

        // Reload method
        public void Reload()
        {
            currentAmmo = magazineSize;
            currentHeat = 0;
            // Optional: Play reload sound or animation
            // gunAudioSource.PlayOneShot(reloadSound);
            // gunAnimator.SetTrigger("Reload");

            // After reload, transition back to the previous combat state
            if (previousCombatState != null)
            {
                TransitionToState(previousCombatState);
            }
            else
            {
                TransitionToState(idleState); // If no previous combat state, go to idle
            }
        }

        public void SetTarget(Transform target)
        {
            targetTransform = target;
            timeSinceTargetSeen = 0f;
        }

        public void ClearTarget()
        {
            targetTransform = null;
        }

        // Gizmos for debug visualization
        private void OnDrawGizmos()
        {
            if (showDebugGizmos)
            {
                // Draw alert range
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, alertDetectionRange);
                // Draw suppressive fire range
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, suppressiveFireRange);
                // Draw precision fire range
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, precisionFireRange);
                // Draw firing arc
                Gizmos.color = Color.blue;
                Vector3 forward = gunPivot.forward;
                Vector3 right = Quaternion.Euler(0, firingArc / 2, 0) * forward;
                Vector3 left = Quaternion.Euler(0, -firingArc / 2, 0) * forward;
                Gizmos.DrawLine(gunPivot.position, gunPivot.position + right * alertDetectionRange);
                Gizmos.DrawLine(gunPivot.position, gunPivot.position + left * alertDetectionRange);
            }
        }
    }
}