using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class AlertState : IEnemyState
    {
        private EnemyAI enemy;
        private bool _countdownStarted;

        public AlertState(EnemyAI enemyAI)
        {
            enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Alert State");

            // Reset alert timer
            enemy.currentAlertTime = 0f;
            _countdownStarted = false;

            // Stop moving
            enemy.navMeshAgent.ResetPath();
        }

        public void Execute()
        {
            // Always look at player
            if (enemy.player != null)
            {
                Vector3 lookDirection = enemy.player.position - enemy.transform.position;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    enemy.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }

            // Check if player is out of detection range
            if (!enemy.playerInDetectionRange)
            {
                enemy.ChangeState(enemy.idleState);
                return;
            }

            // Check line of sight
            if (enemy.playerInLineOfSight)
            {
                // Player is visible, start or continue countdown
                _countdownStarted = true;
                enemy.currentAlertTime += Time.deltaTime;

                // If countdown is complete, switch to follow state
                if (enemy.currentAlertTime >= enemy.alertCountdown)
                {
                    enemy.ChangeState(enemy.followState);
                    return;
                }
            }
            else
            {
                // Player is not visible, pause countdown
                _countdownStarted = false;
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Alert State");
        }
    }
}  