using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Manages all Player Input.
/// </summary>
namespace Player
{
    public class PlayerInputManager : MonoBehaviour
    {
        // Movement related variables
        [SerializeField] private float _moveSpeed = 5f;
        private float _currentMoveSpeed = 1f;
        private InputAction _moveAction;


        //Look around related variables
        [SerializeField] private float _rotationSpeed = 5f;
        private Camera _mainCamera;
        private Plane _groundPlane;


        //Crouch related variables
        private CapsuleCollider _playerCollider;
        private bool _isCrouching;
        [Min(0.1f)]
        [SerializeField] private float _crouchModifier = 1f;


        //Sprint related variables
        private bool _isSprinting;
        [Min(1f)]
        [SerializeField] private float _sprintModifier = 1f;


        //Weapon variable
        private Weapon.Gun _equippedGun;
        private void Awake()
        {
            //Movement variable initialization
            PlayerInput _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions.FindAction("Move");
            if (_playerInput == null) { print($"Input System is missing on {gameObject.name}"); }
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


            //Weapon initialization
            _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
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
        }


        private void MovePlayer()
        {
            Vector2 Direction = _moveAction.ReadValue<Vector2>();
            transform.position += new Vector3(Direction.x, 0, Direction.y) * _currentMoveSpeed * Time.deltaTime;
        }
        private void LookAround()
        {
            if (Mouse.current == null) return;
            Vector2 MousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(MousePosition);                  // Ray from screen mouse position to world

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
        public void Crouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (!_isCrouching)
                {
                    _playerCollider.height /= 2;
                    _playerCollider.center = new Vector3(_playerCollider.center.x, -.5f, _playerCollider.center.z);
                    _isCrouching = true;
                    _currentMoveSpeed = _moveSpeed * _crouchModifier;
                }
                else
                {
                    _playerCollider.height *= 2;
                    _playerCollider.center = new Vector3(_playerCollider.center.x, 0, _playerCollider.center.z);
                    _isCrouching = false;
                    if (Keyboard.current.shiftKey.isPressed)
                    {
                        _currentMoveSpeed = _moveSpeed * _sprintModifier;
                    }
                    else
                    {
                        _currentMoveSpeed /= _crouchModifier;
                    }
                }
            }
        }
        public void Sprint(InputAction.CallbackContext context)
        {
            if (context.performed && !_isCrouching)
            {
                _currentMoveSpeed = _moveSpeed * _sprintModifier;
                _isSprinting = true;
                _equippedGun.StopShooting();
            }
            else if (context.canceled && !_isCrouching)
            {
                _currentMoveSpeed /= _sprintModifier;
                _isSprinting = false;
            }
        }
        public void Attack(InputAction.CallbackContext trigger)
        {

            if (!_isSprinting)
            {
                if (trigger.started || trigger.performed)
                {
                    _equippedGun.StartShooting();
                }
                else if (trigger.canceled)
                {
                    _equippedGun.StopShooting();
                }
            }
        }
        public void PrimaryWeapon(InputAction.CallbackContext context)
        {
            if (context.performed && !_equippedGun.IsShooting)
            {
                _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
            }
        }
        public void SecondaryWeapon(InputAction.CallbackContext context)
        {
            if (context.performed && !_equippedGun.IsShooting)
            {
                _equippedGun = gameObject.GetComponentInChildren<Weapon.SemiAutomaticGun>();
            }
        }
        public void Reload(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _equippedGun.StopShooting();
                _equippedGun.CurrentMagazineSize = _equippedGun.Magazine_Size;
            }

        }
    }
}
