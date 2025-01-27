using UnityEngine;
/// <summary>
/// General Implementation of Gun
/// </summary>
namespace Weapon
{
    public abstract class Gun : MonoBehaviour
    {
        [SerializeField] protected float Fire_Rate;
        [SerializeField] protected Transform Fire_Point;
        [SerializeField] protected GameObject Prefab_Bullet;

        protected bool _isShooting;
        private void Awake()
        {
            if (Fire_Point == null) { print($"Fire Point not Assigned for{gameObject.name}"); }
            if (Prefab_Bullet == null) { print($"BulletPrefab not Assigned for {gameObject.name}"); }
        }
        protected abstract void Shoot();

        public void StartShooting() => _isShooting = true;
        public void StopShooting() => _isShooting = false;
    }
}
