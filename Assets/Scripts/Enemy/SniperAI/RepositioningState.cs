using UnityEngine;

namespace sniperAI
{
    public class RepositioningState : SniperState
    {
        private int currentPosIndex = 0;
        private float moveSpeed = 3f;

        public RepositioningState(SniperAI sniper) : base(sniper) { }

        public override void EnterState()
        {
            Debug.Log("Entering Repositioning State");
            currentPosIndex = (currentPosIndex + 1) % sniper.repositionPoints.Length;
        }

        public override void UpdateState()
        {
            Vector3 targetPos = sniper.repositionPoints[currentPosIndex];
            sniper.transform.position = Vector3.MoveTowards(
                sniper.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(sniper.transform.position, targetPos) < 0.1f)
            {
                sniper.CurrentState = sniper.hiddenState; // Access through SniperAI
                sniper.CurrentState.EnterState();
            }
        }

        public override void ExitState() { }
    }
}