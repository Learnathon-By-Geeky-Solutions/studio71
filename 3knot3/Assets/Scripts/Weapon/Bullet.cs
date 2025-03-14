using UnityEngine;
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
            print($"Hit {other.gameObject.name}");
            other.gameObject.GetComponent<Health>().TakeDmg(_bulletDmg);
            Destroy(transform.parent.gameObject);

        }
    }
}
