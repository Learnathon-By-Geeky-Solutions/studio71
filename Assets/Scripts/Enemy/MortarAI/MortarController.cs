using DG.Tweening;
using HealthSystem;
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

        private Animator _animator;


        [Header("Targeting")]
        public Transform Player { get; private set; }
        [SerializeField] private float AlertRadius = 575f;
        [SerializeField] private float FiringRadius = 50f;
        private Health _health;

        [Header("Idle Behavior")]
        [SerializeField] private float idleScanAngle = 0f;
        [SerializeField] private float idleScanRange = 90f;

        [Header("Firing")]
        [SerializeField] public GameObject ProjectilePrefab;
        public Transform FirePoint;
        [SerializeField] private float baseTrajectoryHeight = 5f;
        [SerializeField] private float maxTrajectoryHeight = 20f;
        [SerializeField] private float trajectoryHeightMultiplier = 0.15f;
        [SerializeField] private int projectilePathResolution = 100;

        [Header("Visualization")]
        private LineRenderer projectilePathRenderer;
        [SerializeField] private bool showProjectilePath = true;
        [SerializeField] private float pathDisplayDuration = 3f;
        private float _pathDisplayTimer = 0f;
        public bool _isDeath { get; private set; }
        public float IdleScanAngle => idleScanAngle;
        public float IdleScanRange => idleScanRange;

        public float TrajectoryHeight
        {
            get
            {
                if (Player == null) return baseTrajectoryHeight;
                float distance = Vector3.Distance(FirePoint.position, Player.position);
                float dynamicHeight = baseTrajectoryHeight + (distance * trajectoryHeightMultiplier);
                return Mathf.Min(dynamicHeight, maxTrajectoryHeight);
            }
        }

        private void Awake()
        {
            IdleState = new IdleState();
            AlertState = new AlertState();
            FiringState = new FiringState();

            _health = GetComponent<Health>();
            _animator = GetComponent<Animator>();

            _currentState = IdleState;
            _currentState.EnterState(this);

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                Player = playerObject.transform;
            else
                Debug.LogError("No GameObject found with the tag 'Player'. Please ensure your player object has this tag assigned.");

            if (projectilePathRenderer == null)
            {
                projectilePathRenderer = gameObject.AddComponent<LineRenderer>();
                projectilePathRenderer.startWidth = 0.1f;
                projectilePathRenderer.endWidth = 0.1f;
                projectilePathRenderer.material = new Material(Shader.Find("Sprites/Default"));
                projectilePathRenderer.startColor = Color.red;
                projectilePathRenderer.endColor = Color.yellow;
                projectilePathRenderer.positionCount = 0;
            }
        }

        private void Update()
        {
            _currentState.UpdateState(this);

            if (_pathDisplayTimer > 0)
            {
                _pathDisplayTimer -= Time.deltaTime;
                if (_pathDisplayTimer <= 0)
                {
                    projectilePathRenderer.positionCount = 0;
                }
            }

            if (_health.CurrentHealth <= 0)
            {
                _isDeath = true;
                _animator.Play("Death");
            }
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
            return Vector3.Distance(transform.position, Player.position) <= AlertRadius;
        }

        public bool PlayerInFiringRange()
        {
            if (Player == null) return false;
            return Vector3.Distance(transform.position, Player.position) <= FiringRadius;
        }

        public void VisualizeProjectilePath()
        {
            if (!showProjectilePath || projectilePathRenderer == null || Player == null) return;

            Vector3 startPosition = FirePoint.position;
            Vector3 targetPosition = Player.position;
            float gravity = Physics.gravity.magnitude;
            float trajectoryHeight = TrajectoryHeight;

            Vector3 horizontalDisplacement = new Vector3(targetPosition.x - startPosition.x, 0f, targetPosition.z - startPosition.z);
            float verticalDisplacement = targetPosition.y - startPosition.y;

            float sqrtTerm1 = 2 * (verticalDisplacement + trajectoryHeight) / gravity;
            float sqrtTerm2 = 2 * trajectoryHeight / gravity;

            if (sqrtTerm1 < 0 || sqrtTerm2 < 0) return;

            float timeToTarget = Mathf.Sqrt(sqrtTerm1) + Mathf.Sqrt(sqrtTerm2);
            Vector3 horizontalVelocity = horizontalDisplacement / timeToTarget;
            Vector3 verticalVelocity = Vector3.up * Mathf.Sqrt(2 * gravity * trajectoryHeight);
            Vector3 launchVelocity = horizontalVelocity + verticalVelocity;

            projectilePathRenderer.positionCount = projectilePathResolution + 1;

            for (int i = 0; i <= projectilePathResolution; i++)
            {
                float timeStep = timeToTarget * i / projectilePathResolution;
                Vector3 pos = startPosition + launchVelocity * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;
                projectilePathRenderer.SetPosition(i, pos);
            }

            _pathDisplayTimer = pathDisplayDuration;
        }

        #region OnDrawGizmos

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, AlertRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, FiringRadius);

            Gizmos.color = Color.green;
            if (FirePoint != null)
            {
                Gizmos.DrawSphere(FirePoint.position, 0.2f);
                Gizmos.DrawLine(transform.position, FirePoint.position);

                if (Player != null && _currentState == FiringState)
                {
                    Gizmos.color = Color.cyan;
                    Vector3 targetPosition = Player.position;
                    Vector3 startPosition = FirePoint.position;
                    float gravity = Physics.gravity.magnitude;
                    float trajectoryHeight = TrajectoryHeight;

                    float sqrtTerm1 = 2 * (targetPosition.y - startPosition.y + trajectoryHeight) / gravity;
                    float sqrtTerm2 = 2 * trajectoryHeight / gravity;

                    if (sqrtTerm1 >= 0 && sqrtTerm2 >= 0)
                    {
                        float timeToTarget = Mathf.Sqrt(sqrtTerm1) + Mathf.Sqrt(sqrtTerm2);
                        Vector3 horizontalDisplacement = new Vector3(targetPosition.x - startPosition.x, 0f, targetPosition.z - startPosition.z);
                        Vector3 horizontalVelocity = horizontalDisplacement / timeToTarget;
                        Vector3 verticalVelocity = Vector3.up * Mathf.Sqrt(2 * gravity * trajectoryHeight);
                        Vector3 launchVelocity = horizontalVelocity + verticalVelocity;

                        Vector3 previousPosition = startPosition;
                        for (int i = 1; i <= projectilePathResolution; i++)
                        {
                            float timeStep = timeToTarget * i / projectilePathResolution;
                            Vector3 currentPosition = startPosition + launchVelocity * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;
                            Gizmos.DrawLine(previousPosition, currentPosition);
                            previousPosition = currentPosition;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
