using UnityEngine;

namespace HealthSystem
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int _maxHealth = 0;
        
        private int _currentHealth = 0;
        
        // Properties to access health values
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _currentHealth = _maxHealth;
        }
        private void Update()
        {
            if (gameObject.CompareTag("Enemy") && _currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
        public void TakeDmg(int DmgAmount)
        {
            _currentHealth -= DmgAmount;
            // Clamp health at 0
            if (_currentHealth < 0)
                _currentHealth = 0;
        }
    }
}