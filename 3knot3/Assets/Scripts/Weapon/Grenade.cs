using HealthSystem;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionDelay = 3f; // Time before the grenade explodes
    public float explosionRadius = 5f; // Radius of the explosion
    public float explosionForce = 700f; // Force applied to objects within the explosion radius
    public GameObject explosionEffect; // Visual effect for the explosion
    [SerializeField] private LayerMask hitLayers; // Layers affected by the explosion
    [SerializeField] private int _GrenadeDMG = 0; // Damage dealt by the grenade

    private Rigidbody _rigidbody; // Reference to the Rigidbody component

    private void Start()
    {
        // Get the Rigidbody component
        _rigidbody = GetComponent<Rigidbody>();

        // Throw the grenade with an initial force
        ThrowGrenade();

        // Schedule the explosion and destruction of the grenade
        Invoke("Explode", explosionDelay);
        Destroy(gameObject, explosionDelay + .01f);
    }

    private void ThrowGrenade()
    {
        // Apply an initial force to the grenade to make it act like a projectile
        if (_rigidbody != null)
        {
            // Example: Throw the grenade forward and upward
            Vector3 throwDirection = transform.forward + transform.up; // Adjust the direction as needed
            _rigidbody.AddForce(throwDirection * 5f, ForceMode.Impulse); // Adjust the force as needed
        }
    }

    private void Explode()
    {
        // Instantiate the explosion effect
        if (explosionEffect)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Detect all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider other in colliders)
        {
            // Check if the collider is on a valid layer
            if (((1 << other.gameObject.layer) & hitLayers) != 0)
            {
                Debug.Log($"Hit {other.gameObject.name}");

                // Apply explosion force to Rigidbody components
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }

                // Apply damage to Health components
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDmg(_GrenadeDMG);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the explosion radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}