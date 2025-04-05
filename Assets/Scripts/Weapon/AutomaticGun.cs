using UnityEngine;
using SingletonManagers;
/// <summary>
/// Implementation for Automatic Gun.
/// </summary>
namespace Weapon
{
    public class AutomaticGun : Gun
    {
        private float _nextFireTime;
        private void Awake()
        {
            CurrentMagazineSize = Magazine_Size;
        }
        private void Update()
        {
            if (!IsShooting || (!(Time.time >= _nextFireTime)) || CurrentMagazineSize <= 0) return;
            Shoot();
            _nextFireTime = Time.time + 1f / Fire_Rate;
            CurrentMagazineSize -= 1;
        }

        public override void Shoot()
        {
            Instantiate(Prefab_Bullet, Fire_Point.position, Rotation.rotation);
            ParticleManager.Instance.PlayParticle("Gunshot", Fire_Point.position, Quaternion.identity);
            AudioManager.Instance.PlaySound("gunShot", Fire_Point.position);
        }
    }
}
