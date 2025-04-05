namespace sniperAI
{
    public abstract class SniperState
    {
        protected readonly SniperAI sniper;

        public SniperState(SniperAI sniperAI)
        {
            this.sniper = sniperAI;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }
}