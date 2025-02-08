using UnityEngine;
using UnityEngine.InputSystem;
using Player;
/// <summary>
/// Crouch logic for player.
/// </summary>
namespace Player
{
    public class PlayerCrouch : MonoBehaviour
    {
        private CapsuleCollider _playerCollider;
        private bool _isCrouching = false;
        private PlayerMovement _playerMovement;
        [Min(0.1f)]
        [SerializeField] private float _crouchModifier = 1f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _playerCollider = GetComponent<CapsuleCollider>();
            _playerMovement = GetComponent<PlayerMovement>();
        }
        void Start()
        {
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
        public void Crouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (!_isCrouching)
                {
                    _playerCollider.height = _playerCollider.height / 2;
                    _playerCollider.center = new Vector3(_playerCollider.center.x, -.5f, _playerCollider.center.z);
                    _isCrouching = true;
                    _playerMovement.Move_Speed = _playerMovement.Move_Speed * _crouchModifier;
                }
                else
                {
                    _playerCollider.height = _playerCollider.height * 2;
                    _playerCollider.center = new Vector3(_playerCollider.center.x, 0, _playerCollider.center.z);
                    _isCrouching = false;
                    _playerMovement.Move_Speed = _playerMovement.Move_Speed / _crouchModifier;
                }
            }
        }
    }
}