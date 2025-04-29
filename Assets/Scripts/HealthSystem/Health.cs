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
        [SerializeField] private float _maxHealth = 100;
        
        [Header("Events")] 
        [Tooltip("Event triggered when health changes. Passes current health value.")]
        [SerializeField] private UnityEvent<float> _onHealthChanged = new UnityEvent<float>();
        
        [Tooltip("Event triggered when entity dies.")]
        [SerializeField] private UnityEvent _onDeath = new UnityEvent();
        
        private float _currentHealth;
        
        #region Public Events (Read-Only Access)
        /// <summary>
        /// Event triggered when health changes. Passes current health value.
        /// Subscribe to this event to react to health updates.
        /// </summary>
        public UnityEvent<float> OnHealthChanged => _onHealthChanged;
        
        /// <summary>
        /// Event triggered when entity dies.
        /// Subscribe to this event to react to the entity's death.
        /// </summary>
        public UnityEvent OnDeath => _onDeath;
        #endregion
        
        #region Properties
        /// <summary>Maximum possible health.</summary>
        public float MaxHealth => _maxHealth;
        
        /// <summary>Current health value.</summary>
        public float CurrentHealth => _currentHealth;
        
        /// <summary>Whether the entity is alive (health > 0).</summary>
        public bool IsAlive => _currentHealth > 0;
        #endregion

        private void Awake()
        {
            ResetHealth();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Start can not be static")]
        private void Start()
        {
            if (!gameObject.CompareTag("Enemy")) return;

            if (EnemyChecker.Instance == null) return;
            EnemyChecker.Instance.RegisterEnemy();

            switch (gameObject.name)
            {
                case "FirstClear":
                    EnemyChecker.Instance.RegisterFirstClear();
                    break;
                case "SecondClear":
                    EnemyChecker.Instance.RegisterSecondClear();
                    break;
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
        public void Heal(float healAmount)
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
                if (EnemyChecker.Instance != null)
                {
                    EnemyChecker.Instance.UnregisterEnemy();

                    switch (gameObject.name)
                    {
                        case "FirstClear":
                            EnemyChecker.Instance.UnregisterFirstClear();
                            break;
                        case "SecondClear":
                            EnemyChecker.Instance.UnregisterSecondClear();
                            break;
                    }
                }
                Destroy(gameObject, 3.2f);
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