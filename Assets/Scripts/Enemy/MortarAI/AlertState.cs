using UnityEngine;

namespace MortarSystem
{
    public class AlertState : IMortarState
    {
        private float _loadingTimer = 0f;
        private float _loadingDuration = 2f; // Adjust loading duration as needed

        public void EnterState(MortarController mortar)
        {
            Debug.Log("Mortar entered Alert State. Loading...");
            _loadingTimer = 0f;
        }

        public void UpdateState(MortarController mortar)
        {
            _loadingTimer += Time.deltaTime;

            // Keep aiming at the player while in alert
            if (mortar.Player)
            {
                Vector3 targetDirection = (mortar.Player.position - mortar.transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(targetDirection.x, 0f, targetDirection.z)); // Ignore vertical look
                mortar.transform.rotation = Quaternion.Slerp(mortar.transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation
            }

            if (_loadingTimer >= _loadingDuration)
            {
                if (mortar.PlayerInFiringRange())
                {
                    mortar.SwitchState(mortar.FiringState);
                }
                else if (!mortar.PlayerInAlertZone())
                {
                    mortar.SwitchState(mortar.IdleState);
                }
                // If player is still in alert zone but not firing range, stay in alert.
            }
            else if (!mortar.PlayerInAlertZone())
            {
                mortar.SwitchState(mortar.IdleState);
            }
        }

        public void ExitState(MortarController mortar)
        {
            Debug.Log("Mortar exited Alert State.");
        }
    }
}