using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Sprint Logic of the player.
/// </summary>
namespace Player
{
    public class PlayerSprint : MonoBehaviour
    {
        public bool Is_Sprinting { get; private set; }
        private PlayerMovement _playerMovement;
        [Min(1f)]
        [SerializeField] private float _sprintModifier = 1f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            Is_Sprinting = false;
            _playerMovement = GetComponent<PlayerMovement>();
        }

        // Update is called once per frame
        public void Sprint(InputAction.CallbackContext context)
        {
            if (context.performed && !gameObject.GetComponent<PlayerCrouch>()._isCrouching)
            {
                _playerMovement.Move_Speed = _playerMovement.Move_Speed * _sprintModifier;
                Is_Sprinting = true;
                gameObject.GetComponent<PlayerAttack>()._equippedGun.StopShooting();
            }else if (context.canceled && !gameObject.GetComponent<PlayerCrouch>()._isCrouching)
            {
                _playerMovement.Move_Speed = _playerMovement.Move_Speed / _sprintModifier;
                Is_Sprinting=false;
            }
        }
    }
}
