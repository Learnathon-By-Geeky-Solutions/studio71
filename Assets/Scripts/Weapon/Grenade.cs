using HealthSystem;
using UnityEngine;
using SingletonManagers;
using Player;
namespace Weapon
{
    /// <summary>
    /// Grenade Implementation.
    /// </summary>
    public class Grenade : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [SerializeField] private float explosionDelay = 3f;
        [SerializeField] private float explosionRadius = 4f;
        [SerializeField] private float explosionForce = 700f;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private int _GrenadeDMG = 0;

        [Header("Throw Settings")]
        [SerializeField] private float forwardForce = 5f; // Forward force multiplier
        [SerializeField] private float upwardForce = 2f; // Upward force multiplier

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

            ThrowGrenade();

            // Schedule explosion
            Invoke(nameof(Explode), explosionDelay);
        }

        private void ThrowGrenade()
        {
            if (_rigidbody != null && player != null)
            {
                // Use player's forward direction instead of grenade's local forward
                Vector3 throwDirection = (player.forward * forwardForce) + (Vector3.up * upwardForce) + player.gameObject.GetComponent<PlayerController>().CurrentVelocity;
                _rigidbody.AddForce(throwDirection, ForceMode.Impulse);
            }
        }

        private void Explode()
        {

            ParticleManager.Instance.PlayParticle("Grenade Explosion", transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySound("Grenade_Explosion", transform.position, 1f, 1f); // Play explosion sound
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
                    health.TakeDmg(_GrenadeDMG);
            }

            Destroy(gameObject); // Destroy grenade after explosion
        }
    }
}
