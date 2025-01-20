using UnityEngine;

public class AutomaticGun : Gun
{
    private float _nextFireTime;

    private void Update()
    {
        if (_isShooting && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + 1f / _fireRate;
        }
    }

    public override void Shoot()
    {
        Instantiate(Prefab_Bullert,_firePoint.position, _firePoint.rotation);
    }
}
