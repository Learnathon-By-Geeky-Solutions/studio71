// --- Start of reload-state.cs ---
using UnityEngine;

namespace MachineGunAI
{
    // Reload State Implementation
    public class ReloadState : IMachineGunnerState
    {
        private MachineGunnerAI gunner;
        private float reloadTimer = 0f;
        public ReloadState(MachineGunnerAI gunner)
        {
            this.gunner = gunner;
        }

        public void OnEnter()
        {
            Debug.Log("Machine Gunner entered Reload State");
            reloadTimer = 0f;
        }

        public void UpdateState()
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= gunner.ReloadTime)
            {
                gunner.Reload();
                gunner.TransitionToState(gunner.idleState);
            }
        }

        public void OnExit()
        {
            // Nothing specific to clean up in this state
        }
    }
}
// --- End of reload-state.cs ---