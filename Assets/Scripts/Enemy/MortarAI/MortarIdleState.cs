using UnityEngine;

namespace MortarAI
{
    public class MortarIdleState : IMortarState
    {
        private MortarEnemyController controller;
        private Animator animator;

        public MortarIdleState(MortarEnemyController controller, Animator animator)
        {
            this.controller = controller;
            this.animator = animator;
        }

        public void Enter()
        {
            animator.SetTrigger("Idle");
        }

        public void Update()
        {
            if (!controller.IsPlayerPositionKnown())
                return;

            float distanceToPlayer = controller.GetDistanceToPlayer();
            MortarTargetingSystem targetingSystem = controller.GetComponent<MortarTargetingSystem>();
            MortarTeleporter teleporter = controller.GetComponent<MortarTeleporter>();

            // Check if player is too close and we can teleport
            if (targetingSystem.IsPlayerTooClose(distanceToPlayer) && teleporter.CanTeleport())
            {
                controller.EnterTeleportingState();
                return;
            }

            // Check if in range and can attack
            if (targetingSystem.IsInAttackRange(distanceToPlayer) && targetingSystem.CanAttack())
            {
                controller.EnterTargetingState();
                return;
            }

            // Look at player
            LookAtPlayer();
        }

        public void Exit()
        {
            // Nothing specific to clean up
        }

        private void LookAtPlayer()
        {
            if (!controller.IsPlayerPositionKnown())
                return;

            Vector3 direction = controller.GetPlayerPosition() - controller.transform.position;
            direction.y = 0; // Keep the enemy level

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                controller.transform.rotation = Quaternion.Slerp(
                    controller.transform.rotation,
                    lookRotation,
                    Time.deltaTime * 5f);
            }
        }
    }
}