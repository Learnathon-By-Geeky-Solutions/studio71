using System.Collections;
using UnityEngine;

namespace MortarAI
{
    public class MortarFiringState : IMortarState
    {
        private MortarEnemyController controller;
        private MortarTargetingSystem targetingSystem;
        private Animator animator;

        public MortarFiringState(MortarEnemyController controller, MortarTargetingSystem targetingSystem, Animator animator)
        {
            this.controller = controller;
            this.targetingSystem = targetingSystem;
            this.animator = animator;
        }

        public void Enter()
        {
            animator.SetTrigger("Fire");

            // Fire the mortar projectile
            targetingSystem.FireProjectile();

            // Schedule transition to cooldown state
            controller.StartCoroutine(TransitionToCooldown());
        }

        public void Update()
        {
            // This state is very brief, nothing to do in update
        }

        public void Exit()
        {
            // Nothing specific to clean up
        }

        private IEnumerator TransitionToCooldown()
        {
            // Brief delay for firing animation
            yield return new WaitForSeconds(0.5f);

            // Transition to cooldown state
            controller.EnterCooldownState();
        }
    }
}