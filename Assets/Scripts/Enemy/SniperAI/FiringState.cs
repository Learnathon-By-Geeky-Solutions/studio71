using UnityEngine;


namespace sniperAI
{
    public class FiringState : SniperState
    {
        public FiringState(SniperAI sniper) : base(sniper) { }

        public override void EnterState()
        {
            Debug.Log("Firing shot!");
            
            if (sniper.playerTarget != null)
            {
                // Calculate shot direction
                Vector3 shotDirection = (sniper.playerTarget.position - sniper.transform.position).normalized;

                // Raycast to check hit and verify if it hits the player
                if (Physics.Raycast(sniper.transform.position, shotDirection, out RaycastHit hit, sniper.optimalRange * 2f) && hit.collider.CompareTag("Player"))
                {
                    Debug.Log($"Hit player for {sniper.shotDamage} damage!");
                }


                // Play shooting effects
                Debug.Log("Bang! Shot fired");
            }

            // Transition to repositioning
            sniper.CurrentState = sniper.repositioningState;
            sniper.CurrentState.EnterState();
        }

        public override void UpdateState() { }

        public override void ExitState() { }
    }
}