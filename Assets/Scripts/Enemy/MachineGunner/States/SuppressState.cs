using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class SuppressState : IMachineGunnerState
    {
        private float _burstTimer = 0f;

        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Suppress State");
            _burstTimer = 0f;
        }

        public void UpdateState(MachineGunnerController controller)
        {
            _burstTimer += Time.deltaTime;
            if (controller.Player != null)
            {
                controller.ShootBullet(controller.Player.transform.position, true); // Area denial
            }

            if (_burstTimer >= controller.suppressiveBurstDuration)
            {
                _burstTimer = 0f; // Reset burst timer

                if (controller.IsPlayerInShootRange())
                {
                    controller.SwitchState(new ShootState());
                }
                else if (!controller.IsPlayerInSuppressiveRange())
                {
                    if (controller.IsPlayerInAlertRange())
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
            Debug.Log("Machine Gunner exited Suppress State");
        }
    }
}