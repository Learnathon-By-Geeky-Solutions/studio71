using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using SingletonManagers;
using Player;
using LevelSpecific;

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
        [Tooltip("Event triggered when health changes. Passes current health value.")]
        [SerializeField] private UnityEvent<int> _onHealthChanged = new UnityEvent<int>();
        
        [Tooltip("Event triggered when entity dies.")]
        [SerializeField] private UnityEvent _onDeath = new UnityEvent();
        
        private int _currentHealth;
        
        #region Public Events (Read-Only Access)
        /// <summary>
        /// Event triggered when health changes. Passes current health value.
        /// Subscribe to this event to react to health updates.
        /// </summary>
        public UnityEvent<int> OnHealthChanged => _onHealthChanged;
        
        /// <summary>
        /// Event triggered when entity dies.
        /// Subscribe to this event to react to the entity's death.
        /// </summary>
        public UnityEvent OnDeath => _onDeath;
        #endregion
        
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
        private void Start()
        {
            if (!gameObject.CompareTag("Enemy")) return;
            EnemyChecker.Instance?.RegisterEnemy();
            switch (gameObject.name)
            {
                case "FirstClear":
                {
                    EnemyChecker.Instance?.RegisterFirstClear();
                    break;
                }
                case "SecondClear":
                {
                    EnemyChecker.Instance?.RegisterSecondClear();
                    break;
                }
            }
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
            _onHealthChanged.Invoke(_currentHealth);
            
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
            _onHealthChanged.Invoke(_currentHealth);
        }
        
        /// <summary>
        /// Reset health to maximum value.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _onHealthChanged.Invoke(_currentHealth);
        }
        
        /// <summary>
        /// Handle entity death - trigger events and optionally destroy the GameObject.
        /// </summary>
        private void HandleDeath()
        {
            // Trigger death event
            _onDeath.Invoke();
            // Destroy object if it's an enemy or destroyOnDeath is true
            if (gameObject.CompareTag("Enemy"))
            {
                EnemyChecker.Instance.UnregisterEnemy();
                if (gameObject.name == "FirstClear")
                {
                    EnemyChecker.Instance.UnregisterFirstClear();
                }
                if (gameObject.name == "SecondClear") 
                { 
                    EnemyChecker.Instance.UnregisterSecondClear(); 
                }
                Destroy(gameObject);
            }
            else
            {
                gameObject.GetComponent<PlayerAnimation>().DeathAnimation();
                StartCoroutine(InvokeDelayedDeath());
            }
        }
        private static IEnumerator InvokeDelayedDeath()
        {
            yield return new WaitForSeconds(3.5f);
            LevelConditionManager.Instance.OnPlayerDeath();
        }
    }
}