using UnityEngine;

namespace MachineGunAI
{
// Alert State Implementation
    public class AlertState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float alertDuration = 1.5f; // Time to stay in alert state before transitioning
        private float alertTimer = 0f;
        private float targetCheckInterval = 0.2f;
        private float lastCheckTime = 0f;

        public AlertState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            // Reset alert timer
            alertTimer = 0f;

            Debug.Log("Machine Gunner entered Alert State");

            // Optional: Play alert sound or animation
            // gunner.GunAudioSource.PlayOneShot(gunner.AlertSound);
        }

        public void UpdateState()
        {
            // Rotate towards the target
            if (gunner.Target != null)
            {
                gunner.RotateToward(gunner.Target.position, 1.5f); // Faster rotation in alert state
            }
            else
            {
                // If no target, rotate towards last known position
                gunner.RotateToward(gunner.LastKnownTargetPosition, 1.5f);
            }

            // Check target status at regular intervals
            if (Time.time >= lastCheckTime + targetCheckInterval)
            {
                lastCheckTime = Time.time;

                // Check if we've lost the target
                if (gunner.Target == null)
                {
                    // Try to re-detect or return to idle
                    if (!gunner.DetectTargetInAlertRange())
                    {
                        // Return to idle if we can't find the target
                        gunner.TransitionToState(gunner.idleState);
                        return;
                    }
                }

                // Check if target is in suppressive fire range
                if (gunner.IsTargetInSuppressiveRange())
                {
                    // If we can see the target directly, go to precision fire
                    if (gunner.HasLineOfSightToTarget() && gunner.IsTargetInPrecisionRange())
                    {
                        gunner.TransitionToState(gunner.precisionFireState);
                        return;
                    }
                    // Otherwise, go to suppressive fire
                    else
                    {
                        gunner.TransitionToState(gunner.suppressiveFireState);
                        return;
                    }
                }
            }

            // Update alert timer
            alertTimer += Time.deltaTime;

            // If we've been in alert state too long without a valid target, return to idle
            if (alertTimer >= alertDuration && gunner.Target == null)
            {
                gunner.TransitionToState(gunner.idleState);
            }
        }

        public void OnExit()
        {
            // Nothing specific to clean up in this state
        }
    }
}