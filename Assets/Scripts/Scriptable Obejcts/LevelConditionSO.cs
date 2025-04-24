using System;
using UnityEngine;
namespace LevelConditions
{
    [CreateAssetMenu(menuName = "Levels/Level Conditions")]
    public class LevelConditionSO : ScriptableObject
    {
        #region SO Information
        public string levelName;
        public int sceneBuildIndex;
        #endregion

        #region Properties for Win.
        [Header("Win Conditions")]
        public bool winOnAllEnemiesDead;
        public bool winOnTimerEnd;
        public float surviveTime;
        #endregion

        #region Properties for Lose.
        [Header("Lose Conditions")]
        public bool loseOnPlayerDeath = true;
        public bool loseOnBaseDestroyed = false;
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



