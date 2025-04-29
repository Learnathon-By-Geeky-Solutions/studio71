using UnityEngine;
using UnityEngine.AI;
using System;
using PatrolEnemy;
using HealthSystem;
using Ink.Runtime;

namespace PatrolEnemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyStateType
        {
            Idle,
            Alert,
            Follow,
            Shoot,
            GrenadeThrow,
            Recovery,
            Death

        }

        // References
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform grenadePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject grenadePrefab;

        public Health _health{get; private set;}

        private IdleState IdleStatVar;

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
        [SerializeField] private float recoveryThreshold=0f;
        [SerializeField] private int _recoveryRate;

        [Header("Alert Settings")]
        [SerializeField] private float alertCountdown = 3f;

        public IEnemyState currentState;
        private IdleState idleState = new IdleState();
        private AlertState alertState = new AlertState();
        private FollowState followState = new FollowState();
        private ShootState shootState = new ShootState();
        private GrenadeThrowState grenadeThrowState = new GrenadeThrowState();
        private RecoveryState recoveryState = new RecoveryState();

        private DeathState deathState = new DeathState();

        public NavMeshAgent Agent ;
        public Transform CurrentTarget { get; private set;}
        public int CurrentAmmo { get; set;}
        public int CurrentGrenades { get; set;}
        public float AlertTime { get; set;}
        public bool IsReloading { get; set;}
        public bool IsIdleAlert = false;
        public bool IsRecoverReturing { get; set;}
        public bool IsNotGrenadeThrowing { get; set;}


        public bool HasLineOfSight { get; private set; }
        public Vector3 InitialPosition { get; private set; }

         private bool isAlert;
         private bool isShooting;
         private bool isRecovering;
         private bool isIdle;
         private bool isFollowing;
         private bool isThrowingGrenade;
         private bool _isDead;

        //Public Access Modifiers
        public float PatrolRange => patrolRange;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float RecoveryThreshold => recoveryThreshold;
        public float RecoveryRate => _recoveryRate;
        public Transform FirePoint => firePoint;
        public Transform GrenadePoint => grenadePoint;
        public GameObject BulletPrefab => bulletPrefab;
        public GameObject GrenadePrefab => grenadePrefab;
        public float ReloadTime => reloadTime;
        public float GrenadeThrowCooldown => grenadeThrowCooldown;
        public int MaxAmmo => maxAmmo;
        public bool _isFollowing => isFollowing;
        public bool _isShooting => isShooting;
        public bool _isIdle => isIdle;
        public bool _isRecovering => isRecovering;
        public bool _isAlert => isAlert;
        public bool _isThrowingGrenade => isThrowingGrenade;
        public bool IsDead=>_isDead;


        // Event for state change
        public event Action<EnemyStateType> OnStateChanged;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
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
            currentState?.UpdateState(this);
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
                case EnemyStateType.Death:
                    newState = deathState;
                    break;
                default:
                    newState = idleState;
                    break;
            }

            currentState?.ExitState(this);
            currentState = newState;
            currentState?.EnterState(this);

            // Update booleans
            isAlert = stateType == EnemyStateType.Alert;
            isShooting = stateType == EnemyStateType.Shoot;
            isRecovering = stateType == EnemyStateType.Recovery;
            isIdle = stateType == EnemyStateType.Idle;
            isFollowing = stateType == EnemyStateType.Follow;
            isThrowingGrenade = stateType == EnemyStateType.GrenadeThrow;
            

        }

        public void TakeDamage()
        {
            if (_health.CurrentHealth <= 0)
            {
                _isDead=true;
               ChangeState(EnemyStateType.Death);
            }
            else if (_health.CurrentHealth < recoveryThreshold && !(currentState is RecoveryState))
            {
                ChangeState(EnemyStateType.Recovery);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Application.isPlaying ? InitialPosition : transform.position, patrolRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

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
