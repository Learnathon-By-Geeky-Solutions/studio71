using UnityEngine;

namespace MachineGunAI
{
    // Alert State Implementation
    public class AlertState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float alertDuration = 1.5f;
        private float alertTimer = 0f;
        private float targetCheckInterval = 0.2f;
        private float lastCheckTime = 0f;

        // Timers for smooth transitions
        private float suppressiveFireEnterDelay = 0.2f;
        private float suppressiveFireEnterTimer = 0f;
        private float precisionFireEnterDelay = 0.2f;
        private float precisionFireEnterTimer = 0f;

        public AlertState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Alert State");
            alertTimer = 0f;
            suppressiveFireEnterTimer = 0f;
            precisionFireEnterTimer = 0f;
            lastCheckTime = Time.time; // Initialize lastCheckTime
        }

        public void UpdateState()
        {
            // Rotate towards the target
            if (gunner.Target != null)
            {
                gunner.RotateToward(gunner.Target.position, 1.5f);
            }
            else
            {
                gunner.RotateToward(gunner.LastKnownTargetPosition, 1.5f);
            }

            // Check target status at regular intervals
            if (Time.time >= lastCheckTime + targetCheckInterval)
            {
                lastCheckTime = Time.time;

                // Check if we've lost the target
                if (gunner.Target == null)
                {
                    if (!gunner.DetectTargetInAlertRange())
                    {
                        gunner.TransitionToState(gunner.idleState);
                        return;
                    }
                }

                // Check if target is out of alert range
                if (!gunner.IsTargetInAlertRange())
                {
                    gunner.TransitionToState(gunner.idleState);
                    return;
                }

                // Check for transitions to more aggressive states
                HandleCombatTransitions();
            }

            // Update alert timer
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertDuration && gunner.Target == null)
            {
                gunner.TransitionToState(gunner.idleState);
            }
        }

        private void HandleCombatTransitions()
        {
            // Check for precision fire range and line of sight
            if (gunner.IsTargetInPrecisionRange() && gunner.HasLineOfSightToTarget())
            {
                precisionFireEnterTimer += Time.deltaTime;
                if (precisionFireEnterTimer >= precisionFireEnterDelay)
                {
                    gunner.TransitionToState(gunner.precisionFireState);
                    return;
                }
            }
            else
            {
                precisionFireEnterTimer = 0f;
            }

            // If not in precision range, check for suppressive fire range
            if (gunner.IsTargetInSuppressiveRange())
            {
                suppressiveFireEnterTimer += Time.deltaTime;
                if (suppressiveFireEnterTimer >= suppressiveFireEnterDelay)
                {
                    gunner.TransitionToState(gunner.suppressiveFireState);
                    return;
                }
            }
            else
            {
                suppressiveFireEnterTimer = 0f;
            }
        }

        public void OnExit()
        {
            // Nothing specific to clean up in this state
        }
    }
}