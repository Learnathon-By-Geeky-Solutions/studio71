using UnityEngine;
using System.Collections;

namespace MachineGunAI
{

// Suppressive Fire State Implementation
    public class SuppressiveFireState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;

        // Suppressive fire parameters
        private float burstDuration = 2.0f;
        private float burstInterval = 0.5f;
        private float fireRate = 0.1f; // Time between shots
        private float sweepAngle = 30f;
        private float sweepSpeed = 10f;

        // State tracking
        private bool isFiring = false;
        private float burstTimer = 0f;
        private float fireTimer = 0f;
        private float lastTargetCheckTime = 0f;
        private float targetCheckInterval = 0.5f;
        private float originalAngle = 0f;
        private float currentSweepAngle = 0f;
        private float sweepDirection = 1f;

        // Cone of suppression
        private Vector3 suppressionCenter;
        private float suppressionRadius = 5f;

        public SuppressiveFireState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Suppressive Fire State");

            // Set initial parameters
            isFiring = true;
            burstTimer = 0f;
            fireTimer = 0f;

            // Store original angle for reference
            originalAngle = gunner.GunPivot.eulerAngles.y;
            currentSweepAngle = 0f;

            // Set suppression center to target position or last known position
            suppressionCenter = gunner.Target != null ? gunner.Target.position : gunner.LastKnownTargetPosition;

            // Start with a sweeping motion
            sweepDirection = Random.value > 0.5f ? 1f : -1f;
        }

        public void UpdateState()
        {
            // Check for target updates
            if (Time.time >= lastTargetCheckTime + targetCheckInterval)
            {
                lastTargetCheckTime = Time.time;

                // Update suppression center if target is visible
                if (gunner.Target != null)
                {
                    suppressionCenter = gunner.Target.position;

                    // If target is in precision range and visible, switch to precision fire
                    if (gunner.IsTargetInPrecisionRange() && gunner.HasLineOfSightToTarget())
                    {
                        gunner.TransitionToState(gunner.precisionFireState);
                        return;
                    }
                }

                // If we've lost the target completely, go back to alert state
                if (gunner.Target == null && !gunner.DetectTargetInAlertRange())
                {
                    gunner.TransitionToState(gunner.alertState);
                    return;
                }
            }

            // Update burst timing
            burstTimer += Time.deltaTime;

            // Toggle firing state based on burst timing
            if (burstTimer >= burstDuration && isFiring)
            {
                isFiring = false;
                burstTimer = 0f;
            }
            else if (burstTimer >= burstInterval && !isFiring)
            {
                isFiring = true;
                burstTimer = 0f;

                // Adjust suppression center each burst
                suppressionCenter += new Vector3(
                    Random.Range(-suppressionRadius, suppressionRadius),
                    0,
                    Random.Range(-suppressionRadius, suppressionRadius)
                );
            }

            // Handle firing logic
            if (isFiring)
            {
                // Update sweep angle for suppressive fire
                currentSweepAngle += sweepDirection * sweepSpeed * Time.deltaTime;

                // Reverse sweep direction if we hit the edge of the sweep range
                if (Mathf.Abs(currentSweepAngle) >= sweepAngle / 2)
                {
                    sweepDirection *= -1;
                    currentSweepAngle = Mathf.Clamp(currentSweepAngle, -sweepAngle / 2, sweepAngle / 2);
                }

                // Calculate direction to suppression center
                Vector3 directionToCenter = suppressionCenter - gunner.transform.position;
                directionToCenter.y = 0; // Keep on horizontal plane

                // Create a rotation based on the sweep angle
                Quaternion baseRotation = Quaternion.LookRotation(directionToCenter);
                Quaternion sweepRotation = Quaternion.Euler(0, currentSweepAngle, 0) * baseRotation;

                // Apply rotation to gun pivot
                gunner.GunPivot.rotation = Quaternion.Slerp(
                    gunner.GunPivot.rotation,
                    sweepRotation,
                    Time.deltaTime * 3f // Fast rotation for suppressive fire
                );

                // Handle firing rate
                fireTimer += Time.deltaTime;
                if (fireTimer >= fireRate)
                {
                    fireTimer = 0f;

                    // Fire with low accuracy for suppressive effect
                    gunner.FireBullet(0.3f); // 30% accuracy
                }
            }
            else
            {
                // When not firing, rotate towards the general direction of the suppression center
                gunner.RotateToward(suppressionCenter, 0.5f);
            }
        }

        public void OnExit()
        {
            // Stop firing
            isFiring = false;
        }
    }
}
