using UnityEngine;

namespace patrolEnemy
{
    public class ShootState : IEnemyState
    {
        private readonly EnemyAI _enemy;
        private float _shootTimer = 0f;
        private float _repositionTimer = 0f;
        private readonly float _repositionInterval = 3f;
        public bool IsRepositioning { get; private set; } = false;

        public ShootState(EnemyAI enemyAI)
        {
            _enemy = enemyAI;
        }

        public void Enter()
        {
            Debug.Log("Entering Shoot State");

            // Reset timers
            _shootTimer = 0f;
            _repositionTimer = 0f;
            IsRepositioning = false;

            // Stop moving initially to shoot
            _enemy.navMeshAgent.ResetPath();
        }

        public void Execute()
        {
            // Always look at player
            LookAtPlayer();

            // Check if player is out of attack range
            if (!_enemy.playerInAttackRange)
            {
                _enemy.ChangeState(_enemy.followState);
                return;
            }

            // Handle repositioning when no line of sight
            if (!_enemy.playerInLineOfSight && _enemy.currentGrenades <= 0)
            {
                HandleRepositioning();
                return;
            }

            // Switch to grenade throw if we have grenades and lose line of sight
            if (!_enemy.playerInLineOfSight && _enemy.currentGrenades > 0)
            {
                _enemy.ChangeState(_enemy.grenadeThrowState);
                return;
            }

            // We have line of sight, stop moving and shoot
            if (IsRepositioning)
            {
                StopRepositioning();
            }

            // Shooting logic
            HandleShooting();
        }

        private void LookAtPlayer()
        {
            if (_enemy.player != null)
            {
                Vector3 lookDirection = _enemy.player.position - _enemy.transform.position;
                lookDirection.y = 0;

                if (lookDirection != Vector3.zero)
                {
                    _enemy.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }

        private void HandleRepositioning()
        {
            if (!IsRepositioning)
            {
                StartRepositioning();
            }
            else
            {
                CheckRepositionTarget();
            }
        }

        private void StartRepositioning()
        {
            // Declare repositionTarget as a local variable
            Vector3 repositionTarget = _enemy.FindPositionWithLineOfSight();
            _enemy.navMeshAgent.SetDestination(repositionTarget);
            IsRepositioning = true;

            Debug.Log("Repositioning Target");
        }

        private void CheckRepositionTarget()
        {
            // Declare repositionTarget as a local variable
            Vector3 repositionTarget = _enemy.FindPositionWithLineOfSight();

            // Check if we've reached the repositioning target
            if (_enemy.navMeshAgent.remainingDistance <= _enemy.navMeshAgent.stoppingDistance)
            {
                _repositionTimer += Time.deltaTime;

                // Try another position after a delay if still no line of sight
                if (_repositionTimer >= _repositionInterval)
                {
                    _repositionTimer = 0f;
                    _enemy.navMeshAgent.SetDestination(repositionTarget);
                }
            }
        }

        private void StopRepositioning()
        {
            _enemy.navMeshAgent.ResetPath();
            IsRepositioning = false;
        }

        private void HandleShooting()
        {
            if (!_enemy.isReloading)
            {
                _shootTimer += Time.deltaTime;

                if (_shootTimer >= 1f / _enemy.fireRate)
                {
                    _shootTimer = 0f;
                    _enemy.Shoot();
                }
            }
        }

        public void Exit()
        {
            Debug.Log("Exiting Shoot State");
        }
    }
}
