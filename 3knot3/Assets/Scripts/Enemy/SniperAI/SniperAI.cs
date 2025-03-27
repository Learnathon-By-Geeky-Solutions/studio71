using UnityEngine;
using sniperAI;

public class SniperAI : MonoBehaviour
{
    public SniperState CurrentState { get; set; }
    public float optimalRange = 30f;
    public float minEngagementRange = 10f;
    public float aimTime = 2f;

    [Header("States")]
    public HiddenState hiddenState;
    public AimingState aimingState;
    public FiringState firingState;
    public RepositioningState repositioningState;
    public FleeState fleeState;

    private void Start()
    {
        // Initialize all states
        hiddenState = new HiddenState(this);
        aimingState = new AimingState(this);
        firingState = new FiringState(this);
        repositioningState = new RepositioningState(this);
        fleeState = new FleeState(this);

        // Start in Hidden State
        CurrentState = hiddenState;
        CurrentState.EnterState();
    }

    private void Update()
    {
        CurrentState.UpdateState();
    }

    public void ChangeState(SniperState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}