using SingletonManagers;
using UnityEngine;

namespace MortarSystem
{
    public class FiringState : IMortarState
    {
        private float _reloadTimer = 0f;
        private readonly float _reloadDuration = 10f; // Adjust reload duration as needed

        public void EnterState(MortarController mortar)
        {
            Debug.Log("Mortar entered Firing State. Firing!");
            FireProjectile(mortar);
            _reloadTimer = 0f;
        }

        public void UpdateState(MortarController mortar)
        {
            _reloadTimer += Time.deltaTime;

            if (_reloadTimer >= _reloadDuration)
            {
                if (mortar.PlayerInFiringRange())
                {
                    FireProjectile(mortar);
                    _reloadTimer = 0f;
                }
                else if (mortar.PlayerInAlertZone())
                {
                    mortar.SwitchState(mortar.AlertState);
                }
                else
                {
                    mortar.SwitchState(mortar.IdleState);
                }
            }
        }

        public void ExitState(MortarController mortar)
        {
            Debug.Log("Mortar exited Firing State.");
        }

        private static void FireProjectile(MortarController mortar)
        {
            if (mortar._ProjectilePrefab == null || mortar._FirePoint == null || mortar.Player == null||mortar._isDeath)
            {
                Debug.LogError("Projectile Prefab, Fire Point, or Player not assigned!");
                mortar.SwitchState(mortar.IdleState);
                return;
            }

            Vector3 targetPosition = mortar.Player.position;
            Vector3 startPosition = mortar._FirePoint.position;
            float gravity = Physics.gravity.magnitude;
            float trajectoryHeight = mortar.TrajectoryHeight; // Now using dynamic height

            Debug.Log($"Target Y: {targetPosition.y}, Start Y: {startPosition.y}, Trajectory Height: {trajectoryHeight}, Gravity: {gravity}");

            float sqrtTerm1 = 2 * (targetPosition.y - startPosition.y + trajectoryHeight) / gravity;
            float sqrtTerm2 = 2 * trajectoryHeight / gravity;

            Debug.Log($"sqrtTerm1: {sqrtTerm1}, sqrtTerm2: {sqrtTerm2}");

            float timeToTarget = 0f;
            if (sqrtTerm1 >= 0 && sqrtTerm2 >= 0)
            {
                timeToTarget = Mathf.Sqrt(sqrtTerm1) + Mathf.Sqrt(sqrtTerm2);
                Debug.Log($"Time to Target: {timeToTarget}");
            }
            else
            {
                Debug.LogError("Negative value inside Sqrt for Time to Target!");
                return; // Prevent further calculations with NaN
            }

            Vector3 horizontalDisplacement = new Vector3(targetPosition.x - startPosition.x, 0f, targetPosition.z - startPosition.z);
            Vector3 horizontalVelocity;
            if (timeToTarget > Mathf.Epsilon) // Avoid division by zero
            {
                horizontalVelocity = horizontalDisplacement / timeToTarget;
                Debug.Log($"Horizontal Velocity: {horizontalVelocity}");
            }
            else
            {
                Debug.LogError("Time to Target is zero or very small!");
                return;
            }

            float verticalVelocityValue = 0f;
            if (2 * gravity * trajectoryHeight >= 0)
            {
                verticalVelocityValue = Mathf.Sqrt(2 * gravity * trajectoryHeight);
                Vector3 verticalVelocity = Vector3.up * verticalVelocityValue;
                Debug.Log($"Vertical Velocity Value: {verticalVelocityValue}, Vertical Velocity Vector: {verticalVelocity}");
                Vector3 launchVelocity = horizontalVelocity + verticalVelocity;
                Debug.Log($"Launch Velocity: {launchVelocity}");

                // Visualize the projectile path using LineRenderer
                mortar.VisualizeProjectilePath();

                GameObject projectile = GameObject.Instantiate(mortar._ProjectilePrefab, startPosition, Quaternion.identity);
                Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

                if (projectileRb != null)
                {
                    // Fixed property name: changed from linearVelocity to velocity
                    projectileRb.linearVelocity = launchVelocity;
                }
                else
                {
                    Debug.LogError("Projectile prefab does not have a Rigidbody component!");
                    GameObject.Destroy(projectile);
                }
                projectile.transform.forward = launchVelocity.normalized;
            }
            else
            {
                Debug.LogError("Negative value inside Sqrt for Vertical Velocity!");
            }
        }
    }
}