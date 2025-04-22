using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using SingletonManagers;

namespace PatrolEnemy
{
    public class GrenadeThrowState : IEnemyState
    {
        private CancellationTokenSource grenadeCTS;
        private bool hasThrown = false;
        
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Grenade Throw State");
            hasThrown = false;
            grenadeCTS = new CancellationTokenSource();
        }
        
        public void UpdateState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            // If player moved out of attack range, switch to follow state
            if (distanceToPlayer > controller.AttackRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
                return;
            }
            
            // If line of sight is gained, switch to shoot state
            if (controller.HasLineOfSight)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Shoot);
                return;
            }
            
            // If we're out of grenades, switch to shoot state
            if (controller.CurrentGrenades <= 0)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Shoot);
                return;
            }
            
            // Look at player position
            Vector3 targetPosition = controller.CurrentTarget.position;
            targetPosition.y = controller.transform.position.y;
            controller.transform.LookAt(targetPosition);
            
            // Throw grenade if we haven't already
            if (!hasThrown && !controller.IsThrowingGrenade && controller.CurrentGrenades > 0)
            {
                ThrowGrenadeAsync(controller);
                hasThrown = true;
            }
            
            // Wait for cooldown then switch back to follow state
            if (hasThrown && !controller.IsThrowingGrenade)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Grenade Throw State");
            grenadeCTS?.Cancel();
            grenadeCTS?.Dispose();
            grenadeCTS = null;
        }

        private async void ThrowGrenadeAsync(EnemyController controller)
        {
            if (controller.CurrentGrenades <= 0)
                return;
                
            controller.IsThrowingGrenade = true;
            
            // Use the model of the particle manager for our grenade
            Vector3 targetPosition = controller.CurrentTarget.position;
            GameObject grenade = GameObject.Instantiate(controller.GrenadePrefab, controller.GrenadePoint.position, controller.GrenadePoint.rotation);
            
            // Setup grenade trajectory (this would need a separate component on the grenade prefab)
            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
            if (grenadeRb != null)
            {
                // Calculate arc trajectory
                Vector3 directionToTarget = (targetPosition - controller.GrenadePoint.position).normalized;
                float distanceToTarget = Vector3.Distance(controller.GrenadePoint.position, targetPosition);
                
                // Adjust force based on distance
                float throwForce = Mathf.Clamp(distanceToTarget * 2f, 10f, 20f);
                Vector3 throwDirection = directionToTarget + Vector3.up * 0.5f;
                grenadeRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
            }
            
            // Play explosion effect after delay
            
            controller.CurrentGrenades--;
            Debug.Log($"Threw grenade. Grenades remaining: {controller.CurrentGrenades}");
            
            try
            {
                await Task.Delay((int)(controller.GrenadeThrowCooldown * 1000), grenadeCTS.Token);
                controller.IsThrowingGrenade = false;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Grenade cooldown interrupted");
                controller.IsThrowingGrenade = false;
            }
        }
        
    }
    
   
}