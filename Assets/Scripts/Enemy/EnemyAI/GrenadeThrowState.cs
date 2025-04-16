using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class GrenadeThrowState : IEnemyState
    {
        private readonly EnemyAI _enemy;
        private float _throwTimer = 0f;
        private readonly float _throwInterval = 1f;
        private bool _hasThrown = false;

        public GrenadeThrowState(EnemyAI enemyAI)
        {
            _enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Grenade Throw State");

            // Reset timers
            _throwTimer = 0f;
            _hasThrown = false;

            // Stop moving
            _enemy.navMeshAgent.ResetPath();
        }

        public void Execute()
        {
            // Look at player
            if (_enemy.player != null)
            {
                Vector3 lookDirection = _enemy.player.position - _enemy.transform.position;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    _enemy.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }

            // Check if player is out of attack range
            if (!_enemy.playerInAttackRange)
            {
                _enemy.ChangeState(_enemy.followState);
                return;
            }

            // Check if player is now in line of sight
            if (_enemy.playerInLineOfSight)
            {
                _enemy.ChangeState(_enemy.shootState);
                return;
            }

            // Check if out of grenades
            if (_enemy.currentGrenades <= 0)
            {
                _enemy.ChangeState(_enemy.shootState);
                return;
            }

            // Throw grenade if possible
            if (_enemy.canThrowGrenade && !_hasThrown)
            {
                _throwTimer += Time.deltaTime;

                if (_throwTimer >= _throwInterval)
                {
                    _enemy.ThrowGrenade();
                    _hasThrown = true;

                    // Wait a bit after throwing before potentially changing state
                    _throwTimer = 0f;
                }
            }
            else if (_hasThrown && _throwTimer < 2f)
            {
                // Wait a bit after throwing to see if the situation changes
                _throwTimer += Time.deltaTime;
            }
            else if (_hasThrown)
            {
                // After waiting, reassess the situation
                _hasThrown = false;
                _throwTimer = 0f;
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Grenade Throw State");
        }
    }
}