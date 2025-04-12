using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class IdleState : IMachineGunnerState
    {
        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Idle State");
        }

        public void UpdateState(MachineGunnerController controller)
        {
            if (controller.IsPlayerInAlertRange())
            {
                controller.SwitchState(new AlertState());
            }
        }

        public void ExitState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner exited Idle State");
        }
    }
}