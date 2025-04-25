using UnityEngine;
using SingletonManagers;
namespace LevelSpecific
{
    public class EnemyChecker : MonoBehaviour
    {
        private int _aliveEnemyCount = 0;
        private int _firstClear = 0;
        private int _secondClear = 0;

        [SerializeField] BoxCollider _firstClearCollider;
        [SerializeField] BoxCollider _secondClearCollider;
        public static EnemyChecker Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }


        public void RegisterEnemy()
        {
            _aliveEnemyCount++;
        }
        public void UnregisterEnemy()
        {

            _aliveEnemyCount--;
            if (_aliveEnemyCount > 0) return;
            LevelConditionManager.Instance.OnAllEnemiesDefeated();
        }
        public void RegisterFirstClear()
        {
            _firstClear++;
        }
        public void UnregisterFirstClear()
        {
            _firstClear--;
            if(_firstClear > 0)return;
            print("First Part Cleared");
            _firstClearCollider.enabled = false;
        }
        public void RegisterSecondClear()
        {
            _secondClear++;
        }
        public void UnregisterSecondClear()
        {
            _secondClear--;
            if (_secondClear > 0) return;
            print("Second Part Cleared");
            _secondClearCollider.enabled = false;
        }
    }
}