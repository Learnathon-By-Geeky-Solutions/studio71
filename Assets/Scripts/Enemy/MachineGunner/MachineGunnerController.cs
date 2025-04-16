using UnityEngine;
using MachineGunner.States; // Ensure you have this using statement

namespace MachineGunner
{
    public class MachineGunnerController : MonoBehaviour
    {
        #region Public Variables

        [Header("Ranges")]
        public float alertRange { get; private set; }= 10f;

        private float SuppressiveRange { get; } = 15f;
        private readonly float _shootRange = 7f;

        [Header("Shooting Configuration")]
        public GameObject bulletPrefab{ get; private set; }
        public Transform firePoint{ get; private set; }
        public float fireRate { get; private set; }= 0.1f;
        public float burstDuration { get; private set; }= 2f; // Duration of a single burst in ShootState
        public float suppressiveBurstDuration { get; private set; }= 3f;
        public float overheatThreshold { get; private set; }= 10f;
        public float coolingRate { get; private set; }= 2f;
        public float reloadTime { get; private set; }= 3f;
        public int magazineSize { get; private set; }= 30;
        public float suppressiveFireSpreadAngle { get; private set; }= 15f; // Angle for bullet spread in suppressive fire
        public float bulletForce { get; private set; }= 10f;

        [Header("Detection")]
        public string playerTag { get; private set; } = "Player";
        public LayerMask lineOfSightMask{ get; private set; }

        [Header("Gizmo Colors")] 
        private readonly Color alertGizmoColor = Color.yellow;
        private Color suppressiveGizmoColor = Color.blue;
        private Color shootGizmoColor = Color.red;
        private Color lineOfSightColor = Color.green;
        private Color noLineOfSightColor = Color.magenta;
        private Color suppressiveArcColor = new Color(1f, 0.5f, 0f, 0.5f); // Orange with alpha

        [Header("Idle Rotation")]
        public float idleRotationSpeed { get; private set; }= 10f;
        [Range(0f, 180f)]
        public float idleRotationAngle { get; private set; }= 60f; // Total angle of rotation
        public float idleRotationOffset{ get; private set; } = 0f; // Starting offset for the rotation

        [Header("Shoot State")]
        [Range(0f, 90f)]
        public float burstSweepAngle { get; private set; } = 30f; // Total sweep angle during burst
        public float burstSweepSpeed { get; private set; } = 60f; // Degrees per second of sweep

        #endregion

        #region Private Variables

        private IMachineGunnerState _currentState;
        private GameObject _player;
        private float _timeSinceLastShot = 0f;
        private float _currentHeat = 0f;
        private bool _isOverheated = false;
        private float _reloadStartTime;
        private int _currentAmmo;
        private Vector3 _lastKnownPlayerPosition;

        #endregion

        #region Properties

        public IMachineGunnerState CurrentState => _currentState;
        public GameObject Player => _player;
        public float TimeSinceLastShot => _timeSinceLastShot;
        public float CurrentHeat => _currentHeat;
        public bool IsOverheated => _isOverheated;
        public int CurrentAmmo => _currentAmmo;
        public float SuppressiveFireSpreadAngle => suppressiveFireSpreadAngle; // Expose for gizmo
        public float BurstSweepAngle => burstSweepAngle; // Expose for ShootState
        public float BurstSweepSpeed => burstSweepSpeed; // Expose for ShootState

        public Color AlertGizmoColor => alertGizmoColor;

        public float AlertRange
        {
            get => alertRange;
            set => alertRange = value;
        }

        #endregion

        #region Unity Callbacks

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag(playerTag);
            _currentState = new IdleState();
            _currentState.EnterState(this);
            _currentAmmo = magazineSize;
            _lastKnownPlayerPosition = transform.forward; // Default forward if no player yet
            idleRotationOffset = transform.eulerAngles.y; // Initialize offset with current Y rotation
        }

