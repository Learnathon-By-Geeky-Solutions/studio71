using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class DeathState : IMachineGunnerState
    {
        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Death State");
            // Perform death actions here (e.g., play animation, trigger explosion)
            controller.Die();
        }

        public void UpdateState(MachineGunnerController controller)
        {
            // No updates needed in the death state
        }

        public void ExitState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner exited Death State (this might not be called if object is destroyed)");
        }
    }
}