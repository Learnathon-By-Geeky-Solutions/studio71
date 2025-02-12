using UnityEngine;
using UnityEngine.InputSystem;
using Weapon;
/// <summary>
/// Controls Attack command of the player.
/// </summary>
namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {

        public Gun _equippedGun { get; private set; }
        private void Start()
        {
            _equippedGun = GetComponent<PlayerWeaponChange>()._currentEquippedGun;
        }
        public void OnFire(InputAction.CallbackContext trigger)
        {
            _equippedGun = GetComponent<PlayerWeaponChange>()._currentEquippedGun;
            if (_equippedGun == null) { print($"No gun equipped on {gameObject.name}"); return; }

            if (!gameObject.GetComponent<PlayerSprint>().Is_Sprinting)
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
    }
}
