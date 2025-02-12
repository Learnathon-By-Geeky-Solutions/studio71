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
        [field: SerializeField] public int Magazine_Size { get; private set; }
        public int CurrentMagazineSize { get; set; }

        public bool IsShooting { get; protected set; }
        private void Awake()
        {
            if (Fire_Point == null) { print($"Fire Point not Assigned for{gameObject.name}"); }
            if (Prefab_Bullet == null) { print($"BulletPrefab not Assigned for {gameObject.name}"); }
        }
        protected abstract void Shoot();

        public void StartShooting() => IsShooting = true;
        public void StopShooting() => IsShooting = false;
    }
}
