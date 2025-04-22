using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PatrolEnemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
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
        [SerializeField] private float recoveryThreshold = 30f;
        [SerializeField] private float recoveryRate = 5f;
        
        // Alert countdown
        [Header("Alert Settings")]
        [SerializeField] private float alertCountdown = 3f;
        
        // State management
        private IEnemyState currentState;
        private IdleState idleState = new IdleState();
        private AlertState alertState = new AlertState();
        private FollowState followState = new FollowState();
        private ShootState shootState = new ShootState();
        private GrenadeThrowState grenadeThrowState = new GrenadeThrowState();
        private RecoveryState recoveryState = new RecoveryState();
        
        // Public properties
        public NavMeshAgent Agent { get; private set; }
        public Transform CurrentTarget { get; private set; }
        public float CurrentHealth ;
        public int CurrentAmmo { get; set; }
        public int CurrentGrenades { get; set; }
        public float AlertTime { get; set; }
        public bool IsReloading { get; set; }
        public bool IsThrowingGrenade { get; set; }
        public bool HasLineOfSight { get; private set; }
        
        // Public accessors for ranges
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
        }
        
        private void Start()
        {
            ChangeState(idleState);
        }
        
        private void Update()
        {
            DetectPlayer();
            if (currentState != null)
            {
                currentState.UpdateState(this);
            }
        }
        
        private void DetectPlayer()
        {
            // Find all players with "Player" tag
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            if (players.Length == 0)
            {
                CurrentTarget = null;
                HasLineOfSight = false;
                return;
            }
            
            // Find closest player
            Transform closestPlayer = null;
            float closestDistance = float.MaxValue;
            
            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
            
            CurrentTarget = closestPlayer;
            
            // Check line of sight if player is within detection range
            if (CurrentTarget != null && closestDistance <= detectionRange)
            {
                // Look at player on Y axis only
                Vector3 targetPosition = CurrentTarget.position;
                targetPosition.y = transform.position.y;
                transform.LookAt(targetPosition);
                
                // Check line of sight
                Vector3 direction = (CurrentTarget.position - firePoint.position).normalized;
                RaycastHit hit;
                Debug.DrawRay(firePoint.position, direction * detectionRange, Color.red);
                
                if (Physics.Raycast(firePoint.position, direction, out hit, detectionRange, obstacleLayer))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        HasLineOfSight = true;
                    }
                    else
                    {
                        HasLineOfSight = false;
                    }
                }
                else
                {
                    HasLineOfSight = false;
                }
            }
            else
            {
                HasLineOfSight = false;
            }
        }
        
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
        
        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
            Debug.Log($"Enemy took {amount} damage. Current health: {CurrentHealth}");
            
            if (CurrentHealth <= 0)
            {
                Die();
            }
            else if (CurrentHealth < recoveryThreshold && currentState != recoveryState)
            {
                ChangeState(recoveryState);
            }
        }
        
        private void Die()
        {
            Debug.Log("Enemy died!");
            // Implement death logic here
            Destroy(gameObject);
        }
        
        public void FireBullet()
        {
            if (CurrentAmmo > 0 && !IsReloading)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                CurrentAmmo--;
                Debug.Log($"Fired bullet. Ammo remaining: {CurrentAmmo}");
                
                if (CurrentAmmo <= 0)
                {
                    StartCoroutine(ReloadWeapon());
                }
            }
        }
        
        public void ThrowGrenade()
        {
            if (CurrentGrenades > 0 && !IsThrowingGrenade)
            {
                Instantiate(grenadePrefab, grenadePoint.position, grenadePoint.rotation);
                CurrentGrenades--;
                Debug.Log($"Threw grenade. Grenades remaining: {CurrentGrenades}");
                
                StartCoroutine(GrenadeCooldown());
            }
        }
        
        private IEnumerator ReloadWeapon()
        {
            Debug.Log("Reloading weapon...");
            IsReloading = true;
            yield return new WaitForSeconds(reloadTime);
            CurrentAmmo = maxAmmo;
            IsReloading = false;
            Debug.Log("Weapon reloaded!");
        }
        
        private IEnumerator GrenadeCooldown()
        {
            IsThrowingGrenade = true;
            yield return new WaitForSeconds(grenadeThrowCooldown);
            IsThrowingGrenade = false;
        }
        
        private void OnDrawGizmos()
        {
            // Draw patrol range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, patrolRange);
            
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Let current state draw its gizmos
            if (currentState != null && Application.isPlaying)
            {
                currentState.OnDrawGizmos(this);
            }
        }
    }
}