using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public GameObject explosionEffect;

    private void Start()
    {
        Invoke("Explode", explosionDelay);
    }

    private void Explode()
    {
        if (explosionEffect)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        Destroy(gameObject);
    }
}