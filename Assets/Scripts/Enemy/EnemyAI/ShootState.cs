using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.AI;

namespace PatrolEnemy
{
    public class ShootState : IEnemyState
    {
        private readonly float fireRate = 0.5f;
        private float nextFireTime = 0f;
        
        private CancellationTokenSource reloadCTS;


        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Shoot State");
            nextFireTime = 0f;
            reloadCTS = new CancellationTokenSource();
        }

        public void UpdateState(EnemyController controller)
        {
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            float desiredDistance = controller.AttackRange * 0.8f;
            Vector3 directionToPlayer = (controller.transform.position - controller.CurrentTarget.position).normalized;
            Vector3 desiredPosition = controller.CurrentTarget.position + directionToPlayer * desiredDistance;

    // Set destination if too close/far
    if (Vector3.Distance(controller.transform.position, desiredPosition) > 0.8f)
    {
        controller.Agent.SetDestination(desiredPosition);
        controller.Agent.isStopped = false;
    }
    else
    {
        controller.Agent.isStopped = true;
    }

            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return;
            }

            // If player moved out of attack range, switch to follow state
            if (distanceToPlayer > controller.AttackRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
                return;
            }

            // If line of sight is lost, switch to grenade throw state if we have grenades
            if (!controller.HasLineOfSight && controller.CurrentGrenades > 0)
            {
                controller.Agent.isStopped = true;
                controller.ChangeState(EnemyController.EnemyStateType.GrenadeThrow);
                return;
            }

            // If line of sight is lost, gain LOS if we don't have grenades
             if (!controller.HasLineOfSight && controller.CurrentGrenades <= 0 )
            {
            controller.Agent.SetDestination(controller.CurrentTarget.position);
            controller.Agent.isStopped = false; // Ensure agent moves if outside attack range
            }

            // Look at player on Y axis only
            Vector3 targetPosition = controller.CurrentTarget.position;
            targetPosition.y = controller.transform.position.y;
            controller.transform.LookAt(targetPosition);

            // Shoot at player if we can
            if (Time.time >= nextFireTime && !controller.IsReloading)
            {
                if (controller.CurrentAmmo > 0)
                {
                    FireBullet(controller);
                    nextFireTime = Time.time + fireRate;
                }
                else
                {
                    ReloadWeaponAsync(controller).Forget();
                }
            }
        }

        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Shoot State");
            reloadCTS?.Cancel();
            reloadCTS?.Dispose();
            reloadCTS = null;
        }

        private void FireBullet(EnemyController controller)
        {
            
            controller.CurrentAmmo--;
            Debug.Log($"Fired bullet. Ammo remaining: {controller.CurrentAmmo}");

            if (controller.CurrentAmmo <= 0)
            {
                ReloadWeaponAsync(controller).Forget();
            }
        }

        private async UniTaskVoid ReloadWeaponAsync(EnemyController controller)
        {
            Debug.Log("Reloading weapon...");
            controller.IsReloading = true;

            try
            {
                await UniTask.Delay((int)(controller.ReloadTime * 1000), cancellationToken: reloadCTS.Token);
                controller.CurrentAmmo = controller.MaxAmmo;
                controller.IsReloading = false;
                Debug.Log("Weapon reloaded!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Reload interrupted");
                controller.IsReloading = false;
            }
        }
    }
}