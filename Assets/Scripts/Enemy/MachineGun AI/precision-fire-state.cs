using UnityEngine;

namespace MachineGunAI
{
    // Precision Fire State Implementation
    public class PrecisionFireState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float fireRate = 0.2f;
        private float fireTimer = 0f;

        // Timers for smooth transitions and target persistence
        private float suppressiveStateEnterDelay = 0.3f;
        private float suppressiveStateEnterTimer = 0f;
        private float alertStateEnterDelay = 0.5f;
        private float alertStateEnterTimer = 0f;
        private float timeSinceLastSeen = 0f;
        private float targetLostDuration = 1.5f; // How long to remember the target

        public PrecisionFireState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Precision Fire State");
            fireTimer = 0f;
            suppressiveStateEnterTimer = 0f;
            alertStateEnterTimer = 0f;
            timeSinceLastSeen = 0f;
        }

        public void UpdateState()
        {
            // Rotate towards the target
            if (gunner.Target != null)
            {
                gunner.RotateToward(gunner.Target.position, 1f);
                timeSinceLastSeen = 0f; // Reset timer when target is seen
            }
            else
            {
                timeSinceLastSeen += Time.deltaTime; // Increment timer when target is lost
            }

            // Fire at target
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                gunner.FireBullet(0.9f);
                fireTimer = 0f;
            }

            // Check for state transitions
            HandleStateTransitions();
        }

        private void HandleStateTransitions()
        {
            // Check if target is lost or out of range
            if (gunner.Target == null || timeSinceLastSeen >= targetLostDuration ||
                !gunner.IsTargetInPrecisionRange() || !gunner.HasLineOfSightToTarget())
            {
                // If still in suppressive range, go back to suppressive after delay
                if (gunner.IsTargetInSuppressiveRange())
                {
                    suppressiveStateEnterTimer += Time.deltaTime;
                    if (suppressiveStateEnterTimer >= suppressiveStateEnterDelay)
                    {
                        gunner.TransitionToState(gunner.suppressiveFireState);
                        return;
                    }
                }
                else
                {
                    suppressiveStateEnterTimer = 0f;
                }

                // If no longer in suppressive range, go back to alert after delay
                if (!gunner.IsTargetInSuppressiveRange() && gunner.IsTargetInAlertRange())
                {
                    alertStateEnterTimer += Time.deltaTime;
                    if (alertStateEnterTimer >= alertStateEnterDelay)
                    {
                        gunner.TransitionToState(gunner.alertState);
                        return;
                    }
                }
                else
                {
                    alertStateEnterTimer = 0f;
                }

                // If not in alert range, go back to idle
                if (!gunner.IsTargetInAlertRange())
                {
                    gunner.TransitionToState(gunner.idleState);
                    return;
                }
            }
            else
            {
                // Reset timers if conditions are not met
                suppressiveStateEnterTimer = 0f;
                alertStateEnterTimer = 0f;
            }
        }

        public void OnExit()
        {
            // Nothing specific to clean up in this state
        }
    }
}