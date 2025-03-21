using UnityEngine;

// Overheated State Implementation

namespace MachineGunAI
{
    public class OverheatedState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float overheatDuration = 5.0f;
        private float overheatTimer = 0f;
        private bool coolingStarted = false;
        private float targetCheckInterval = 0.5f;
        private float lastTargetCheckTime = 0f;
        private bool playedOverheatFX = false;

        // Visual effects for overheating
        private ParticleSystem steamFX;

        public OverheatedState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Overheated State");

            // Reset timers and flags
            overheatTimer = 0f;
            coolingStarted = false;
            playedOverheatFX = false;

            // Play overheat sound effect
            if (gunner.GunAudioSource && gunner.OverheatedSound)
            {
                gunner.GunAudioSource.PlayOneShot(gunner.OverheatedSound);
            }

            // Optionally create or activate steam/smoke particle effect
            // steamFX = gunner.GetComponentInChildren<ParticleSystem>();
            // if (steamFX != null)
            // {
            //     steamFX.Play();
            // }
        }

        public void UpdateState()
        {
            // Increment overheat timer
            overheatTimer += Time.deltaTime;

            // Play visual effects if not done yet
            if (!playedOverheatFX)
            {
                playedOverheatFX = true;

                // Add visual effects for overheating like steam or smoke
                // Could spawn temporary particle system or enable an existing one
            }

            // Continue to track target while cooling down
            if (Time.time >= lastTargetCheckTime + targetCheckInterval)
            {
                lastTargetCheckTime = Time.time;

                // Try to keep target awareness
                gunner.DetectTargetInAlertRange();
            }

            // If we have a target, try to keep facing it
            if (gunner.Target != null)
            {
                // Rotate very slowly while overheated
                gunner.RotateToward(gunner.Target.position, 0.2f);
            }
            else if (!Vector3.zero.Equals(gunner.LastKnownTargetPosition))
            {
                // Rotate towards last known position very slowly
                gunner.RotateToward(gunner.LastKnownTargetPosition, 0.1f);
            }

            // Start cooling procedure after a certain time
            if (!coolingStarted && overheatTimer >= 1.0f)
            {
                coolingStarted = true;

                // Optional: Play cooling animation
                // gunner.PlayAnimation("CoolWeapon");
            }

            // Check if cooling is complete
            if (overheatTimer >= overheatDuration)
            {
                // Reset heat value completely
                // The actual heat value is already managed in MachineGunnerAI.Update()

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
            // Clean up any overheat-specific state
            coolingStarted = false;

            // Stop any particle effects
            // if (steamFX != null)
            // {
            //     steamFX.Stop();
            // }
        }
    }
}