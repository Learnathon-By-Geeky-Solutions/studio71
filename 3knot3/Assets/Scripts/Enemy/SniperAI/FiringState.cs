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

                // Raycast to check hit
                if (Physics.Raycast(
                        sniper.transform.position,
                        shotDirection,
                        out RaycastHit hit,
                        sniper.optimalRange * 2f))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log($"Hit player for {sniper.shotDamage} damage!");
                        //hit.collider.GetComponent<PlayerHealth>()?.TakeDamage(sniper.shotDamage);
                    }
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