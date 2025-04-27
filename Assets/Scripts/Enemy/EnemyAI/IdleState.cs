using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

namespace PatrolEnemy
{
    public class IdleState : IEnemyState
    {
        private float patrolWaitTime = 3f;
        private bool returningToZone = false;
        private bool isRunning = false;

        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Idle State");
            isRunning = true;

            // Check if enemy needs to return to patrol zone
            float distanceFromStart = Vector3.Distance(controller.transform.position, controller.InitialPosition);
            if (distanceFromStart > controller.PatrolRange * 0.8f)
            {
                returningToZone = true;
                MoveToPosition(controller, controller.InitialPosition);
            }
            else
            {
                // Start the patrol routine
                StartPatrolRoutine(controller);
            }
        }

        public void UpdateState(EnemyController controller)
        {
            // Player detection check
            if (controller.CurrentTarget != null &&
                Vector3.Distance(controller.transform.position, controller.CurrentTarget.position) <= controller.DetectionRange)
            {
                isRunning = false;
                controller.ChangeState(EnemyController.EnemyStateType.Alert);
                return;
            }
        }

        private async void StartPatrolRoutine(EnemyController controller)
        {
            if (!isRunning) return;

            try
            {
                while (isRunning)
                {
                    // 1. Generate random patrol point
                    Vector3 patrolPoint = GetPatrolPoint(controller);

                    // 2. Move to patrol point with IsIdleAlert = false
                    controller.IsIdleAlert = false;
                    await MoveToPosition(controller, patrolPoint);

                    if (!isRunning) break;

                    // 3. Wait at patrol point with IsIdleAlert = true
                    controller.IsIdleAlert = true;
                    await UniTask.Delay(System.TimeSpan.FromSeconds(patrolWaitTime));

                    if (!isRunning) break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in patrol routine: {e.Message}");
            }
        }

        private async UniTask MoveToPosition(EnemyController controller, Vector3 position)
        {
            controller.Agent.SetDestination(position);

            // Wait until reaching destination or state is exited
            while (isRunning && (controller.Agent.pathPending ||
                  controller.Agent.remainingDistance > controller.Agent.stoppingDistance))
            {
                await UniTask.Yield();
            }

            // Handle returning to zone completion
            if (returningToZone && isRunning)
            {
                returningToZone = false;
                StartPatrolRoutine(controller);
            }
        }

        private Vector3 GetPatrolPoint(EnemyController controller)
        {
            // Generate random point within patrol range
            Vector2 randomCirclePoint = Random.insideUnitCircle * controller.PatrolRange;
            Vector3 randomPoint = controller.InitialPosition + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);

            // Validate NavMesh position
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, controller.PatrolRange, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return controller.InitialPosition; // Fallback
        }

        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Idle State");
            isRunning = false;
        }
    }
}