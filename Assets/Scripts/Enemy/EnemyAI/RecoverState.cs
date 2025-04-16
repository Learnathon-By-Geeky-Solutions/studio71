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
            HandleCoverOrRetreat();

            if (isHealing)
            {
                HealEnemy();

                if (enemy.currentHealth >= enemy.maxHealth)
                {
                    enemy.currentHealth = enemy.maxHealth;
                    HandlePostHealingState();
                }
            }
        }

        private void HandleCoverOrRetreat()
        {
            if (isTakingCover && !isHealing && enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
            {
                isHealing = true;
            }
        }

        private void HealEnemy()
        {
            enemy.currentHealth += enemy.recoveryRate * Time.deltaTime;
        }

        private void HandlePostHealingState()
        {
            if (enemy.playerInAttackRange)
            {
                HandleAttackRangeState();
            }
            else if (enemy.playerInDetectionRange)
            {
                enemy.ChangeState(enemy.followState);
            }
            else
            {
                enemy.ChangeState(enemy.idleState);
            }
        }

        private void HandleAttackRangeState()
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

        public void Exit()
        {
            Debug.Log("Exiting Recover State");
        }
    }
}