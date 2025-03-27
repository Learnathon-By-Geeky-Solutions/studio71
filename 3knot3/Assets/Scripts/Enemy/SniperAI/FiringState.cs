using UnityEngine;

namespace sniperAI
{
    public class FiringState : SniperState
    {
        public FiringState(SniperAI sniperAI) : base(sniperAI) { }

        public override void EnterState()
        {
            Debug.Log("Sniper: FIRING!");
            FireShot();
            sniperAI.ChangeState(sniperAI.repositioningState); // Move after firing
        }

        public override void UpdateState() { }
        public override void ExitState() { }

        private void FireShot()
        {
            Debug.Log("Sniper: High-damage shot fired!");
            // Implement shooting logic here
        }
    }
}