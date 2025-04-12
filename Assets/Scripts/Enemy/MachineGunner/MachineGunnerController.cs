using UnityEngine;
using System.Collections;

namespace MachineGunner
{
    #region Interfaces and Enums

    public interface IMachineGunnerState
    {
        void EnterState(MachineGunnerController controller);
        void UpdateState(MachineGunnerController controller);
        void ExitState(MachineGunnerController controller);
    }

    public enum MachineGunnerStateType
    {
        Idle,
        Alert,
        Suppress,
        Shoot,
        OverheatAndReload,
        Death
    }

    #endregion

    public class MachineGunnerController : MonoBehaviour
    {
        #region Public Variables

        [Header("Ranges")]
        public float alertRange = 10f;
        public float suppressiveRange = 15f;
        public float shootRange = 7f;

        [Header("Shooting Configuration")]
        public GameObject bulletPrefab;
        public Transform firePoint;
        public float fireRate = 0.1f;
        public float burstDuration = 2f; // Duration of a single burst in ShootState
        public float suppressiveBurstDuration = 3f;
        public float overheatThreshold = 10f;
        public float coolingRate = 2f;
        public float reloadTime = 3f;
        public int magazineSize = 30;
        public float suppressiveFireSpreadAngle = 15f; // Angle for bullet spread in suppressive fire
        public float bulletForce = 10f;

        [Header("Detection")]
        public string playerTag = "Player";
        public LayerMask lineOfSightMask;

        [Header("Gizmo Colors")]
        public Color alertGizmoColor = Color.yellow;
        public Color suppressiveGizmoColor = Color.orange;
        public Color shootGizmoColor = Color.red;

        #endregion

        #region Private Variables

        private IMachineGunnerState _currentState;
        private GameObject _player;
        private float _timeSinceLastShot = 0f;
        private float _currentHeat = 0f;
        private bool _isOverheated = false;
        private float _overheatStartTime;
        private float _reloadStartTime;
        private int _currentAmmo;

        #endregion

        #region Properties

        public IMachineGunnerState CurrentState => _currentState;
        public GameObject Player => _player;
        public float TimeSinceLastShot => _timeSinceLastShot;
        public float CurrentHeat => _currentHeat;
        public bool IsOverheated => _isOverheated;
        public int CurrentAmmo => _currentAmmo;

        #endregion

        #region Unity Callbacks

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag(playerTag);
            _currentState = new IdleState();
            _currentState.EnterState(this);
            _currentAmmo = magazineSize;
        }

        void Update()
        {
            if (_player == null) return; // Player might be destroyed

            _currentState?.UpdateState(this);
            _timeSinceLastShot += Time.deltaTime;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = alertGizmoColor;
            Gizmos.DrawWireSphere(transform.position, alertRange);

            Gizmos.color = suppressiveGizmoColor;
            Gizmos.DrawWireSphere(transform.position, suppressiveRange);

            Gizmos.color = shootGizmoColor;
            Gizmos.DrawWireSphere(transform.position, shootRange);
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
            return Vector3.Distance(transform.position, _player.transform.position) <= alertRange;
        }

        public bool IsPlayerInSuppressiveRange()
        {
            return Vector3.Distance(transform.position, _player.transform.position) <= suppressiveRange;
        }

        public bool IsPlayerInShootRange()
        {
            return Vector3.Distance(transform.position, _player.transform.position) <= shootRange;
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
                    Vector3 direction = (targetPosition - firePoint.position).normalized;
                    if (isSuppressive)
                    {
                        direction = Quaternion.Euler(0f, Random.Range(-suppressiveFireSpreadAngle, suppressiveFireSpreadAngle), 0f) * direction;
                    }
                    rb.velocity = direction * bulletForce;
                }
                _timeSinceLastShot = 0f;
                _currentAmmo--;
                _currentHeat += 1f;
                if (_currentHeat >= overheatThreshold)
                {
                    _isOverheated = true;
                    _overheatStartTime = Time.time;
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
            _overheatStartTime = Time.time;
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

        public void Die()
        {
            Debug.Log("Machine Gunner Died!");
            // Implement death effects, like explosion
            Destroy(gameObject);
        }

        #endregion
    }
}