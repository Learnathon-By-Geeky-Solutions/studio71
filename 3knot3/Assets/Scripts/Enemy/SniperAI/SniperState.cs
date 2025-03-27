using UnityEngine;

namespace sniperAI
{
    public abstract class SniperState
    {
        protected SniperAI sniperAI;

        public SniperState(SniperAI sniperAI)
        {
            this.sniperAI = sniperAI;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }
}