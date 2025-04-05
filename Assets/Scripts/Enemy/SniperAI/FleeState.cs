using UnityEngine;

namespace sniperAI
{
    public class FleeState : SniperState
    {
        private Vector3 fleeDirection;
        private float fleeSpeed = 5f;
        private float safeDistance;

        public FleeState(SniperAI sniper) : base(sniper) 
        {
            safeDistance = sniper.fleeDistance;
        }

        public override void EnterState()
        {
            Debug.Log("Entering Flee State");
            
            // Calculate flee direction away from player
            if (sniper.playerTarget != null)
            {
                fleeDirection = (sniper.transform.position - sniper.playerTarget.position).normalized;
            }
            else
            {
                fleeDirection = Random.insideUnitSphere.normalized;
            }

            // Deploy smoke grenade effect
            Debug.Log("Deploying smoke grenade!");
        }

        public override void UpdateState()
        {
            // Move away
            sniper.transform.position += fleeDirection * fleeSpeed * Time.deltaTime;

            // Check if reached safe distance
            if (sniper.playerTarget == null || 
                Vector3.Distance(sniper.transform.position, sniper.playerTarget.position) >= safeDistance)
            {
                sniper.CurrentState = sniper.repositioningState;
                sniper.CurrentState.EnterState();
            }
        }

        public override void ExitState()
        {
            Debug.Log("Exiting Flee State");
        }
    }
}