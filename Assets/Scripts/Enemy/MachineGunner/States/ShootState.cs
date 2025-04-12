using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class ShootState : IMachineGunnerState
    {
        private float _burstTimer = 0f;

        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Shoot State");
            _burstTimer = 0f;
        }

        public void UpdateState(MachineGunnerController controller)
        {
            _burstTimer += Time.deltaTime;

            if (controller.Player != null)
            {
                if (controller.HasLineOfSightToPlayer())
                {
                    controller.ShootBullet(controller.Player.transform.position); // Precision fire
                }
                else
                {
                    controller.SwitchState(new SuppressState()); // Fallback to suppress if no LOS
                    return;
                }
            }

            if (_burstTimer >= controller.burstDuration)
            {
                _burstTimer = 0f; // Reset burst timer

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
        }
    }
}