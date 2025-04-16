using UnityEngine;


namespace MachineGunner
{
    public class AlertState : IMachineGunnerState
    {
        private float _alertStartTime;
        private const float AlertDuration = 1f; // Brief alert duration

        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Alert State");
            _alertStartTime = Time.time;
        }

        public void UpdateState(MachineGunnerController controller)
        {
            if (controller.Player != null)
            {
                // Rotate to face the player
                Vector3 directionToPlayer = controller.Player.transform.position - controller.transform.position;
                directionToPlayer.y = 0f; // Keep rotation on the horizontal plane
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation
            }

            if (Time.time >= _alertStartTime + AlertDuration)
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