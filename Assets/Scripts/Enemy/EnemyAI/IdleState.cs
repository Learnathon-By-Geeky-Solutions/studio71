using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class IdleState : IEnemyState
    {
        private readonly EnemyAI enemy;
        private readonly float patrolWaitTime = 2f;
        private float patrolTimer = 0f;
        public bool isWaiting = false;
        private bool hasDestination = false;

        public IdleState(EnemyAI enemyAI)
        {
            enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Idle State");

            // Reset patrol timer
            patrolTimer = 0f;
            isWaiting = false;
            hasDestination = false;

            // Set agent speed for patrolling
            enemy.navMeshAgent.speed = 2f;
        }

        public void Execute()
        {
            // Check if player is in detection range
            if (enemy.playerInDetectionRange)
            {
                enemy.ChangeState(enemy.alertState);
                return;
            }

            // Random patrol behavior
            if (isWaiting)
            {
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolWaitTime)
                {
                    isWaiting = false;
                    hasDestination = false;
                    patrolTimer = 0f;
                }
            }
            else
            {
                if (!hasDestination)
                {
                    // Get a new random patrol point
                    Vector3 newDestination = enemy.GetRandomPatrolPoint();
                    enemy.navMeshAgent.SetDestination(newDestination);
                    hasDestination = true;
                }
                else
                {
                    // Check if we've reached the destination
                    if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance &&
                        !enemy.navMeshAgent.pathPending)
                    {
                        isWaiting = true;
                        patrolTimer = 0f;
                    }
                }
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Idle State");
        }
    }
}