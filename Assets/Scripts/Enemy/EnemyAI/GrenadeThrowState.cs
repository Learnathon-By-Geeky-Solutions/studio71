using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;


namespace PatrolEnemy
{
    public class GrenadeThrowState : IEnemyState
    {
        private CancellationTokenSource grenadeCTS;
        private bool isThrowing;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Grenade Throw State");
            grenadeCTS = new CancellationTokenSource();
            isThrowing = false;
            controller.IsNotGrenadeThrowing = true;
            AttemptGrenadeThrow(controller).Forget();
        }
        
        public void UpdateState(EnemyController controller)
        {
            // Immediately exit if invalid
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return;
            }

            // If player regains LOS or we run out of grenades, switch to shoot
            if (controller.HasLineOfSight || controller.CurrentGrenades <= 0)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Shoot);
                return;
            }

            // If out of attack range, chase player
            float distance = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            if (distance > controller.AttackRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
                return;
            }

            // Face player (Y-axis only)
            Vector3 lookPos = controller.CurrentTarget.position;
            lookPos.y = controller.transform.position.y;
            controller.transform.LookAt(lookPos);
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Grenade Throw State");
            grenadeCTS?.Cancel();
            grenadeCTS?.Dispose();
        }

        private async UniTaskVoid AttemptGrenadeThrow(EnemyController controller)
        {
            while (!grenadeCTS.IsCancellationRequested && 
                   controller.CurrentGrenades > 0 && 
                   !controller.HasLineOfSight)
            {
                isThrowing = true;
                controller.Agent.isStopped = true;
                controller.IsNotGrenadeThrowing = false;
                
                // Throw grenade
                GameObject grenade = GameObject.Instantiate(
                    controller.GrenadePrefab,
                    controller.GrenadePoint.position,
                    controller.GrenadePoint.rotation
                );

                // Calculate trajectory (with slight randomness)
                Vector3 targetPos = controller.CurrentTarget.position;
                Vector3 direction = (targetPos - controller.GrenadePoint.position).normalized;
                float force = Mathf.Clamp(Vector3.Distance(controller.transform.position, targetPos) * 2f, 10f, 20f);
                grenade.GetComponent<Rigidbody>().AddForce((direction + Vector3.up * 0.5f) * force, ForceMode.Impulse);
                
                controller.CurrentGrenades--;
                Debug.Log($"Threw grenade. Left: {controller.CurrentGrenades}");

                // Wait for cooldown (while still checking conditions)
                try
                {
                    await UniTask.Delay(
                        (int)(controller.GrenadeThrowCooldown * 1000),
                        cancellationToken: grenadeCTS.Token
                    );
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                isThrowing = false;
                controller.IsNotGrenadeThrowing = false;

            }
        }

    }
}