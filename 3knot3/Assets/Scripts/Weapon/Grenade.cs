using HealthSystem;
using Ink.Parsed;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public GameObject explosionEffect;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private int _GrenadeDMG = 0;

    private void Start()
    {
        Invoke("Explode", explosionDelay);
        Destroy(gameObject, explosionDelay + .01f);
    }

    private void Explode()
    {
        if (explosionEffect)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider other in colliders)
        {
            if (((1 << other.gameObject.layer) & hitLayers) == 0) return;
            print($"Hit {other.gameObject.name}");
            other.gameObject.GetComponent<Health>().TakeDmg(_GrenadeDMG);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}