using UnityEngine;


namespace MachineGunner
{
    public class OverheatAndReloadState : IMachineGunnerState
    {
        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered OverheatAndReload State");
            controller.StartCoolingAndReloading();
        }

        public void UpdateState(MachineGunnerController controller)
        {
            controller.UpdateCoolingAndReloading();
            // The SwitchState logic happens within the controller's UpdateCoolingAndReloading method
        }

        public void ExitState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner exited OverheatAndReload State");
        }
    }
}