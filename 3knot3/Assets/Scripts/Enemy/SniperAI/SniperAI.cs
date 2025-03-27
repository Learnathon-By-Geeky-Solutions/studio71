using UnityEngine;
using sniperAI;

public class SniperAI : MonoBehaviour
{
    // Inspector Settings
    public float optimalRange = 30f;
    public float minEngagementRange = 10f;
    public float aimTime = 2f;
    public float detectionRadius = 20f;
    public float fleeDistance = 15f;
    public Vector3[] repositionPoints = new Vector3[2];
    public float shotDamage = 50f;

    // State References
    [SerializeField] private SniperState _currentState;
    public HiddenState hiddenState { get; private set; }
    public AimingState aimingState { get; private set; }
    public FiringState firingState { get; private set; }
    public RepositioningState repositioningState { get; private set; }
    public FleeState fleeState { get; private set; }

    public Transform playerTarget { get; private set; }

    private void Start()
    {
        // Find player
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Initialize states
        hiddenState = new HiddenState(this);
        aimingState = new AimingState(this);
        firingState = new FiringState(this);
        repositioningState = new RepositioningState(this);
        fleeState = new FleeState(this);

        // Set default positions if empty
        if (repositionPoints[0] == Vector3.zero)
        {
            repositionPoints[0] = transform.position + new Vector3(5, 0, 0);
            repositionPoints[1] = transform.position + new Vector3(-5, 0, 5);
        }

        CurrentState = hiddenState;
        CurrentState.EnterState();
    }

    public SniperState CurrentState {
        get => _currentState;
        set => _currentState = value;
    }

    private void Update() => CurrentState?.UpdateState();

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, detectionRadius);

        // Reposition points
        Gizmos.color = Color.cyan;
        foreach (Vector3 point in repositionPoints)
        {
            if (point != Vector3.zero)
            {
                Gizmos.DrawSphere(point, 0.3f);
                Gizmos.DrawLine(transform.position, point);
            }
        }
    }
}