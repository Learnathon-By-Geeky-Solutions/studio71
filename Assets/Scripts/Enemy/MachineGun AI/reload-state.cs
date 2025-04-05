using UnityEngine;

// Reload State Implementation

namespace MachineGunAI
{
    public class ReloadState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float reloadTimer = 0f;
        private bool reloadStarted = false;
        private float targetCheckInterval = 0.5f;
        private float lastTargetCheckTime = 0f;

        public ReloadState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Reload State");

            // Reset reload timer and flag
            reloadTimer = 0f;
            reloadStarted = false;

            // Play reload animation or sound immediately when entering state
            if (gunner.GunAudioSource && gunner.ReloadSound)
            {
                gunner.GunAudioSource.PlayOneShot(gunner.ReloadSound);
            }
        }

        public void UpdateState()
        {
            // Increment reload timer
            reloadTimer += Time.deltaTime;

            // Check if we need to reload
            if (!reloadStarted && reloadTimer >= 0.5f) // Small delay before actual reload begins
            {
                reloadStarted = true;

                // Optional: Play reload animation
                // gunner.PlayAnimation("Reload");
            }

            // Continue to track target while reloading if possible
            if (Time.time >= lastTargetCheckTime + targetCheckInterval)
            {
                lastTargetCheckTime = Time.time;

                // Try to keep target awareness during reload
                gunner.DetectTargetInAlertRange();
            }

            // If we have a target, try to keep facing it
            if (gunner.Target != null)
            {
                // Rotate slowly during reload
                gunner.RotateToward(gunner.Target.position, 0.3f);
            }
            else if (!Vector3.zero.Equals(gunner.LastKnownTargetPosition))
            {
                // Rotate towards last known position slowly
                gunner.RotateToward(gunner.LastKnownTargetPosition, 0.2f);
            }

            // Check if reload is complete
            if (reloadTimer >= gunner.ReloadTime)
            {
                // Perform the actual reload
                gunner.ReloadMagazine();

                // Transition to appropriate state based on target status
                if (gunner.Target != null)
                {
                    if (gunner.IsTargetInPrecisionRange() && gunner.HasLineOfSightToTarget())
                    {
                        gunner.TransitionToState(gunner.precisionFireState);
                    }
                    else if (gunner.IsTargetInSuppressiveRange())
                    {
                        gunner.TransitionToState(gunner.suppressiveFireState);
                    }
                    else
                    {
                        gunner.TransitionToState(gunner.alertState);
                    }
                }
                else
                {
                    // No target, go back to alert or idle
                    gunner.TransitionToState(gunner.alertState);
                }
            }
        }

        public void OnExit()
        {
            // Clean up any reload-specific state
            reloadStarted = false;
        }
    }
}
