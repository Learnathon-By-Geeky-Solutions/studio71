using UnityEngine;

namespace sniperAI
{
    public class RepositioningState : SniperState
    {
        private Vector3[] preDefinedPositions;
        private int currentPosIndex = 0;
        private float moveSpeed = 3f;

        public RepositioningState(SniperAI sniperAI) : base(sniperAI) { }

        public override void EnterState()
        {
            Debug.Log("Sniper: Repositioning...");
            preDefinedPositions = GetPredefinedPositions();
            MoveToNextPosition();
        }

        public override void UpdateState()
        {
            if (Vector3.Distance(sniperAI.transform.position, preDefinedPositions[currentPosIndex]) < 0.1f)
            {
                sniperAI.ChangeState(sniperAI.hiddenState);
            }
        }

        public override void ExitState()
        {
            Debug.Log("Sniper: Repositioned successfully.");
        }

        private Vector3[] GetPredefinedPositions()
        {
            // Define sniper's alternate positions (e.g., waypoints)
            return new Vector3[] { /* positions here */ };
        }

        private void MoveToNextPosition()
        {
            currentPosIndex = (currentPosIndex + 1) % preDefinedPositions.Length;
            // Implement movement logic (NavMeshAgent, Transform, etc.)
        }
    }
}