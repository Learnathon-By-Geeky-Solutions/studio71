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

        private Gun _equippedGun;
        public void OnFire(InputAction.CallbackContext trigger)
        {
            _equippedGun = GetComponent<PlayerWeaponChange>()._currentEquippedGun;
            if (_equippedGun == null) { print($"No gun equipped on {gameObject.name}"); return; }
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
