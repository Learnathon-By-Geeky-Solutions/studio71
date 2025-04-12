using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class ShootState : IMachineGunnerState
    {
        private float _burstTimer = 0f;
        private float _sweepAngleProgress = 0f;
        private Quaternion _initialRotation;

        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Shoot State");
            _burstTimer = 0f;
            _sweepAngleProgress = 0f;
            _initialRotation = controller.firePoint.localRotation; // Store initial local rotation
        }

        public void UpdateState(MachineGunnerController controller)
        {
            if (controller.Player != null)
            {
                // Look at the player first
                Vector3 directionToPlayer = controller.Player.transform.position - controller.transform.position;
                directionToPlayer.y = 0f;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f);

                if (controller.HasLineOfSightToPlayer())
                {
                    // Apply sweep rotation during burst
                    float sweep = Mathf.Sin(_sweepAngleProgress * Mathf.PI) * controller.BurstSweepAngle;
                    Quaternion sweepRotation = Quaternion.Euler(0f, sweep, 0f);
                    controller.firePoint.localRotation = _initialRotation * sweepRotation; // Apply relative to initial

                    controller.ShootBullet(controller.Player.transform.position); // Fire towards the current aim
                }
                else
                {
                    controller.SwitchState(new SuppressState());
                    return;
                }

                _sweepAngleProgress += Time.deltaTime * controller.BurstSweepSpeed / controller.BurstSweepAngle;
                _sweepAngleProgress = Mathf.Clamp01(_sweepAngleProgress); // Ensure it stays within 0 and 1
            }

            _burstTimer += Time.deltaTime;
            if (_burstTimer >= controller.burstDuration)
            {
                controller.firePoint.localRotation = _initialRotation; // Reset rotation after burst
                _burstTimer = 0f;
                _sweepAngleProgress = 0f;

                if (!controller.IsPlayerInShootRange())
                {
                    if (controller.IsPlayerInSuppressiveRange())
                    {
                        controller.SwitchState(new SuppressState());
                    }
                    else if (controller.IsPlayerInAlertRange())
                    {
                        controller.SwitchState(new AlertState());
                    }
                    else
                    {
                        controller.SwitchState(new IdleState());
                    }
                }
            }
        }

        public void ExitState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner exited Shoot State");
            controller.firePoint.localRotation = _initialRotation; // Ensure reset on exit
        }
    }
}