using UnityEngine;

namespace patrolEnemy
{
    public class RecoverState : IEnemyState
    {
        private readonly EnemyAI _enemy;
        private bool _isTakingCover = false;
        private bool _isHealing = false;

        public RecoverState(EnemyAI enemyAI)
        {
            _enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Recover State");

            // Reset flags
            _isTakingCover = false;
            _isHealing = false;

            // Declare coverTarget as a local variable
            Transform coverTarget = _enemy.FindCover();

            if (coverTarget != null)
            {
                _enemy.navMeshAgent.SetDestination(coverTarget.position);
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
                        //todo: check navmesh is disabled or not => if (_enemy.navMeshAgent.enabled)
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
                    //todo: you don't have to do this if you've already this if (_enemy.currentHealth >= _enemy.maxHealth) my suggestion is either clamp when updating or remove the redundant check
                    //_enemy.currentHealth = Mathf.Min(_enemy.currentHealth + _enemy.recoveryRate * Time.deltaTime, _enemy.maxHealth);
                    
                    _enemy.currentHealth = Mathf.Clamp(_enemy.currentHealth, 0, _enemy.maxHealth); // Ensuring health doesn't exceed max 
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

        public void Exit()
        {
            Debug.Log("Exiting Recover State");
        }
    }
}
