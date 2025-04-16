using UnityEngine;

namespace MachineGunner
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
            if (controller.Player != null)
            {
                // Rotate to face the player
                Vector3 directionToPlayer = controller.Player.transform.position - controller.transform.position;
                directionToPlayer.y = 0f; // Keep rotation on the horizontal plane
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation

                controller.ShootBullet(controller.Player.transform.position, true); // Area denial
            }

            _burstTimer += Time.deltaTime;
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