using Player;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Handles reload of weapon.
/// </summary>
namespace Player
{
    public class PlayerReload : MonoBehaviour
    {

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Reload(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                gameObject.GetComponent<PlayerAttack>()._equippedGun.StopShooting();
                gameObject.GetComponent<PlayerWeaponChange>()._currentEquippedGun.CurrentMagazineSize = gameObject.GetComponent<PlayerWeaponChange>()._currentEquippedGun.Magazine_Size;
            }

        }
    }
}
