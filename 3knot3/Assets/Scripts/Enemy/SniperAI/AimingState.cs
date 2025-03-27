using UnityEngine;

namespace sniperAI
{
    public class AimingState : SniperState
    {
        private float aimTimer = 0f;
        private bool hasAimed = false;

        public AimingState(SniperAI sniperAI) : base(sniperAI) { }

        public override void EnterState()
        {
            Debug.Log("Sniper: Entering Aiming State");
            aimTimer = 0f;
            hasAimed = false;
            ShowScopeGlint(); // Visual feedback
        }

        public override void UpdateState()
        {
            if (!hasAimed)
            {
                aimTimer += Time.deltaTime;
                if (aimTimer >= sniperAI.aimTime)
                {
                    hasAimed = true;
                    sniperAI.ChangeState(sniperAI.firingState);
                }
            }
        }

        public override void ExitState()
        {
            Debug.Log("Sniper: Exiting Aiming State");
            HideScopeGlint();
        }

        private void ShowScopeGlint()
        {
            Debug.Log("Sniper: Scope glint visible!");
        }

        private void HideScopeGlint()
        {
            Debug.Log("Sniper: Scope glint hidden.");
        }
    }
}