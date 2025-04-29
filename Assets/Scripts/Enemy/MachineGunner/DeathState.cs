using UnityEngine;


namespace MachineGunner
{
    public class DeathState : IMachineGunnerState
    {
        public void EnterState(MachineGunnerController controller)
        {

            Debug.Log("DeathMG");
        }

        public void UpdateState(MachineGunnerController controller)
        {
            Debug.Log("DeathMG");
        }

        public void ExitState(MachineGunnerController controller)
        {
            Debug.Log("DeathMG");
        }
    }
}