using UnityEngine;

namespace MortarSystem
{
    public class MortarController : MonoBehaviour
    {
        [Header("State Management")]
        private IMortarState _currentState;
        public IdleState IdleState { get; private set; }
        public AlertState AlertState { get; private set; }
        public FiringState FiringState { get; private set; }

        [Header("Targeting")]
        [HideInInspector] public Transform Player;
        public float alertRadius = 10f;
        public float firingRadius = 5f;

        [Header("Idle Behavior")]
        public float idleScanAngle = 0f;
        public float idleScanRange = 90f;

        [Header("Firing")]
        public GameObject ProjectilePrefab;
        public Transform FirePoint;
        public float trajectoryHeight = 5f; // Adjust for the arc height
        public int projectilePathResolution = 10; // Number of points to simulate path

        private void Awake()
        {
            IdleState = new IdleState();
            AlertState = new AlertState();
            FiringState = new FiringState();

            _currentState = IdleState;
            _currentState.EnterState(this);

            // Automatically find the player by tag "Player"
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
            }
            else
            {
                Debug.LogError("No GameObject found with the tag 'Player'. Please ensure your player object has this tag assigned.");
            }
        }

        private void Update()
        {
            _currentState.UpdateState(this);
        }

        public void SwitchState(IMortarState newState)
        {
            _currentState.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }

        public bool PlayerInAlertZone()
        {
            if (Player == null) return false;
            return Vector3.Distance(transform.position, Player.position) <= alertRadius;
        }

        public bool PlayerInFiringRange()
        {
            if (Player == null) return false;
            return Vector3.Distance(transform.position, Player.position) <= firingRadius;
        }

        #region OnDrawGizmos

        private void OnDrawGizmos()
        {
            // Alert Radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, alertRadius);

            // Firing Radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, firingRadius);

            // Fire Point
            Gizmos.color = Color.green;
            if (FirePoint != null)
            {
                Gizmos.DrawSphere(FirePoint.position, 0.2f);
                Gizmos.DrawLine(transform.position, FirePoint.position);

                // Projectile Path Prediction
                if (Player != null && _currentState == FiringState)
                {
                    Gizmos.color = Color.cyan;
                    Vector3 targetPosition = Player.position;
                    Vector3 startPosition = FirePoint.position;
                    float gravity = Physics.gravity.magnitude;

                    // Calculate initial velocity (same as in FiringState)
                    float timeToTarget = Mathf.Sqrt(2 * (targetPosition.y - startPosition.y + trajectoryHeight) / gravity) +
                                         Mathf.Sqrt(2 * trajectoryHeight / gravity);
                    Vector3 horizontalDisplacement = new Vector3(targetPosition.x - startPosition.x, 0f, targetPosition.z - startPosition.z);
                    Vector3 horizontalVelocity = horizontalDisplacement / timeToTarget;
                    Vector3 verticalVelocity = Vector3.up * Mathf.Sqrt(2 * gravity * trajectoryHeight);
                    Vector3 launchVelocity = horizontalVelocity + verticalVelocity;

                    Vector3 previousPosition = startPosition;
                    for (int i = 1; i <= projectilePathResolution; i++)
                    {
                        float timeStep = timeToTarget * (float)i / projectilePathResolution;
                        Vector3 currentPosition = startPosition + launchVelocity * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;
                        Gizmos.DrawLine(previousPosition, currentPosition);
                        previousPosition = currentPosition;
                    }
                }
            }
        }

        #endregion
    }
}