using System.Collections;
using UnityEngine;

namespace MortarAI
{
    public class MortarTeleportingState : IMortarState
    {
        private MortarEnemyController controller;
        private MortarTeleporter teleporter;
        private Animator animator;
        private Coroutine teleportCoroutine;

        public MortarTeleportingState(MortarEnemyController controller, MortarTeleporter teleporter, Animator animator)
        {
            this.controller = controller;
            this.teleporter = teleporter;
            this.animator = animator;
        }

        public void Enter()
        {
            animator.SetTrigger("Teleport");

            if (controller.IsPlayerPositionKnown())
            {
                // Get the targeting system component
                MortarTargetingSystem targetingSystem = controller.GetComponent<MortarTargetingSystem>();
                if (targetingSystem != null)
                {
                    // Start teleport coroutine
                    teleportCoroutine = controller.StartCoroutine(
                        teleporter.ExecuteTeleport(
                            controller.GetPlayerPosition(),
                            targetingSystem.GetAttackRange() * 0.8f)); // Use 80% of attack range for teleport

                    // Schedule transition back to idle
                    controller.StartCoroutine(TransitionToIdle());
                }
                else
                {
                    Debug.LogError("MortarTargetingSystem component not found on the enemy!");
                    controller.EnterIdleState();
                }
            }
            else
            {
                // No player position known, go back to idle
                controller.EnterIdleState();
            }
        }

        public void Update()
        {
            // This state is handled by coroutines
        }

        public void Exit()
        {
            // Stop coroutine if still running
            if (teleportCoroutine != null)
            {
                controller.StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
            }
        }

        private IEnumerator TransitionToIdle()
        {
            // Wait for teleport completion
            yield return new WaitForSeconds(teleporter.GetSmokeBombCooldown() * 0.1f); // Just enough time for teleport

            // Transition to idle state
            controller.EnterIdleState();
        }
    }
}