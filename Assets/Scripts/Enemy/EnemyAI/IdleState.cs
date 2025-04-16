using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class IdleState : IEnemyState
    {
        private readonly EnemyAI _enemy;
        private readonly float _patrolWaitTime = 2f;
        private float _patrolTimer = 0f;
        public bool IsWaiting{ get; private set; } = false;
        private bool _hasDestination = false;

        public IdleState(EnemyAI enemyAI)
        {
            _enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Idle State");

            // Reset patrol timer
            _patrolTimer = 0f;
            IsWaiting = false;
            _hasDestination = false;

            // Set agent speed for patrolling
            _enemy.navMeshAgent.speed = 2f;
        }

        public void Execute()
        {
            // Check if player is in detection range
            if (_enemy.playerInDetectionRange)
            {
                _enemy.ChangeState(_enemy.alertState);
                return;
            }

            // Random patrol behavior
            if (IsWaiting)
            {
                _patrolTimer += Time.deltaTime;
                if (_patrolTimer >= _patrolWaitTime)
                {
                    IsWaiting = false;
                    _hasDestination = false;
                    _patrolTimer = 0f;
                }
            }
            else
            {
                if (!_hasDestination)
                {
                    // Get a new random patrol point
                    Vector3 newDestination = _enemy.GetRandomPatrolPoint();
                    _enemy.navMeshAgent.SetDestination(newDestination);
                    _hasDestination = true;
                }
                else
                {
                    // Check if we've reached the destination
                    if (_enemy.navMeshAgent.remainingDistance <= _enemy.navMeshAgent.stoppingDistance &&
                        !_enemy.navMeshAgent.pathPending)
                    {
                        IsWaiting = true;
                        _patrolTimer = 0f;
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