using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class FollowState : IEnemyState
    {
        private EnemyAI enemy;
        private float updatePathInterval = 0.5f;
        private float updatePathTimer = 0f;

        public FollowState(EnemyAI enemyAI)
        {
            enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Follow State");

            // Set agent speed
            enemy.navMeshAgent.speed = 3.5f;

            // Start following player
            if (enemy.player != null)
            {
                enemy.navMeshAgent.SetDestination(enemy.player.position);
            }
        }

        public void Execute()
        {
            // Check if player is out of detection range
            if (!enemy.playerInDetectionRange)
            {
                enemy.ChangeState(enemy.idleState);
                return;
            }

            // Check if player is in attack range
            if (enemy.playerInAttackRange)
            {
                // Decide between shooting and grenade throwing based on line of sight
                if (enemy.playerInLineOfSight)
                {
                    enemy.ChangeState(enemy.shootState);
                    return;
                }
                else if (enemy.currentGrenades > 0)
                {
                    enemy.ChangeState(enemy.grenadeThrowState);
                    return;
                }
                else
                {
                    // No grenades, try to get line of sight
                    enemy.ChangeState(enemy.shootState);
                    return;
                }
            }

            // Update path to player periodically
            updatePathTimer += Time.deltaTime;
            if (updatePathTimer >= updatePathInterval)
            {
                updatePathTimer = 0f;
                if (enemy.player != null)
                {
                    enemy.navMeshAgent.SetDestination(enemy.player.position);
                }
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Follow State");
        }
    }
}