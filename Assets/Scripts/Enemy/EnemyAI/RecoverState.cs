using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class RecoverState : IEnemyState
    {
        private EnemyAI enemy;
        private Transform coverTarget;
        private bool isTakingCover = false;
        private bool isHealing = false;

        public RecoverState(EnemyAI enemyAI)
        {
            enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Recover State");

            // Reset flags
            isTakingCover = false;
            isHealing = false;

            // Find cover
            coverTarget = enemy.FindCover();

            if (coverTarget != null)
            {
                enemy.navMeshAgent.SetDestination(coverTarget.position);
                isTakingCover = true;
            }
            else
            {
                // No cover found, move away from player
                if (enemy.player != null)
                {
                    Vector3 directionAway = enemy.transform.position - enemy.player.position;
                    Vector3 retreatPosition = enemy.transform.position +
                                              directionAway.normalized * enemy.detectionRange * 0.5f;

                    if (UnityEngine.AI.NavMesh.SamplePosition(retreatPosition, out UnityEngine.AI.NavMeshHit hit, 10f,
                            UnityEngine.AI.NavMesh.AllAreas))
                    {
                        enemy.navMeshAgent.SetDestination(hit.position);
                        isTakingCover = true;
                    }
                }
            }
        }

        public void Execute()
        {
            // Check if at cover or retreat position
            if (isTakingCover && !isHealing &&
                enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
            {
                isHealing = true;
            }

            // Heal if behind cover
            if (isHealing)
            {
                enemy.currentHealth += enemy.recoveryRate * Time.deltaTime;

                // Cap health at max
                if (enemy.currentHealth >= enemy.maxHealth)
                {
                    enemy.currentHealth = enemy.maxHealth;

                    // Healing complete, return to appropriate state
                    if (enemy.playerInAttackRange)
                    {
                        if (enemy.playerInLineOfSight)
                        {
                            enemy.ChangeState(enemy.shootState);
                        }
                        else if (enemy.currentGrenades > 0)
                        {
                            enemy.ChangeState(enemy.grenadeThrowState);
                        }
                        else
                        {
                            enemy.ChangeState(enemy.shootState);
                        }
                    }
                    else if (enemy.playerInDetectionRange)
                    {
                        enemy.ChangeState(enemy.followState);
                    }
                    else
                    {
                        enemy.ChangeState(enemy.idleState);
                    }

                    return;
                }
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Recover State");
        }
    }
}