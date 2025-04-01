using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using SingletonManagers;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Manages all Player Input.
/// </summary>
namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        // Movement related variables
        [SerializeField] private float _moveSpeed = 5f;
        private float _currentMoveSpeed = 1f;


        //Look around related variables
        [SerializeField] private float _rotationSpeed = 5f;
        private Camera _mainCamera;
        private Plane _groundPlane;


        //Crouch related variables
        private CapsuleCollider _playerCollider;
        private bool _isCrouching;
        [Range(0.1f,1f)]
        [SerializeField] private float _crouchModifier = 1f;


        //Sprint related variables
        public bool _isSprinting { get; private set; }
        [Min(1f)]
        [SerializeField] private float _sprintModifier = 1f;


        //Gun variables
        private Weapon.Gun _equippedGun;
        private GameObject _equippedGunMagazine;

        //Grenade variables
        [SerializeField] private GameObject _grenade;
        [SerializeField] private Transform _throwPoint;
        private LineRenderer _lineRenderer;
        [SerializeField] private int _resolution = 30; // Number of points in trajectory
        [SerializeField] private float _timeStep = 0.05f; // Simulation time step
        [SerializeField] private LayerMask _collisionMask; // Stops drawing when hitting obstacles

        private bool _showingTrajectory = false; // To track visibility

        //PlayerAnimation Variable
        private PlayerAnimation _playerAnimation;
        private void OnEnable()
        {
            InputHandler.Instance.OnCrouch += Crouch;
            InputHandler.Instance.OnSprint += Sprint;
            InputHandler.Instance.OnPrimaryWeapon += PrimaryWeapon;
            InputHandler.Instance.OnSecondaryWeapon += SecondaryWeapon;
            InputHandler.Instance.OnAttack += Attack;
            InputHandler.Instance.OnReload += Reload;
            InputHandler.Instance.OnGrenade += Grenade;
        }
        private void OnDisable()
        {
            InputHandler.Instance.OnCrouch -= Crouch;
            InputHandler.Instance.OnSprint -= Sprint;
            InputHandler.Instance.OnPrimaryWeapon -= PrimaryWeapon;
            InputHandler.Instance.OnSecondaryWeapon -= SecondaryWeapon;
            InputHandler.Instance.OnAttack -= Attack;
            InputHandler.Instance.OnReload -= Reload;
            InputHandler.Instance.OnGrenade-= Grenade;
        }
        private void Awake()
        {
            //Movement variable initialization
            _currentMoveSpeed = _moveSpeed;


            //Look around variable initialization
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                print("Main Camera is Missing");
                enabled = false;
                return;
            }
            _groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0)); //Plane where the ray is hitting.


            //Crouch variable initialization
            _isCrouching = false;
            _playerCollider = GetComponent<CapsuleCollider>();


            //Sprint variable initialization
            _isSprinting = false;


            //Gun variable initialization
            _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
            _equippedGunMagazine = _equippedGun.transform.Find("Mag")?.gameObject;

            //Grenade Variable initialization
            _lineRenderer = GetComponentInChildren<LineRenderer>();
            _lineRenderer.enabled = false;
            //Animation Initialization
            _playerAnimation=GetComponent<PlayerAnimation>();
        }
        private void Start()
        {
            if (_rotationSpeed <= 0f)
            {
                print("Invalid Rotation Speed. Defaulting to 5");
                _rotationSpeed = 5f;
            }

            if (_playerCollider == null)
            {
                print($"Player named {gameObject.name} is Missing Collider");
            }

            if (_crouchModifier > 1)
            {
                print("Crouch Speed is set higher than standing speed.Setting Crouch Speed as standing speed");
                _crouchModifier = 1;
            }
        }
        private void Update()
        {
            MovePlayer();
            LookAround();
            DrawTrajectory();
            
        }


        private void MovePlayer()
        {
            transform.position += _currentMoveSpeed * Time.deltaTime * new Vector3(InputHandler.Instance.MoveDirection.x, 0, InputHandler.Instance.MoveDirection.y);
        }
        private void LookAround()
        {
            if (_playerAnimation.IsThrowingGrenade) return;
            Ray ray = _mainCamera.ScreenPointToRay(InputHandler.Instance.MousePosition);  // Ray from screen mouse position to world

            if (_groundPlane.Raycast(ray, out float Distance))                        // Checks is ray hit the ground
            {

                Vector3 mouseWorldPosition = ray.GetPoint(Distance);
                // Rotate the player to face the mouse position
                Vector3 direction = mouseWorldPosition - transform.position;
                direction.y = 0; // Ignore Y-axis for rotation
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), _rotationSpeed * Time.deltaTime);
                }

            }
        }
        private void Crouch()
        {
                  
             if (!_isCrouching)
             {
                 _playerCollider.height /= 2;
                 _playerCollider.center = new Vector3(_playerCollider.center.x, .5f, _playerCollider.center.z);
                 _isCrouching = true;
                 _currentMoveSpeed = _moveSpeed * _crouchModifier;
                _isSprinting = false;
             }
             else
             {
                 _playerCollider.height *= 2;
                 _playerCollider.center = new Vector3(_playerCollider.center.x, 0.93f, _playerCollider.center.z);
                _isCrouching = false;
                 if (Keyboard.current.shiftKey.isPressed)
                 {
                     _currentMoveSpeed = _moveSpeed * _sprintModifier;
                    _isSprinting = true;
                 }
                 else
                 {
                     _currentMoveSpeed /= _crouchModifier;
                 }
             }
            
        }
        private void Sprint(bool isPressed)
        {
            if (isPressed && !_isCrouching)
            {
                _currentMoveSpeed = _moveSpeed * _sprintModifier;
                _isSprinting = true;
                _equippedGun.StopShooting();
            }
            else if (!_isCrouching)
            {
                _currentMoveSpeed /= _sprintModifier;
                _isSprinting = false;
            }
        }
        private void Attack(bool isPressed)
        {

            if (_isSprinting || _playerAnimation.IsBusy) return;       
                if (isPressed)
                {
                    _equippedGun.StartShooting();
                }
                else
                {
                    _equippedGun.StopShooting();
                }
            
        }
        private void PrimaryWeapon()
        {
            if (!_equippedGun.IsShooting)
            {
                _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
            }
        }
        private void SecondaryWeapon()
        {
            if (!_equippedGun.IsShooting)
            {
                _equippedGun = gameObject.GetComponentInChildren<Weapon.SemiAutomaticGun>();
            }
        }
        private void Reload()
        {        
             _equippedGun.StopShooting();
            StartCoroutine(DelayedAction(1f, () => { _equippedGunMagazine.SetActive(false); }));
            StartCoroutine(DelayedAction(2f, () =>
            { _equippedGun.CurrentMagazineSize = _equippedGun.Magazine_Size; _equippedGunMagazine.SetActive(true); }));            
        }
        private void Grenade()
        {
            if (_playerAnimation.IsThrowingGrenade) return; // Prevents throwing if grenade animation is already playing
            _playerAnimation.IsThrowingGrenade = true;
            _equippedGun.StopShooting();
            StartCoroutine(DelayedAction(1.7f,
                () => { Instantiate(_grenade, _throwPoint.position, _grenade.transform.rotation); }));
        }
        private void DrawTrajectory()
        {
            if (!InputHandler.Instance.GrenadeThrowStart)
            {
                _lineRenderer.enabled = false;
                //_showingTrajectory = false;
                return;
            }
            _lineRenderer.enabled = true;
            //_showingTrajectory = true;

            List<Vector3> points = new List<Vector3>();
            Vector3 startPosition = _throwPoint.position;
            Vector3 startVelocity = (transform.forward * 5) + (Vector3.up * 6); // Use the same force as the real throw

            points.Add(startPosition); // Ensure there's always at least one point

            for (float t = 0; t < _resolution * _timeStep; t += _timeStep)
            {
                Vector3 point = startPosition + startVelocity * t + 0.5f * Physics.gravity * t * t;
                points.Add(point);

                // Ensure we have at least 2 points before checking for collisions
                if (points.Count > 1)
                {
                    Vector3 lastPoint = points[^2];
                    Vector3 direction = (point - lastPoint).normalized;
                    float distance = (point - lastPoint).magnitude;

                    if (Physics.Raycast(lastPoint, direction, out RaycastHit hit, distance, _collisionMask))
                    {
                        points.Add(hit.point);
                        break;
                    }
                }
            }

            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());
            
        }

        private static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
