using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Handles Weapon Change of the player.
/// </summary>
namespace Player
{
    public class PlayerWeaponChange : MonoBehaviour
    {
        public Weapon.Gun _currentEquippedGun { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _currentEquippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
        }
        public void PrimaryWeapon(InputAction.CallbackContext context)
        {
            if (context.performed && !_currentEquippedGun.IsShooting) 
            {
                _currentEquippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
            }
        }
        public void SecondaryWeapon(InputAction.CallbackContext context)
        {
            if (context.performed && !_currentEquippedGun.IsShooting)
            {
                _currentEquippedGun = gameObject.GetComponentInChildren<Weapon.SemiAutomaticGun>();
            }
        }
    }
}