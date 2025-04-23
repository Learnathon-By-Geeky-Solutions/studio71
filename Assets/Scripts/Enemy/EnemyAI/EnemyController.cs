using UnityEngine;
using UnityEngine.AI;

namespace PatrolEnemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        // State types enum
        public enum EnemyStateType
        {
            Idle,
            Alert,
            Follow,
            Shoot,
            GrenadeThrow,
            Recovery
        }

        // References
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform grenadePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject grenadePrefab;

        // Detection ranges
        [Header("Detection Settings")]
        [SerializeField] private float patrolRange = 10f;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private LayerMask obstacleLayer;

        // Combat settings
        [Header("Combat Settings")]
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private float reloadTime = 2f;
        [SerializeField] private int maxGrenades = 3;
        [SerializeField] private float grenadeThrowCooldown = 5f;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float recoveryThreshold = 30f;
        [SerializeField] private float recoveryRate = 5f;

        // Alert countdown
        [Header("Alert Settings")]
        [SerializeField] private float alertCountdown = 3f;

        // State management
        private IEnemyState currentState;
        private readonly IdleState idleState = new IdleState();
        private readonly AlertState alertState = new AlertState();
        private readonly FollowState followState = new FollowState();
        private readonly ShootState shootState = new ShootState();
        private readonly GrenadeThrowState grenadeThrowState = new GrenadeThrowState();
        private readonly RecoveryState recoveryState = new RecoveryState();

        // Public properties
        public NavMeshAgent Agent { get; private set; }
        public Transform CurrentTarget { get; private set; }
        public float CurrentHealth
        {
            get { return currentHealth; }
            set { currentHealth = Mathf.Clamp(value, 0, maxHealth); } // Ensure health stays within bounds
        }
        public int CurrentAmmo { get; set; }
        public int CurrentGrenades { get; set; }
        public float AlertTime { get; set; }
        public bool IsReloading { get; set; }
        public bool IsThrowingGrenade { get; set; }
        public bool HasLineOfSight { get; private set; }
        public Vector3 InitialPosition { get; private set; }

        // Public accessors for ranges and settings
        public float PatrolRange => patrolRange;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float MaxHealth => maxHealth;
        public float RecoveryThreshold => recoveryThreshold;
        public float RecoveryRate => recoveryRate;
        public Transform FirePoint => firePoint;
        public Transform GrenadePoint => grenadePoint;
        public GameObject BulletPrefab => bulletPrefab;
        public GameObject GrenadePrefab => grenadePrefab;
        public float ReloadTime => reloadTime;
        public float GrenadeThrowCooldown => grenadeThrowCooldown;
        public int MaxAmmo => maxAmmo;
        public int MaxGrenades => maxGrenades;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            CurrentHealth = maxHealth;
            CurrentAmmo = maxAmmo;
            CurrentGrenades = maxGrenades;
            AlertTime = alertCountdown;
            InitialPosition = transform.position;
        }

        private void Start()
        {
            ChangeState(EnemyStateType.Idle);
        }

        private void Update()
        {
            DetectPlayer();
            if (currentState != null)
            {
                currentState.UpdateState(this);
            }

            TakeDamage();
        }
private void DetectPlayer()
{
    GameObject[] players = FindActivePlayers();
    if (players.Length == 0)
    {
        CurrentTarget = null;
        HasLineOfSight = false;
        return;
    }

    Transform closestPlayer = FindClosestPlayer(players);
    CurrentTarget = closestPlayer;

    if (CurrentTarget == null || Vector3.Distance(transform.position, CurrentTarget.position) > detectionRange)
    {
        HasLineOfSight = false;
        CurrentTarget = null;
        return;
    }

    HasLineOfSight = CheckLineOfSight(CurrentTarget);
}

private GameObject[] FindActivePlayers()
{
    return GameObject.FindGameObjectsWithTag("Player");
}

private Transform FindClosestPlayer(GameObject[] players)
{
    Transform closest = null;
    float closestDistance = float.MaxValue;

    foreach (GameObject playerObject in players)
    {
        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closest = playerObject.transform;
        }
    }
    return closest;
}

private bool CheckLineOfSight(Transform target)
{
    Vector3 targetPosition = target.position;
    targetPosition.y = transform.position.y;
    transform.LookAt(targetPosition);

    Vector3 direction = (target.position - firePoint.position).normalized;
    RaycastHit hit;
    Debug.DrawRay(firePoint.position, direction * detectionRange, Color.red);

    if (Physics.Raycast(firePoint.position, direction, out hit, detectionRange, obstacleLayer))
    {
        return hit.transform.IsChildOf(target) || hit.transform == target;
    }

    return true;
}

        public void ChangeState(EnemyStateType stateType)
        {
            IEnemyState newState;

            switch (stateType)
            {
                case EnemyStateType.Idle:
                    newState = idleState;
                    break;
                case EnemyStateType.Alert:
                    newState = alertState;
                    break;
                case EnemyStateType.Follow:
                    newState = followState;
                    break;
                case EnemyStateType.Shoot:
                    newState = shootState;
                    break;
                case EnemyStateType.GrenadeThrow:
                    newState = grenadeThrowState;
                    break;
                case EnemyStateType.Recovery:
                    newState = recoveryState;
                    break;
                default:
                    newState = idleState;
                    break;
            }

            if (currentState != null)
            {
                currentState.ExitState(this);
            }

            currentState = newState;

            if (currentState != null)
            {
                Debug.Log($"Changing to {currentState.GetType().Name}");
                currentState.EnterState(this);
            }
        }

        // For backward compatibility with existing state scripts
        public void ChangeState(IEnemyState newState)
        {
            if (currentState != null)
            {
                currentState.ExitState(this);
            }

            currentState = newState;

            if (currentState != null)
            {
                Debug.Log($"Changing to {currentState.GetType().Name}");
                currentState.EnterState(this);
            }
        }

        public void TakeDamage()
        {
            
            
            if (CurrentHealth <= 0)
            {
                //Die
            }
            else if (CurrentHealth < recoveryThreshold && !(currentState is RecoveryState))
            {
                ChangeState(EnemyStateType.Recovery);
               
            }
        }

        

        private void OnDrawGizmos()
        {
            // Draw patrol range
            Gizmos.color = Color.green;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(InitialPosition, patrolRange);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, patrolRange);
            }

            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

             if (CurrentTarget != null)
    {
        Gizmos.color = HasLineOfSight ? Color.green : Color.yellow;
        Gizmos.DrawLine(firePoint.position, CurrentTarget.position);
    }

        }

    }
}