        void Update()
        {
            if (_player != null)
            {
                _lastKnownPlayerPosition = _player.transform.position;
            }

            _currentState?.UpdateState(this);
            _timeSinceLastShot += Time.deltaTime;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = AlertGizmoColor;
            Gizmos.DrawWireSphere(transform.position, AlertRange);

            Gizmos.color = suppressiveGizmoColor;
            Gizmos.DrawWireSphere(transform.position, SuppressiveRange);

            Gizmos.color = shootGizmoColor;
            Gizmos.DrawWireSphere(transform.position, _shootRange);

            // Line of Sight Gizmo
            Gizmos.color = HasLineOfSightToPlayer() ? lineOfSightColor : noLineOfSightColor;
            Gizmos.DrawLine(firePoint.position, _lastKnownPlayerPosition);

            // Suppressive Fire Arc Gizmo
            Gizmos.color = suppressiveArcColor;
            Vector3 forwardDirection = transform.forward;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-suppressiveFireSpreadAngle / 2f, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(suppressiveFireSpreadAngle / 2f, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * forwardDirection;
            Vector3 rightRayDirection = rightRayRotation * forwardDirection;

            Gizmos.DrawRay(firePoint.position, leftRayDirection * SuppressiveRange);
            Gizmos.DrawRay(firePoint.position, rightRayDirection * SuppressiveRange);
            Gizmos.DrawWireSphere(transform.position + forwardDirection * SuppressiveRange, 0.5f); // Indicate end of range

            // Idle Rotation Gizmo (visualize the angle)
            Gizmos.color = Color.cyan;
            Quaternion initialRotation = Quaternion.Euler(0f, idleRotationOffset - idleRotationAngle / 2f, 0f);
            Quaternion finalRotation = Quaternion.Euler(0f, idleRotationOffset + idleRotationAngle / 2f, 0f);
            Vector3 initialDirection = initialRotation * Vector3.forward * 2f;
            Vector3 finalDirection = finalRotation * Vector3.forward * 2f;
            Gizmos.DrawRay(transform.position, initialDirection);
            Gizmos.DrawRay(transform.position, finalDirection);

            // Burst Sweep Gizmo
            Gizmos.color = Color.yellow;
            Vector3 currentForward = transform.forward;
            Quaternion sweepLeft = Quaternion.AngleAxis(-burstSweepAngle / 2f, Vector3.up);
            Quaternion sweepRight = Quaternion.AngleAxis(burstSweepAngle / 2f, Vector3.up);
            Gizmos.DrawRay(firePoint.position, sweepLeft * currentForward * _shootRange * 0.75f); // Slightly shorter to visualize
            Gizmos.DrawRay(firePoint.position, sweepRight * currentForward * _shootRange * 0.75f);
        }

        #endregion

        #region State Management

        public void SwitchState(IMachineGunnerState newState)
        {
            _currentState?.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }

        #endregion

        #region Detection Methods

        public bool IsPlayerInAlertRange()
        {
            return Vector3.Distance(transform.position, _player.transform.position) <= AlertRange;
        }

        public bool IsPlayerInSuppressiveRange()
        {
            return Vector3.Distance(transform.position, _player.transform.position) <= SuppressiveRange;
        }

        public bool IsPlayerInShootRange()
        {
            return Vector3.Distance(transform.position, _player.transform.position) <= _shootRange;
        }

        public bool HasLineOfSightToPlayer()
        {
            if (_player == null) return false;
            Vector3 directionToPlayer = _player.transform.position - transform.position;
            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, directionToPlayer.normalized, out hit, Mathf.Infinity, lineOfSightMask))
            {
                return hit.collider.CompareTag(playerTag);
            }
            return false;
        }

        #endregion

        #region Shooting Methods

        public void ShootBullet(Vector3 targetPosition, bool isSuppressive = false)
        {
            if (_currentAmmo > 0 && !_isOverheated && _timeSinceLastShot >= fireRate)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = firePoint.forward; // Use the firePoint's current forward direction
                    if (isSuppressive)
                    {
                        direction = Quaternion.Euler(0f, Random.Range(-suppressiveFireSpreadAngle, suppressiveFireSpreadAngle), 0f) * direction;
                    }
                    rb.linearVelocity = direction * bulletForce;
                }
                _timeSinceLastShot = 0f;
                _currentAmmo--;
                _currentHeat += 1f;
                if (_currentHeat >= overheatThreshold)
                {
                    _isOverheated = true;
                    SwitchState(new OverheatAndReloadState());
                }
            }
            else if (_currentAmmo <= 0 && !_isOverheated)
            {
                SwitchState(new OverheatAndReloadState()); // Force reload if out of ammo
            }
        }

        public void StartCoolingAndReloading()
        {
            _isOverheated = true;
            _reloadStartTime = Time.time;
            _currentAmmo = 0; // Immediately set ammo to 0 when overheating
        }

        public void UpdateCoolingAndReloading()
        {
            if (_isOverheated)
            {
                _currentHeat -= coolingRate * Time.deltaTime;
                _currentHeat = Mathf.Max(0f, _currentHeat); // Ensure heat doesn't go below zero

                if (Time.time >= _reloadStartTime + reloadTime)
                {
                    _currentAmmo = magazineSize;
                }

                if (_currentHeat <= 0f && _currentAmmo == magazineSize)
                {
                    _isOverheated = false;
                    // Decide which state to go back to based on player's position
                    if (IsPlayerInShootRange())
                    {
                        SwitchState(new ShootState());
                    }
                    else if (IsPlayerInSuppressiveRange())
                    {
                        SwitchState(new SuppressState());
                    }
                    else if (IsPlayerInAlertRange())
                    {
                        SwitchState(new AlertState());
                    }
                    else
                    {
                        SwitchState(new IdleState());
                    }
                }
            }
        }
        

        #endregion
    }
}