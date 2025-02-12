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
        private void Start()
        {
            Destroy(gameObject, _maxLifeTime);
        }
        private void Update()
        {
            transform.position += transform.forward * _bulletSpeed * Time.deltaTime;
        }
    }
}
