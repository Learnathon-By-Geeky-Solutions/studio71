using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using LevelConditions;
using Singleton;
namespace SingletonManagers {
    public class LevelConditionManager : SingletonPersistent
    {
        public static LevelConditionManager Instance => GetInstance<LevelConditionManager>();

        #region Properties
        [SerializeField] private List<LevelConditionSO> _allLevelConditions;
        public LevelConditionSO _currentConditions { get; private set; }
        private bool _levelEnded = false;
        #endregion

        #region General Methods
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        #region Methods for Scriptable Object
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AssignLevelConditionForScene(scene.buildIndex);
            _levelEnded = false;
        }

        private void AssignLevelConditionForScene(int buildIndex)
        {
            _currentConditions = _allLevelConditions.Find(condition => condition.sceneBuildIndex == buildIndex);

            if (_currentConditions == null)
            {
                Debug.LogWarning($"No LevelConditionSO found for scene {buildIndex}");
            }
        }
        #endregion

        #region Methods to Execute Win/Lose
        /// <summary>
        /// Execute These Methods in scripts within individual scenes When Win/Lose Condition is Met.
        /// </summary>
        public void OnAllEnemiesDefeated()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.winOnAllEnemiesDead) return;
            HandleWin();
        }

        public void OnTimerFinished()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.winOnTimerEnd) return;
            HandleWin();
        }

        public void OnPlayerDeath()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.loseOnPlayerDeath) return;
            HandleLose();
        }

        public void OnBaseDestroyed()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.loseOnBaseDestroyed) return;
            HandleLose();
        }
        #endregion

        #region Methods for Proper Working
        private void HandleWin()
        {
            _levelEnded = true;
            _currentConditions.TriggerWin();
        }

        private void HandleLose()
        {
            _levelEnded = true;
            _currentConditions.TriggerLose();
        }
        #endregion
    }
}
