using UnityEngine;

// Precision Fire State Implementation

namespace MachineGunAI
{
    public class PrecisionFireState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;

        // Precision fire parameters
        private float fireRate = 0.07f; // Time between shots
        private float fireTimer = 0f;
        private float accuracyBase = 0.7f;
        private float accuracyMax = 0.95f;
        private float accuracyBuildTime = 2.0f;
        private float currentAccuracy;

        // Target tracking
        private float targetCheckInterval = 0.2f;
        private float lastTargetCheckTime = 0f;
        private float targetLostTime = 0f;
        private float targetMemoryDuration = 2.0f;

        public PrecisionFireState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Precision Fire State");

            // Reset timers and accuracy
            fireTimer = 0f;
            targetLostTime = 0f;
            currentAccuracy = accuracyBase;
        }

        public void UpdateState()
        {
            // Check for target status at regular intervals
            if (Time.time >= lastTargetCheckTime + targetCheckInterval)
            {
                lastTargetCheckTime = Time.time;

                // Check if we still have a target
                if (gunner.Target == null)
                {
                    targetLostTime += targetCheckInterval;

                    // If we've lost the target for too long, go back to alert state
                    if (targetLostTime >= targetMemoryDuration)
                    {
                        gunner.TransitionToState(gunner.alertState);
                        return;
                    }
                }
                else
                {
                    // Reset target lost timer if we have a target
                    targetLostTime = 0f;

                    // If target is visible but out of precision range, go to suppressive fire
                    if (!gunner.IsTargetInPrecisionRange() && gunner.IsTargetInSuppressiveRange())
                    {
                        gunner.TransitionToState(gunner.suppressiveFireState);
                        return;
                    }

                    // If target is not in line of sight, go to suppressive fire
                    if (!gunner.HasLineOfSightToTarget() && gunner.IsTargetInSuppressiveRange())
                    {
                        gunner.TransitionToState(gunner.suppressiveFireState);
                        return;
                    }
                }
            }

            // Rotate towards the target with high priority
            if (gunner.Target != null)
            {
                gunner.RotateToward(gunner.Target.position, 2.0f); // Very fast rotation in precision fire

                // Improve accuracy over time while on target
                currentAccuracy = Mathf.Lerp(accuracyBase, accuracyMax,
                    Mathf.Min(1.0f, Time.deltaTime / accuracyBuildTime));
            }
            else
            {
                // If no target, rotate towards last known position
                gunner.RotateToward(gunner.LastKnownTargetPosition, 1.5f);

                // Reduce accuracy when we don't have direct line of sight
                currentAccuracy = accuracyBase;
            }

            // Handle firing logic
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                fireTimer = 0f;

                // Fire with high accuracy for precision effect
                gunner.FireBullet(currentAccuracy);
            }
        }

        public void OnExit()
        {
            // Nothing special to clean up
        }
    }
}