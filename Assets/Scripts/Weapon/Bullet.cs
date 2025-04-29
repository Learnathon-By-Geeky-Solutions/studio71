using UnityEngine;
using HealthSystem;
using SingletonManagers;
/// <summary>
/// Implementation of bullets fired by weapons.
/// </summary>
namespace Weapon
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _bulletSpeed = 1f;
        [SerializeField] private float _maxLifeTime = 3f;
        [SerializeField] private int _bulletDmg = 0;
        [SerializeField] private LayerMask hitLayers;
        private void Start()
        {

            Destroy(transform.parent.gameObject, _maxLifeTime);
        }
        private void Update()
        {
            transform.position += -transform.up * _bulletSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & hitLayers) == 0) return;
            Vector3 hitPoint = transform.position;
            Health health = other.GetComponent<Health>();
            if (other.gameObject.tag is "Player" or "Enemy")
            {
                if (health != null && health.CurrentHealth > 0)
                {
                    health.TakeDmg(_bulletDmg);
                }
                ParticleManager.Instance.PlayParticle("Blood Splatter", hitPoint, Quaternion.identity);
                AudioManager.PlaySound(SoundKeys.BloodHit, hitPoint);
            }
            else {ParticleManager.Instance.PlayParticle("Terrain Hit", hitPoint, Quaternion.Euler(0, transform.eulerAngles.y + 180, 0));
                AudioManager.PlaySound(SoundKeys.TerrainHit, hitPoint);
            }
            Destroy(transform.parent.gameObject);

        }
    }
}
