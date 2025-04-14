using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Implementation for Semi-Auto Gun.
/// </summary>
namespace Weapon
{
    public class SemiAutomaticGun : Gun
    {
        private void Awake()
        {
            CurrentMagazineSize = Magazine_Size;
        }
        private void Update()
        {
            Shoot();
        }

        public override void Shoot()
        {
            if (!IsShooting || CurrentMagazineSize <= 0)
            {
                return;
            }
            Instantiate(Prefab_Bullet, Fire_Point.position, Fire_Point.rotation);
            IsShooting = false;
            CurrentMagazineSize -= 1;
            
        }
    }
}
