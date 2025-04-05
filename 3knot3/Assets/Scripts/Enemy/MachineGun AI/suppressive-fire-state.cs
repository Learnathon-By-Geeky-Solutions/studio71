using UnityEngine;

namespace MachineGunAI
{
    // Suppressive Fire State Implementation
    public class SuppressiveFireState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float fireRate = 0.1f;
        private float fireTimer = 0f;

        // Timers for smooth transitions
        private float alertStateEnterDelay = 0.3f;
        private float alertStateEnterTimer = 0f;
        private float precisionStateEnterDelay = 0.2f;
        private float precisionStateEnterTimer = 0f;
        private float timeSinceLastSeen = 0f;
        private float targetLostDuration = 1.0f; // How long to remember the target

        public SuppressiveFireState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Suppressive Fire State");
            fireTimer = 0f;
            alertStateEnterTimer = 0f;
            precisionStateEnterTimer = 0f;
            timeSinceLastSeen = 0f;
        }

        public void UpdateState()
        {
            // Rotate towards the target
            if (gunner.Target != null)
            {
                gunner.RotateToward(gunner.Target.position, 0.8f);
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
                gunner.FireBullet(0.7f);
                fireTimer = 0f;
            }

            // Check for state transitions
            HandleStateTransitions();
        }

        private void HandleStateTransitions()
        {
            // Check for precision fire
            if (gunner.IsTargetInPrecisionRange() && gunner.HasLineOfSightToTarget())
            {
                precisionStateEnterTimer += Time.deltaTime;
                if (precisionStateEnterTimer >= precisionStateEnterDelay)
                {
                    gunner.TransitionToState(gunner.precisionFireState);
                    return;
                }
            }
            else
            {
                precisionStateEnterTimer = 0f;
            }

            // Check if target is lost or out of range
            if (gunner.Target == null || timeSinceLastSeen >= targetLostDuration ||
                !gunner.IsTargetInSuppressiveRange())
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
        }

        public void OnExit()
        {
            // Nothing specific to clean up in this state
        }
    }
}