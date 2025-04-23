using UnityEngine;
using UnityEngine.AI;

namespace PatrolEnemy
{
    public class RecoveryState : IEnemyState
    {
        private bool isReturning = false;
         private float lastHealthRecoveryTime = 0f;
        private float healthRecoveryInterval = 0.5f;
      

        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Recovery State");
            isReturning = false;
            controller.Agent.isStopped = false;
            controller.Agent.ResetPath();
     
        }

       public void UpdateState(EnemyController controller)
{
    if (!isReturning)
    {
        // Set the destination to the initial position
        controller.Agent.SetDestination(controller.InitialPosition);
        isReturning = true;
        Debug.Log($"Returning to initial position: {controller.InitialPosition} for recovery.");
        lastHealthRecoveryTime = Time.time; // Initialize recovery timer
    }
    else if (controller.Agent.remainingDistance <= controller.Agent.stoppingDistance && !controller.Agent.pathPending)
    {
        controller.Agent.isStopped = true; // Stop at the initial position
        Debug.Log("Reached initial position. Starting recovery.");

        // Recover health over time
        if (Time.time >= lastHealthRecoveryTime + healthRecoveryInterval)
        {
            controller.CurrentHealth = Mathf.Min(controller.CurrentHealth + controller.RecoveryRate * Time.deltaTime, controller.MaxHealth);
            lastHealthRecoveryTime = Time.time;
            Debug.Log($"Recovering at initial position. Current health: {controller.CurrentHealth}");

            // If health is fully recovered, go back to Idle state
            if (controller.CurrentHealth >= controller.MaxHealth)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return; // Exit UpdateState
            }
        }
    }
    else if (isReturning && !controller.Agent.hasPath)
    {
        // Path failed, might need to handle this (e.g., try again, or just recover in place)
        Debug.LogWarning("Path to initial position failed.");
        controller.Agent.isStopped = true;
        // Still attempt to recover in place
        if (Time.time >= lastHealthRecoveryTime + healthRecoveryInterval)
        {
            controller.CurrentHealth = Mathf.Min(controller.CurrentHealth + controller.RecoveryRate * Time.deltaTime, controller.MaxHealth);
            lastHealthRecoveryTime = Time.time;
            Debug.Log($"Recovering in place (path failed). Current health: {controller.CurrentHealth}");
            if (controller.CurrentHealth >= controller.MaxHealth)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return; // Exit UpdateState
            }
        }
    }
}
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Recovery State");
            controller.Agent.isStopped = false;
        }
    }
}