using System.Collections;
using UnityEngine;

namespace MortarAI
{
    public class MortarEnemyController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private MortarTargetingSystem targetingSystem;
        [SerializeField] private MortarTeleporter teleporter;
        [SerializeField] private Animator animator;

        // State management
        private IMortarState currentState;
        private MortarIdleState idleState;
        private MortarTargetingState targetingState;
        private MortarFiringState firingState;
        private MortarCooldownState cooldownState;
        private MortarTeleportingState teleportingState;

        // Player tracking
        private Vector3 playerPosition;
        private Vector3 playerVelocity;
        private bool playerPositionKnown = false;

        private void Awake()
        {
            // Initialize components if not set in inspector
            if (targetingSystem == null) targetingSystem = GetComponent<MortarTargetingSystem>();
            if (teleporter == null) teleporter = GetComponent<MortarTeleporter>();
            if (animator == null) animator = GetComponent<Animator>();

            // Initialize states
            idleState = new MortarIdleState(this, animator);
            targetingState = new MortarTargetingState(this, targetingSystem, animator);
            firingState = new MortarFiringState(this, targetingSystem, animator);
            cooldownState = new MortarCooldownState(this, animator);
            teleportingState = new MortarTeleportingState(this, teleporter, animator);

            // Start in idle state
            ChangeState(idleState);
        }

        private void OnEnable()
        {
            // Subscribe to player position updates
            GameEvents.OnPlayerPositionUpdated += OnPlayerPositionUpdated;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            GameEvents.OnPlayerPositionUpdated -= OnPlayerPositionUpdated;
        }

        private void Update()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
        }

        public void ChangeState(IMortarState newState)
        {
            // Exit current state if exists
            if (currentState != null)
            {
                currentState.Exit();
            }

            // Change state
            currentState = newState;

            // Enter new state
            if (currentState != null)
            {
                currentState.Enter();
            }
        }

        private void OnPlayerPositionUpdated(Vector3 position, Vector3 velocity)
        {
            playerPosition = position;
            playerVelocity = velocity;
            playerPositionKnown = true;
        }

        // Getters for states to access data
        public Vector3 GetPlayerPosition() => playerPosition;
        public Vector3 GetPlayerVelocity() => playerVelocity;
        public bool IsPlayerPositionKnown() => playerPositionKnown;
        public float GetDistanceToPlayer() => playerPositionKnown ?
            Vector3.Distance(transform.position, playerPosition) : float.MaxValue;

        // State transitions
        public void EnterIdleState() => ChangeState(idleState);
        public void EnterTargetingState() => ChangeState(targetingState);
        public void EnterFiringState() => ChangeState(firingState);
        public void EnterCooldownState() => ChangeState(cooldownState);
        public void EnterTeleportingState() => ChangeState(teleportingState);
    }
}