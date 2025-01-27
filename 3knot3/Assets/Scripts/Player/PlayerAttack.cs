using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Controls Attack command of the player.
/// </summary>
namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Weapon.Gun _equippedGun;
        // Start is called once before the first execution of Update after the MonoBehaviour is create
        public void OnFire(InputAction.CallbackContext trigger)
        {
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
