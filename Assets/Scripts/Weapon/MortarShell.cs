using HealthSystem;
using UnityEngine;
using SingletonManagers;
using Player;
namespace Weapon{
public class MortarShell : MonoBehaviour
{
    [Header("Explosion Settings")]
        [SerializeField] private float explosionDelay = 15f;
        [SerializeField] private float explosionRadius = 20f;
        [SerializeField] private float explosionForce = 700f;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private int _MortarDmg = 0;


        private Rigidbody _rigidbody;
         private Transform player; // Reference to the player

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform; // Find player by tag

            if (player == null)
            {
                Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
                return;
            }
            // Schedule explosion
            Invoke(nameof(Explode), explosionDelay);
        }

        private void Explode()
        {
            ParticleManager.Instance.PlayParticle("Grenade Explosion", transform.position, Quaternion.identity);
            AudioManager.PlaySound(SoundKeys.GrenadeExplosion, transform.position, 1f, 1f); // Play explosion sound
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider other in colliders)
            {
                if (((1 << other.gameObject.layer) & hitLayers) == 0) continue; // Correct bitmask check
                Debug.Log($"Hit {other.gameObject.name}");

                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                Health health = other.GetComponent<Health>();
                if (health != null)
                    health.TakeDmg(_MortarDmg);
            }

            Destroy(gameObject); // Destroy grenade after explosion
        }
}
}
