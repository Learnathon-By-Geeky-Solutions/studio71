using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class AlertState : IMachineGunnerState
    {
        private float _alertStartTime;
        private float _alertDuration = 1f; // Brief alert duration

        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Alert State");
            _alertStartTime = Time.time;
        }

        public void UpdateState(MachineGunnerController controller)
        {
            if (Time.time >= _alertStartTime + _alertDuration)
            {
                if (controller.IsPlayerInShootRange())
                {
                    controller.SwitchState(new ShootState());
                }
                else if (controller.IsPlayerInSuppressiveRange())
                {
                    controller.SwitchState(new SuppressState());
                }
                else if (!controller.IsPlayerInAlertRange())
                {
                    controller.SwitchState(new IdleState());
                }
                // Stay in alert if player is still in alert range but not in other ranges
            }
        }

        public void ExitState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner exited Alert State");
        }
    }
}