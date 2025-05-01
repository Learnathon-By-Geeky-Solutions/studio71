using HealthSystem;
using UnityEngine;
using SingletonManagers;
using Player;

namespace Weapon
{
    public class MortarShell : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [SerializeField] private float _explosionRadius = 20f;
        [SerializeField] private float _explosionForce = 700f;
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private int _mortarDamage = 0;

        private bool hasExploded = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (!hasExploded)
            {
                Detonate();
            }
        }

        private void Detonate()
        {
            hasExploded = true;

            ParticleManager.Instance?.PlayParticle("Grenade Explosion", transform.position, Quaternion.identity);
            AudioManager.PlaySound(SoundKeys.GrenadeExplosion, transform.position, 1f, 1f);

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (Collider hitObject in hitColliders)
            {
                if (((1 << hitObject.gameObject.layer) & _targetLayers) == 0) continue;

                Rigidbody targetRigidbody = hitObject.GetComponent<Rigidbody>();
                if (targetRigidbody != null)
                {
                    targetRigidbody.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
                }

                Health targetHealth = hitObject.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDmg(_mortarDamage);
                }
            }

            Destroy(gameObject); // Remove the shell
        }
    }
}