using UnityEngine;

namespace MortarAI
{
    public class MortarCooldownState : IMortarState
    {
        private MortarEnemyController controller;
        private Animator animator;
        private float cooldownStartTime;

        public MortarCooldownState(MortarEnemyController controller, Animator animator)
        {
            this.controller = controller;
            this.animator = animator;
        }

        public void Enter()
        {
            animator.SetTrigger("Cooldown");
            cooldownStartTime = Time.time;
        }

        public void Update()
        {
            float distanceToPlayer = controller.GetDistanceToPlayer();
            MortarTargetingSystem targetingSystem = controller.GetComponent<MortarTargetingSystem>();
            MortarTeleporter teleporter = controller.GetComponent<MortarTeleporter>();

            // If player gets too close during cooldown and we can teleport
            if (targetingSystem.IsPlayerTooClose(distanceToPlayer) && teleporter.CanTeleport())
            {
                controller.EnterTeleportingState();
                return;
            }

            // Check if cooldown is complete
            if (Time.time >= cooldownStartTime + targetingSystem.GetAttackCooldown())
            {
                controller.EnterIdleState();
            }
        }

        public void Exit()
        {
            // Nothing specific to clean up
        }
    }
}