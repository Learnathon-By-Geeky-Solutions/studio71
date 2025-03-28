using UnityEngine;
using UnityEngine.UI;  // Add this namespace for UI components

namespace HealthSystem
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int _maxHealth = 100;  // Changed default to 100 for better visualization
        [SerializeField] private Slider _healthBar;  // Reference to UI slider
        [SerializeField] private int _damageAmount = 10;  // Amount of damage per button press

        private int _currentHealth = 0;

        void Awake()
        {
            _currentHealth = _maxHealth;
            UpdateHealthBar();
        }

        private void Update()
        {
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                UpdateHealthBar();
                Destroy(gameObject);
            }
        }

        public void TakeDmg(int DmgAmount)
        {
            _currentHealth -= DmgAmount;
            _currentHealth = Mathf.Max(0, _currentHealth);  // Ensure health doesn't go below zero
            UpdateHealthBar();
        }

        // Method to call from UI button
        public void DamageOnButtonPress()
        {
            TakeDmg(_damageAmount);
        }

        // Update health bar UI
        private void UpdateHealthBar()
        {
            if (_healthBar != null)
            {
                _healthBar.value = (float)_currentHealth / _maxHealth;
            }
        }
    }
}
