using UnityEngine;

namespace MortarSystem
{
    public class IdleState : IMortarState
    {
        private float _scanTimer = 0f;
        private readonly float _scanInterval = 2f; // Adjust scan interval as needed
        private float _currentScanAngle = 0f;
        private float _scanSpeed = 30f; // Degrees per second

        public void EnterState(MortarController mortar)
        {
            Debug.Log("Mortar entered Idle State.");
            _scanTimer = 0f;
            _currentScanAngle = 0f;
        }

        public void UpdateState(MortarController mortar)
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= _scanInterval)
            {
                _currentScanAngle += _scanSpeed * Time.deltaTime;
                mortar.transform.rotation = Quaternion.Euler(mortar.transform.eulerAngles.x, _currentScanAngle, mortar.transform.eulerAngles.z);

                // Basic back and forth scan
                if (_currentScanAngle > mortar.IdleScanAngle + mortar.IdleScanRange / 2f || _currentScanAngle < mortar.IdleScanAngle - mortar.IdleScanRange / 2f)
                {
                    _scanSpeed *= -1f;
                }

                _scanTimer = 0f;
            }

            // Check for player entering alert zone
            if (mortar.PlayerInAlertZone())
            {
                mortar.SwitchState(mortar.AlertState);
            }
        }

        public void ExitState(MortarController mortar)
        {
            Debug.Log("Mortar exited Idle State.");
        }
    }
}