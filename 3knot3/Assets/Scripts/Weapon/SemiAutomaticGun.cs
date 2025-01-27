using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Implementation for Semi-Auto Gun.
/// </summary>
namespace Weapon
{
    public class SemiAutomaticGun : Gun
    {
        private void OnEnable()
        {
            Shoot();
        }

        protected override void Shoot()
        {
            if (_isShooting)
            {
                Instantiate(Prefab_Bullet, Fire_Point.position, Fire_Point.rotation);
                _isShooting = false;
            }
        }
    }
}
