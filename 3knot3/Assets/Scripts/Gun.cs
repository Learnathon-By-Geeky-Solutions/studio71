using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    public string _gunName;
    public float _fireRate;
    public Transform _firePoint;
    public GameObject Prefab_Bullert;

    protected bool _isShooting;

    public abstract void Shoot();

    public void StartShooting() => _isShooting = true;
    public void StopShooting () => _isShooting = false;
}
