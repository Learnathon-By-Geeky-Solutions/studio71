using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class GrenadeThrowState : IEnemyState
    {
        private readonly EnemyAI enemy;
        private float throwTimer = 0f;
        private readonly float throwInterval = 1f;
        private bool hasThrown = false;

        public GrenadeThrowState(EnemyAI enemyAI)
        {
            enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Grenade Throw State");

            // Reset timers
            throwTimer = 0f;
            hasThrown = false;

            // Stop moving
            enemy.navMeshAgent.ResetPath();
        }

        public void Execute()
        {
            // Look at player
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

            // Check if player is now in line of sight
            if (enemy.playerInLineOfSight)
            {
                enemy.ChangeState(enemy.shootState);
                return;
            }

            // Check if out of grenades
            if (enemy.currentGrenades <= 0)
            {
                enemy.ChangeState(enemy.shootState);
                return;
            }

            // Throw grenade if possible
            if (enemy.canThrowGrenade && !hasThrown)
            {
                throwTimer += Time.deltaTime;

                if (throwTimer >= throwInterval)
                {
                    enemy.ThrowGrenade();
                    hasThrown = true;

                    // Wait a bit after throwing before potentially changing state
                    throwTimer = 0f;
                }
            }
            else if (hasThrown && throwTimer < 2f)
            {
                // Wait a bit after throwing to see if the situation changes
                throwTimer += Time.deltaTime;
            }
            else if (hasThrown)
            {
                // After waiting, reassess the situation
                hasThrown = false;
                throwTimer = 0f;
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Grenade Throw State");
        }
    }
}