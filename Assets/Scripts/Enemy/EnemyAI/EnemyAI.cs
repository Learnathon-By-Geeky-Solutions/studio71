using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace patrolEnemy
{
    public class EnemyAI : MonoBehaviour
    {
        // State Machine
        private IEnemyState currentState;
        public IdleState idleState;
        public AlertState alertState;
        public FollowState followState;
        public ShootState shootState;
        public GrenadeThrowState grenadeThrowState;
        public RecoverState recoverState;

        // References
        public Transform player;
        public NavMeshAgent navMeshAgent;
        public Transform firePoint;
        public Transform grenadeThrowPoint;

        // Detection and Attack
        public float detectionRange = 15f;
        public float attackRange = 8f;
        public LayerMask obstacleLayer;

        // Health System
        public float maxHealth = 100f;
        public float currentHealth;
        public float recoveryThreshold = 30f;
        public float recoveryRate = 10f;

        // Shooting
        public GameObject bulletPrefab;
        public float fireRate = 1f;
        public int maxAmmo = 30;
        public int currentAmmo;
        public float reloadTime = 2f;
        public bool isReloading = false;

        // Grenade
        public GameObject grenadePrefab;
        public int maxGrenades = 3;
        public int currentGrenades;
        public float grenadeThrowCooldown = 5f;
        public bool canThrowGrenade = true;

        // Alert State
        public float alertCountdown = 3f;
        public float currentAlertTime = 0f;

        // Random Patrol
        public float patrolRadius = 10f;
        public Vector3 startPosition;
        private Vector3 currentPatrolDestination;

        // Status flags
        public bool playerInDetectionRange = false;
        public bool playerInAttackRange = false;
        public bool playerInLineOfSight = false;

        void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            // Store starting position for patrol radius
            startPosition = transform.position;

            // Initialize states
            idleState = new IdleState(this);
            alertState = new AlertState(this);
            followState = new FollowState(this);
            shootState = new ShootState(this);
            grenadeThrowState = new GrenadeThrowState(this);
            recoverState = new RecoverState(this);

            // Initialize health and ammo
            currentHealth = maxHealth;
            currentAmmo = maxAmmo;
            currentGrenades = maxGrenades;

            // Set initial state
            currentState = idleState;
            currentState.Enter();
        }

        void Update()
        {
            // Check ranges
            CheckPlayerRanges();

            // Execute current state
            currentState.Execute();

            // Check if health is below recovery threshold
            if (currentHealth < recoveryThreshold && currentState != recoverState)
            {
                ChangeState(recoverState);
            }
        }

        public void ChangeState(IEnemyState newState)
        {
            currentState.Exit();
            currentState = newState;
            currentState.Enter();
        }

        void CheckPlayerRanges()
        {
            if (player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Check if player is in detection range
            playerInDetectionRange = distanceToPlayer <= detectionRange;

            // Check if player is in attack range
            playerInAttackRange = distanceToPlayer <= attackRange;

            // Check if player is in line of sight
            playerInLineOfSight = CheckLineOfSight();
        }

        bool CheckLineOfSight()
        {
            if (player == null) return false;

            Vector3 directionToPlayer = player.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            // Debug ray for visualization
            Debug.DrawRay(transform.position, directionToPlayer, Color.red);

            // Check if there are obstacles between enemy and player
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer,
                    obstacleLayer))
            {
                return false; // Something is blocking the line of sight
            }

            return true; // Clear line of sight
        }

        public void TakeDamage(float damageAmount)
        {
            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            // Handle death - animation, drops, etc.
            Destroy(gameObject);
        }

        public void Shoot()
        {
            if (currentAmmo <= 0 || isReloading) return;

            // Instantiate bullet
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Decrease ammo
            currentAmmo--;

            // Reload if out of ammo
            if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }

        IEnumerator Reload()
        {
            isReloading = true;

            // Play reload animation or sound

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = maxAmmo;
            isReloading = false;
        }

        public void ThrowGrenade()
        {
            if (currentGrenades <= 0 || !canThrowGrenade) return;

            // Calculate throw direction and force
            Vector3 throwDirection = (player.position - grenadeThrowPoint.position).normalized;
            float throwForce = 10f; // Adjust based on distance

            // Instantiate grenade
            GameObject grenade = Instantiate(grenadePrefab, grenadeThrowPoint.position, Quaternion.identity);
            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

            if (grenadeRb != null)
            {
                grenadeRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }

            // Decrease grenade count
            currentGrenades--;

            // Start cooldown
            StartCoroutine(GrenadeCooldown());
        }

        IEnumerator GrenadeCooldown()
        {
            canThrowGrenade = false;
            yield return new WaitForSeconds(grenadeThrowCooldown);
            canThrowGrenade = true;
        }

        public Transform FindCover()
        {
            // Find potential cover spots
            Collider[] obstacles = Physics.OverlapSphere(transform.position, detectionRange, obstacleLayer);

            if (obstacles.Length > 0)
            {
                // Find the closest obstacle that blocks line of sight from player
                float closestDistance = float.MaxValue;
                Transform bestCover = null;

                foreach (Collider obstacle in obstacles)
                {
                    Vector3 directionToObstacle = obstacle.transform.position - transform.position;
                    float distanceToObstacle = directionToObstacle.magnitude;

                    // Check if obstacle is closer than the current best
                    if (distanceToObstacle < closestDistance)
                    {
                        // Check if obstacle blocks line of sight from player
                        Vector3 obstacleToPlayer = player.position - obstacle.transform.position;
                        if (Physics.Raycast(obstacle.transform.position, obstacleToPlayer.normalized,
                                obstacleToPlayer.magnitude, obstacleLayer))
                        {
                            closestDistance = distanceToObstacle;
                            bestCover = obstacle.transform;
                        }
                    }
                }

                return bestCover;
            }

            return null;
        }

        public Vector3 FindPositionWithLineOfSight()
        {
            // Find a position where the enemy can see the player
            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;

            // Try several positions around the player
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // 45 degrees increments
                float distance = attackRange * 0.8f; // Slightly inside attack range

                Vector3 offset = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance,
                    0,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance
                );

                Vector3 potentialPosition = playerPosition + offset;

                // Check if this position is on the NavMesh
                if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    // Check if there's a clear line of sight from this position
                    Vector3 directionToPlayer = playerPosition - hit.position;
                    if (!Physics.Raycast(hit.position, directionToPlayer.normalized, directionToPlayer.magnitude,
                            obstacleLayer))
                    {
                        return hit.position;
                    }
                }
            }

            // If no good position found, return current position
            return enemyPosition;
        }

        public Vector3 GetRandomPatrolPoint()
        {
            // Generate a random point within patrol radius
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0;
            Vector3 randomPoint = startPosition + randomDirection;

            // Find closest point on NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                // Debug visualization
                Debug.DrawRay(hit.position, Vector3.up * 5, Color.blue, 5f);
                return hit.position;
            }

            // If no valid point found, return starting position
            return startPosition;
        }

        public string GetCurrentStateName()
        {
            return currentState.GetType().Name; // Returns "IdleState", "ShootState", etc.
        }

        void OnDrawGizmos()
        {
            // Draw Detection Range (Green)
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
            Gizmos.DrawSphere(transform.position, detectionRange);

            // Draw Attack Range (Red)
            Gizmos.color = new Color(1, 0, 0, 0.3f); // Semi-transparent red
            Gizmos.DrawSphere(transform.position, attackRange);

            // Draw patrol radius (Cyan)
            if (Application.isPlaying)
            {
                Gizmos.color = new Color(0, 1, 1, 0.2f); // Semi-transparent cyan
                Gizmos.DrawSphere(startPosition, patrolRadius);
            }
            else
            {
                Gizmos.color = new Color(0, 1, 1, 0.2f); // Semi-transparent cyan
                Gizmos.DrawSphere(transform.position, patrolRadius);
            }

            // Draw line of sight ray (if player is set)
            if (player != null)
            {
                // Direction to player
                Vector3 directionToPlayer = player.position - transform.position;
                float distanceToPlayer = directionToPlayer.magnitude;

                // Draw line of sight ray
                if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer,
                        obstacleLayer))
                {
                    // Blocked line of sight (Yellow)
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, hit.point);
                }
                else
                {
                    // Clear line of sight (Blue)
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, player.position);
                }
            }
        }
    }
}