using HealthSystem;
using UnityEngine;
using SingletonManagers;
using Player;

namespace Weapon
{
    public class MortarShell : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [SerializeField] private float _explosionDelay = 15f;
        [SerializeField] private float _explosionRadius = 20f;
        [SerializeField] private float _explosionForce = 700f;
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private int _mortarDamage = 0;

        private Rigidbody _shellRigidbody;
        private Transform _playerTransform; // Reference to the player's transform

        private void Awake()
        {
            _shellRigidbody = GetComponent<Rigidbody>();
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform; // Find player by tag

            if (_playerTransform == null)
            {
                Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
                return;
            }

            // Schedule the detonation
            Invoke(nameof(Detonate), _explosionDelay);
        }

        private void Detonate()
        {
            ParticleManager.Instance.PlayParticle("Grenade Explosion", transform.position, Quaternion.identity);
            AudioManager.PlaySound(SoundKeys.GrenadeExplosion, transform.position, 1f, 1f); // Play explosion sound
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius);

            foreach (Collider hitObject in hitColliders)
            {
                if (((1 << hitObject.gameObject.layer) & _targetLayers) == 0) continue; // Correct bitmask check
                Debug.Log($"Mortar hit: {hitObject.gameObject.name}");

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

            Destroy(gameObject); // Destroy the mortar shell after detonation
        }
    }
}