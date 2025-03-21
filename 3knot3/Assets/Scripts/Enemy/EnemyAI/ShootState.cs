using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class ShootState : IEnemyState
    {
        private EnemyAI enemy;
        private float shootTimer = 0f;
        private float repositionTimer = 0f;
        private float repositionInterval = 3f;
        private bool isRepositioning = false;
        private Vector3 repositionTarget;

        public ShootState(EnemyAI enemyAI)
        {
            enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Shoot State");

            // Reset timers
            shootTimer = 0f;
            repositionTimer = 0f;
            isRepositioning = false;

            // Stop moving initially to shoot
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

            // Check if player is out of attack range
            if (!enemy.playerInAttackRange)
            {
                enemy.ChangeState(enemy.followState);
                return;
            }

            // Handle repositioning when no line of sight
            if (!enemy.playerInLineOfSight && enemy.currentGrenades <= 0)
            {
                if (!isRepositioning)
                {
                    // Start repositioning to get line of sight
                    repositionTarget = enemy.FindPositionWithLineOfSight();
                    enemy.navMeshAgent.SetDestination(repositionTarget);
                    isRepositioning = true;
                }
                else
                {
                    // Check if we've reached the repositioning target
                    if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
                    {
                        repositionTimer += Time.deltaTime;

                        // Try another position after a delay if still no line of sight
                        if (repositionTimer >= repositionInterval)
                        {
                            repositionTimer = 0f;
                            repositionTarget = enemy.FindPositionWithLineOfSight();
                            enemy.navMeshAgent.SetDestination(repositionTarget);
                        }
                    }
                }

                return;
            }

            // Switch to grenade throw if we have grenades and lose line of sight
            if (!enemy.playerInLineOfSight && enemy.currentGrenades > 0)
            {
                enemy.ChangeState(enemy.grenadeThrowState);
                return;
            }

            // We have line of sight, stop moving and shoot
            if (isRepositioning)
            {
                enemy.navMeshAgent.ResetPath();
                isRepositioning = false;
            }

            // Shooting logic
            if (!enemy.isReloading)
            {
                shootTimer += Time.deltaTime;

                if (shootTimer >= 1f / enemy.fireRate)
                {
                    shootTimer = 0f;
                    enemy.Shoot();
                }
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Shoot State");
        }
    }
}