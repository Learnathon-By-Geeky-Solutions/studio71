using UnityEngine;

public class SemiAutomaticGun : Gun
{
    private void Update()
    {
        Shoot();
    }
    public override void Shoot()
    {
        if (_isShooting)
        {
            Instantiate(Prefab_Bullert, _firePoint.position, _firePoint.rotation);
            _isShooting = false;
        }
    }
}
