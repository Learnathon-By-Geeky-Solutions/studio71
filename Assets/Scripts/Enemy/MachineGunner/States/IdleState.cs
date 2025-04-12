using UnityEngine;
using MachineGunner;

namespace MachineGunner.States
{
    public class IdleState : IMachineGunnerState
    {
        private protected float RotationTimer { get; private set; }
        private float _currentRotationAngle;
        private float _rotationDirection = 1f; // 1 for forward, -1 for backward

        public void EnterState(MachineGunnerController controller)
        {
            Debug.Log("Machine Gunner entered Idle State");
            RotationTimer = 0f;
            _currentRotationAngle = 0f;
            _rotationDirection = 1f;
        }

        public void UpdateState(MachineGunnerController controller)
        {
            float deltaAngle = controller.idleRotationSpeed * Time.deltaTime * _rotationDirection;
            controller.transform.Rotate(Vector3.up * deltaAngle);
            _currentRotationAngle += deltaAngle;

            if (_currentRotationAngle > controller.idleRotationAngle / 2f || _currentRotationAngle < -controller.idleRotationAngle / 2f)
            {
                _rotationDirection *= -1f; // Reverse direction
            }

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