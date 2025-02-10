using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Implementation for Semi-Auto Gun.
/// </summary>
namespace Weapon
{
    public class SemiAutomaticGun : Gun
    {
        private void Update()
        {
            Shoot();
        }

        protected override void Shoot()
        {
            if (!IsShooting) return;
            Instantiate(Prefab_Bullet, Fire_Point.position, Fire_Point.rotation);
            IsShooting = false;
        }
    }
}
