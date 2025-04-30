using JetBrains.Annotations;
using System;
using UnityEngine;
namespace LevelConditions
{
    [CreateAssetMenu(menuName = "Levels/Level Conditions")]
    public class LevelConditionSO : ScriptableObject
    {
        #region SO Information
        [SerializeField] private string _levelName;
        [SerializeField] private int _sceneBuildIndex;
        public int SceneBuildIndex
        {
            get=>_sceneBuildIndex; set => _sceneBuildIndex = value;
        }
        #endregion

        #region Properties for Win.
        [Header("Win Conditions")]
        [SerializeField] private bool _winOnAllEnemiesDead;
        public bool WinOnAllEnemiesDead
        {
            get => _winOnAllEnemiesDead;private set=>_winOnAllEnemiesDead = value;
        }
        [SerializeField] private bool _winOnTimerEnd;
        public bool WinOnTimerEnd
        {
            get => _winOnTimerEnd;private set=> _winOnTimerEnd = value;
        }
        [SerializeField] private float _surviveTime;
        public float SurviveTime
        {
            get => _surviveTime; set => _surviveTime = value;   
        }
        #endregion

        #region Properties for Lose.
        [Header("Lose Conditions")]
        [SerializeField] private bool _loseOnPlayerDeath = true;
        public bool LoseOnPlayerDeath
        {
            get => _loseOnPlayerDeath;private set => _loseOnPlayerDeath = value;
        }

        [SerializeField] private bool _loseOnTimerEndEnemiesSurviving=false;
        public bool LoseOnTimerEndEnemiesSurviving
        {
            get => _loseOnTimerEndEnemiesSurviving;private set => _loseOnTimerEndEnemiesSurviving=value;
        }
        #endregion

        #region Events to Subscribe/Unsubscribe
        // Events (subscribe via += on Start and -= on OnDisable)
        public event Action OnWin; //Subscribe to This event using LevelConditionManager.Instance._currentConditions.OnWin and do it on Start Method
        public event Action OnLose; //Subscribe to This event using LevelConditionManager.Instance._currentConditions.OnLOse and do it on Start Method
        #endregion

        #region Methods for Scripts
        public void TriggerWin()
        {
            OnWin?.Invoke();
        }

        public void TriggerLose()
        {
            OnLose?.Invoke();
        }
        #endregion
    }
}



