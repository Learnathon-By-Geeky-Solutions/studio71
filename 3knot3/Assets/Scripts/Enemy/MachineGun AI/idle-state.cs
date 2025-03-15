using UnityEngine;

// Idle State Implementation

namespace MachineGunAI
{
    public class IdleState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float scanTime = 0f;
        private float scanDirection = 1f; // 1 for right, -1 for left
        private float detectionCheckInterval = 0.5f;
        private float lastCheckTime = 0f;

        private Vector3 defaultForward;
        private float currentScanAngle = 0f;

        public IdleState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            // Store default forward direction
            defaultForward = gunner.transform.forward;
            currentScanAngle = 0f;

            // Clear any previous targets
            gunner.ClearTarget();

            Debug.Log("Machine Gunner entered Idle State");
        }

        public void UpdateState()
        {
            // Check for targets at regular intervals
            if (Time.time >= lastCheckTime + detectionCheckInterval)
            {
                lastCheckTime = Time.time;

                // Check for targets within alert range
                if (gunner.DetectTargetInAlertRange())
                {
                    gunner.TransitionToState(gunner.alertState);
                    return;
                }
            }

            // Handle scanning behavior
            UpdateScanPattern();
        }

        private void UpdateScanPattern()
        {
            // Calculate scan angle
            currentScanAngle += scanDirection * Time.deltaTime * gunner.ScanSpeed;

            // Check if we need to reverse direction
            if (Mathf.Abs(currentScanAngle) >= gunner.ScanAngle / 2)
            {
                scanDirection *= -1; // Reverse direction
                currentScanAngle = Mathf.Clamp(currentScanAngle, -gunner.ScanAngle / 2, gunner.ScanAngle / 2);
            }

            // Create rotation based on current scan angle
            Quaternion targetRotation =
                Quaternion.Euler(0, currentScanAngle, 0) * Quaternion.LookRotation(defaultForward);

            // Apply rotation to gun pivot
            gunner.GunPivot.rotation = Quaternion.Slerp(
                gunner.GunPivot.rotation,
                targetRotation,
                Time.deltaTime * 2f // Smooth rotation speed
            );
        }

        public void OnExit()
        {
            // Nothing specific to clean up in this state
        }
    }
}