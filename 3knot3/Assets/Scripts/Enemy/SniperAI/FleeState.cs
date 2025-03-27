using UnityEngine;

namespace sniperAI
{
    public class FleeState : SniperState
    {
        private float fleeDistance = 15f;
        private float fleeSpeed = 5f;

        public FleeState(SniperAI sniperAI) : base(sniperAI) { }

        public override void EnterState()
        {
            Debug.Log("Sniper: FLEEING!");
            DeploySmokeGrenade();
            StartFleeing();
        }

        public override void UpdateState()
        {
            if (IsAtSafeDistance())
            {
                sniperAI.ChangeState(sniperAI.repositioningState);
            }
        }

        public override void ExitState()
        {
            Debug.Log("Sniper: Safe distance reached.");
        }

        private void DeploySmokeGrenade()
        {
            Debug.Log("Sniper: Smoke grenade deployed!");
        }

        private void StartFleeing()
        {
            // Implement fleeing movement (e.g., away from player)
        }

        private bool IsAtSafeDistance()
        {
            // Check if sniper is far enough from player
            return true; // Placeholder
        }
    }
}