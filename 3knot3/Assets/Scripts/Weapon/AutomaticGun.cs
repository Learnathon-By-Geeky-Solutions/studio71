using UnityEngine;

public class AutomaticGun : Gun
{
    private float _nextFireTime;

    private void Update()
    {
        if (_isShooting && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + 1f / Fire_Rate;
        }
    }

    protected override void Shoot()
    {
        Instantiate(Prefab_Bullet,Fire_Point.position, Fire_Point.rotation);
    }
}
