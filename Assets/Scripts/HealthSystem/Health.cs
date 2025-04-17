using UnityEngine;
using SingletonManagers;

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

        //todo: Again abusing update. Don't abuse update, write code update in only and only necessary. This is always good practice. 
        //todo: here problem is you're checking health every frame even if no change has occurred. Move the destroy logic into TakeDmg() or a CheckDeath() method.
        
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


        //todo: lack of healing or reset support. If this class will be reused for players or allies, write this
        // public void Heal(int amount)
        // { 
        // _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        // OnHealthChanged?.Invoke(_currentHealth);
        // }

        // public void ResetHealth()
        //{
        // _currentHealth = _maxHealth;
        //  OnHealthChanged?.Invoke(_currentHealth);
        // }
        
    }
}
