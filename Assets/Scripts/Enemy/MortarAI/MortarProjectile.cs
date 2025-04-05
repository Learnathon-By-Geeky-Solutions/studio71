using UnityEngine;

namespace MortarAI
{
    public class MortarProjectile : MonoBehaviour
    {
        public float damage = 25f;
        public float explosionRadius = 2f;
        public GameObject explosionEffectPrefab;

        private void OnCollisionEnter(Collision collision)
        {
            // Create explosion effect
            if (explosionEffectPrefab != null)
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // Damage players in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in colliders)
            {
                // Check if player
                if (hit.CompareTag("Player"))
                {
                    // Apply damage
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        // Calculate distance-based damage falloff
                        float distance = Vector3.Distance(transform.position, hit.transform.position);
                        float damageMultiplier = 1f - (distance / explosionRadius);
                        float finalDamage = damage * Mathf.Max(damageMultiplier, 0.2f);

                        damageable.TakeDamage(finalDamage);
                    }
                }
            }

            // Destroy projectile
            Destroy(gameObject);
        }
    }
}