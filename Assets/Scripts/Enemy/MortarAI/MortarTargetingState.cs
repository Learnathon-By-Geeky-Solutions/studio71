using System.Collections;
using UnityEngine;


namespace MortarAI
{
    public class MortarTargetingState : IMortarState
    {
        private MortarEnemyController controller;
        private MortarTargetingSystem targetingSystem;
        private Animator animator;
        private Coroutine targetingCoroutine;

        public MortarTargetingState(MortarEnemyController controller, MortarTargetingSystem targetingSystem, Animator animator)
        {
            this.controller = controller;
            this.targetingSystem = targetingSystem;
            this.animator = animator;
        }

        public void Enter()
        {
            animator.SetTrigger("StartAiming");

            // Start targeting coroutine
            if (controller.IsPlayerPositionKnown())
            {
                targetingCoroutine = controller.StartCoroutine(
                    targetingSystem.TargetAndFire(
                        controller.GetPlayerPosition(),
                        controller.GetPlayerVelocity()));

                // Schedule transition to firing state
                controller.StartCoroutine(TransitionToFiring());
            }
            else
            {
                // No player position known, go back to idle
                controller.EnterIdleState();
            }
        }

        public void Update()
        {
            float distanceToPlayer = controller.GetDistanceToPlayer();

            // If player gets too close during targeting and we can teleport,
            // interrupt targeting and teleport away
            if (targetingSystem.IsPlayerTooClose(distanceToPlayer) &&
                controller.GetComponent<MortarTeleporter>().CanTeleport())
            {
                controller.EnterTeleportingState();
            }
        }

        public void Exit()
        {
            // Stop coroutine if still running
            if (targetingCoroutine != null)
            {
                controller.StopCoroutine(targetingCoroutine);
                targetingCoroutine = null;
            }
        }

        private IEnumerator TransitionToFiring()
        {
            // Wait for targeting time
            yield return new WaitForSeconds(targetingSystem.GetTargetingTime());

            // Transition to firing state
            controller.EnterFiringState();
        }
    }
}