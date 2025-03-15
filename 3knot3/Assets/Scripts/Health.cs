using UnityEngine;

namespace HealthSystem
{


    public class Health : MonoBehaviour
    {
        [SerializeField] private int _maxHealth = 0;

        private int _currentHealth = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _currentHealth = _maxHealth;
        }

        private void Update()
        {
            if (_currentHealth == 0)
            {
                Destroy(gameObject);
            }
        }

        public void TakeDmg(int DmgAmount)
        {
            _currentHealth -= DmgAmount;
        }
    }
}
