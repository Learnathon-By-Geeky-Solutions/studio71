using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public class RecoverState : IEnemyState
    {
        private readonly EnemyAI _enemy;
        private Transform _coverTarget;
        private bool _isTakingCover = false;
        private bool _isHealing = false;

        public RecoverState(EnemyAI enemyAI)
        {
            _enemy = enemyAI;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void Enter()
        {
            Debug.Log("Entering Recover State");

            // Reset flags
            _isTakingCover = false;
            _isHealing = false;

            // Find cover
            _coverTarget = _enemy.FindCover();

            if (_coverTarget != null)
            {
                _enemy.navMeshAgent.SetDestination(_coverTarget.position);
                _isTakingCover = true;
            }
            else
            {
                // No cover found, move away from player
                if (_enemy.player != null)
                {
                    Vector3 directionAway = _enemy.transform.position - _enemy.player.position;
                    Vector3 retreatPosition = _enemy.transform.position +
                                              directionAway.normalized * _enemy.detectionRange * 0.5f;

                    if (UnityEngine.AI.NavMesh.SamplePosition(retreatPosition, out UnityEngine.AI.NavMeshHit hit, 10f,
                            UnityEngine.AI.NavMesh.AllAreas))
                    {
                        _enemy.navMeshAgent.SetDestination(hit.position);
                        _isTakingCover = true;
                    }
                }
            }
        }

        public void Execute()
        {
            HandleCoverOrRetreat();

            if (_isHealing)
            {
                HealEnemy();

                if (_enemy.currentHealth >= _enemy.maxHealth)
                {
                    _enemy.currentHealth = _enemy.maxHealth;
                    HandlePostHealingState();
                }
            }
        }

        private void HandleCoverOrRetreat()
        {
            if (_isTakingCover && !_isHealing && _enemy.navMeshAgent.remainingDistance <= _enemy.navMeshAgent.stoppingDistance)
            {
                _isHealing = true;
            }
        }

        private void HealEnemy()
        {
            _enemy.currentHealth += _enemy.recoveryRate * Time.deltaTime;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void HandlePostHealingState()
        {
            if (_enemy.playerInAttackRange)
            {
                HandleAttackRangeState();
            }
            else if (_enemy.playerInDetectionRange)
            {
                _enemy.ChangeState(_enemy.followState);
            }
            else
            {
                _enemy.ChangeState(_enemy.idleState);
            }
        }

        private void HandleAttackRangeState()
        {
            if (_enemy.playerInLineOfSight)
            {
                _enemy.ChangeState(_enemy.shootState);
            }
            else if (_enemy.currentGrenades > 0)
            {
                _enemy.ChangeState(_enemy.grenadeThrowState);
            }
            else
            {
                _enemy.ChangeState(_enemy.shootState);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void Exit()
        {
            Debug.Log("Exiting Recover State");
        }
    }
}