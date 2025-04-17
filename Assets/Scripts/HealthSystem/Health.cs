using UnityEngine;
using UnityEngine.Events;
using SingletonManagers;

namespace HealthSystem
{
    /// <summary>
    /// Manages entity health, damage and healing.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int _maxHealth = 100;
        
        [Header("Events")]
        [SerializeField] private bool _destroyOnDeath = true;
        
        private int _currentHealth;
        
        /// <summary>
        /// Event triggered when health changes. Passes current health value.
        /// </summary>
        public UnityEvent<int> OnHealthChanged = new UnityEvent<int>();
        
        /// <summary>
        /// Event triggered when entity dies.
        /// </summary>
        public UnityEvent OnDeath = new UnityEvent();
        
        #region Properties
        /// <summary>Maximum possible health.</summary>
        public int MaxHealth => _maxHealth;
        
        /// <summary>Current health value.</summary>
        public int CurrentHealth => _currentHealth;
        
        /// <summary>Whether the entity is alive (health > 0).</summary>
        public bool IsAlive => _currentHealth > 0;
        #endregion

        private void Awake()
        {
            ResetHealth();
        }

        /// <summary>
        /// Apply damage to this entity.
        /// </summary>
        /// <param name="damageAmount">Amount of damage to inflict</param>
        public void TakeDmg(int damageAmount)
        {
            if (!IsAlive) return;
            
            _currentHealth -= damageAmount;
            
            // Clamp health at 0
            if (_currentHealth < 0)
                _currentHealth = 0;
                
            // Notify listeners about health change
            OnHealthChanged.Invoke(_currentHealth);
            
            // Check if entity has died from this damage
            if (_currentHealth <= 0)
            {
                HandleDeath();
            }
        }
        
        /// <summary>
        /// Heal the entity by the specified amount.
        /// </summary>
        /// <param name="healAmount">Amount of health to restore</param>
        public void Heal(int healAmount)
        {
            if (!IsAlive) return;
            
            _currentHealth = Mathf.Min(_currentHealth + healAmount, _maxHealth);
            OnHealthChanged.Invoke(_currentHealth);
        }
        
        /// <summary>
        /// Reset health to maximum value.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged.Invoke(_currentHealth);
        }
        
        /// <summary>
        /// Handle entity death - trigger events and optionally destroy the GameObject.
        /// </summary>
        private void HandleDeath()
        {
            // Trigger death event
            OnDeath.Invoke();
            
            // Destroy object if it's an enemy or destroyOnDeath is true
            if (_destroyOnDeath || gameObject.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
}