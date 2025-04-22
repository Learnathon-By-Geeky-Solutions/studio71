using UnityEngine;

namespace PatrolEnemy
{
    public class AlertState : IEnemyState
    {
        private float alertTimer = 0f;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Alert State");
            alertTimer = 0f;
            controller.AlertTime = 3f; // Reset alert countdown
            controller.Agent.isStopped = true; // Stop moving while in alert state
        }
        
        public void UpdateState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(new IdleState());
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            // If player moved out of detection range, return to idle
            if (distanceToPlayer > controller.DetectionRange)
            {
                controller.ChangeState(new IdleState());
                return;
            }
            
            // Look at player on Y axis only
            Vector3 targetPosition = controller.CurrentTarget.position;
            targetPosition.y = controller.transform.position.y;
            controller.transform.LookAt(targetPosition);
            
            // If line of sight is clear, count down alert timer
            if (controller.HasLineOfSight)
            {
                alertTimer += Time.deltaTime;
                Debug.Log($"Alert countdown: {controller.AlertTime - alertTimer}");
                
                // When timer reaches alert time, start following
                if (alertTimer >= controller.AlertTime)
                {
                    controller.ChangeState(new FollowState());
                    return;
                }
            }
            else
            {
                // Reset timer if line of sight is lost
                alertTimer = 0f;
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Alert State");
            controller.Agent.isStopped = false;
        }
        
        public void OnDrawGizmos(EnemyController controller)
        {
            if (controller.CurrentTarget != null)
            {
                // Draw line to player
                Gizmos.color = controller.HasLineOfSight ? Color.red : Color.gray;
                Gizmos.DrawLine(controller.transform.position, controller.CurrentTarget.position);
                
                // Draw alert progress
                Vector3 textPosition = controller.transform.position + Vector3.up * 2f;
                float alertProgress = alertTimer / controller.AlertTime;
                Debug.DrawLine(textPosition, textPosition + Vector3.right * alertProgress * 2f, Color.yellow);
            }
        }
    }
}