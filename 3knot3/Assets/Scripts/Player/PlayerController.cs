using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using SingletonManagers;
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
        private bool _isSprinting;
        [Min(1f)]
        [SerializeField] private float _sprintModifier = 1f;


        //Weapon variable
        private Weapon.Gun _equippedGun;

        //Player Animator
        private Animator _playerAnimator;
        private string _currentAnimation;

        private void OnEnable()
        {
            InputHandler.Instance.OnCrouch += Crouch;
            InputHandler.Instance.OnSprint += Sprint;
            InputHandler.Instance.OnPrimaryWeapon += PrimaryWeapon;
            InputHandler.Instance.OnSecondaryWeapon += SecondaryWeapon;
            InputHandler.Instance.OnAttack += Attack;
            InputHandler.Instance.OnReload += Reload;
        }
        private void OnDisable()
        {
            InputHandler.Instance.OnCrouch -= Crouch;
            InputHandler.Instance.OnSprint -= Sprint;
            InputHandler.Instance.OnPrimaryWeapon -= PrimaryWeapon;
            InputHandler.Instance.OnSecondaryWeapon -= SecondaryWeapon;
            InputHandler.Instance.OnAttack -= Attack;
            InputHandler.Instance.OnReload -= Reload;
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


            //Weapon initialization
            _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();

            //Animator Initialization
            _playerAnimator = GetComponent<Animator>();
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
            transform.position += _currentMoveSpeed * Time.deltaTime * new Vector3(InputHandler.Instance.MoveDirection.x, 0, InputHandler.Instance.MoveDirection.y);
            
        }
        private void LookAround()
        {
            Ray ray = _mainCamera.ScreenPointToRay(InputHandler.Instance.MousePosition);                  // Ray from screen mouse position to world

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

            if (!_isSprinting)
            {
                if (isPressed)
                {
                    _equippedGun.StartShooting();
                }
                else
                {
                    _equippedGun.StopShooting();
                }
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
             _equippedGun.CurrentMagazineSize = _equippedGun.Magazine_Size;
            
        }

        void ChangeAnimationState(string newState)
        {
            if (_currentAnimation == newState) return;

            _playerAnimator.Play(newState);

            _currentAnimation = newState;
        }
    }
}
