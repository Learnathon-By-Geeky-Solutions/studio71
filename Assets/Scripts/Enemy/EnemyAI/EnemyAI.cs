using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace patrolEnemy
{
    public class EnemyAI : MonoBehaviour
    {

        //Todo: This class handles state logic, combat, pathfinding, grenade logic, line of sight, cover detection, patrol logic, and visualization.
        // Split into separate classes/services: EnemyCombatHandler, EnemyAwareness, EnemyPatrolSystem, EnemyStateMachine
    
        // State Machine
        private IEnemyState currentState;
        public IdleState idleState{ get; private set; }
        public AlertState alertState{ get; private set; }
        public FollowState followState{ get; private set; }
        public ShootState shootState{ get; private set; }
        public GrenadeThrowState grenadeThrowState{ get; private set; }
        public RecoverState recoverState{ get; private set; }

        // References
        public Transform player{ get; private set; }
        public NavMeshAgent navMeshAgent{ get; private set; }
        [SerializeField]public Transform firePoint{ get; private set; }
        [SerializeField]public Transform grenadeThrowPoint{ get; private set; }

        // Detection and Attack
        [SerializeField]public float detectionRange{ get; private set; } = 15f;  //todo: You're exposing many variables publicly just to get/set them within the class or inspector.
                                                                                // Use [SerializeField] private for variables only meant for the Inspector. Expose them via public float DetectionRange => detectionRange; only if needed.
        [SerializeField]public float attackRange { get; private set; } = 8f;
        [SerializeField]public LayerMask  obstacleLayer{ get; private set; }

        // Health System
        [SerializeField]public float maxHealth{ get; private set; } = 100f;
        [SerializeField]public float currentHealth{ get; set; }
        [SerializeField]public float recoveryThreshold{ get; private set; } = 30f;
        [SerializeField]public float recoveryRate { get; private set; }= 10f;

        // Shooting
        [SerializeField]public GameObject bulletPrefab{ get; private set; }
        [SerializeField]public float fireRate { get; private set; }= 1f;
        public int maxAmmo { get; private set; }= 30;
        public int currentAmmo{ get; private set; }
        public float reloadTime{ get; private set; } = 2f;
        public bool isReloading { get; private set; }= false;

        // Grenade
        [SerializeField]public GameObject grenadePrefab{ get; private set; }
        public int maxGrenades { get; private set; }= 3;
        public int currentGrenades{ get; private set; }
        public float grenadeThrowCooldown { get; private set; }= 5f;
        public bool canThrowGrenade { get; private set; }= true;

        // Alert State
        public float alertCountdown { get; private set; }= 3f;
        public float currentAlertTime { get; set; } = 0f;

        // Random Patrol
        public float patrolRadius { get; private set; } = 10f;
        public Vector3 startPosition{ get; private set; }
        private Vector3 currentPatrolDestination;

        // Status flags
        public bool playerInDetectionRange { get; private set; }= false;
        public bool playerInAttackRange { get; private set; }= false;
        public bool playerInLineOfSight { get; private set; }= false;

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
            if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer,
                    obstacleLayer))
            {
                return false; // Something is blocking the line of sight
            }

            return true; // Clear line of sight
        }
        
        public void Shoot()
        {
            if (currentAmmo <= 0 || isReloading) return;
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

            yield return new WaitForSeconds(reloadTime);        //use unitask instead of coroutine, cz coroutine are bound to monobehaviour which can block logic flow and hard to cancel like this method Reload
                                                                //convert into async unitask 

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
            GameObject grenade = Instantiate(grenadePrefab, grenadeThrowPoint.position, Quaternion.identity);  //Todo: Nope use object pooling here, this could messed up ur performance ,Use Object Pooling for bullets and grenades.
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
            return currentState.GetType().Name; // Returns "IdleState", "ShootState", etc.   //Todo: using reflection is costly, my suggestion is let IEnemeyState define a string StateName {get;} property 
                                                // ,then implement in state like public string StateName => "IdleState";
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
