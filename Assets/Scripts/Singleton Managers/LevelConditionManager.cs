using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using LevelConditions;
using Singleton;
using Unity.VisualScripting;
namespace SingletonManagers {
    public class LevelConditionManager : SingletonPersistent
    {
        public static LevelConditionManager Instance => GetInstance<LevelConditionManager>();

        #region Properties
        [SerializeField] private List<LevelConditionSO> _allLevelConditions;
        public LevelConditionSO _currentConditions { get; private set; }
        private bool _levelEnded = false;
        public bool _videoPlayed { get; private set; }
        #endregion

        #region General Methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "OnEnable can not be static")]
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "OnDisable can not be static")]
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        #region Methods for Scriptable Object
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Time.timeScale = 1.0f;
            AssignLevelConditionForScene(scene.buildIndex);
            _levelEnded = false;
            _videoPlayed = true;
        }

        private void AssignLevelConditionForScene(int buildIndex)
        {
            _currentConditions = _allLevelConditions.Find(condition => condition.SceneBuildIndex == buildIndex);

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
            if (_levelEnded || _currentConditions == null || !_currentConditions.WinOnAllEnemiesDead) return;
            HandleWin();
            print("ALL ENEMIES DEAD");
        }

        public void OnTimerFinished()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.WinOnTimerEnd) return;
            Time.timeScale = 0f;
            HandleWin();
        }

        public void OnPlayerDeath()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.LoseOnPlayerDeath) return;
            HandleLose();
        }

        public void OnBaseDestroyed()
        {
            if (_levelEnded || _currentConditions == null || !_currentConditions.LoseOnBaseDestroyed) return;
            HandleLose();
        }

        public void OnTimerEndEnemiesSurviving()
        {
            if(_levelEnded||_currentConditions==null||!_currentConditions.LoseOnTimerEndEnemiesSurviving) return;
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
