using UnityEngine;

namespace sniperAI
{
    public class HiddenState : SniperState
    {
        private float scanTimer = 0f;
        private float scanInterval = 3f;

        public HiddenState(SniperAI sniperAI) : base(sniperAI) { }

        public override void EnterState()
        {
            Debug.Log("Sniper: Entering Hidden State");
        }

        public override void UpdateState()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer >= scanInterval)
            {
                ScanForPlayer();
                scanTimer = 0f;
            }

            // If player is detected within optimal range, switch to Aiming
            if (PlayerDetected())
            {
                sniperAI.ChangeState(sniperAI.aimingState);
            }
        }

        public override void ExitState()
        {
            Debug.Log("Sniper: Exiting Hidden State");
        }

        private bool PlayerDetected()
        {
            // Implement player detection logic (Raycast, OverlapSphere, etc.)
            return false; // Placeholder
        }

        private void ScanForPlayer()
        {
            Debug.Log("Sniper: Scanning for player...");
        }
    }
}