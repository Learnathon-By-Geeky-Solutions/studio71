using UnityEngine;
using System.Linq; // Required for .Any()

namespace sniperAI
{
    public class HiddenState : SniperState
    {
        private float scanTimer;
        private const float SCAN_INTERVAL = 1f;

        public HiddenState(SniperAI sniper) : base(sniper) { }

        public override void EnterState()
        {
            Debug.Log($"{sniper.gameObject.name}: Entering Hidden State");
            scanTimer = 0f;
        }

        public override void UpdateState()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer >= SCAN_INTERVAL)
            {
                scanTimer = 0f;
                Collider[] hits = Physics.OverlapSphere(
                    sniper.transform.position,
                    sniper.detectionRadius
                );

                if (hits.Any(c => c.CompareTag("Player")))
                {
                    // Verify line of sight
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null && HasClearLineOfSight(player.transform))
                    {
                        sniper.CurrentState = sniper.aimingState;
                        sniper.CurrentState.EnterState();
                    }
                }
            }
        }

        private bool HasClearLineOfSight(Transform target)
        {
            Vector3 direction = target.position - sniper.transform.position;
            return !Physics.Raycast(
                sniper.transform.position,
                direction.normalized,
                direction.magnitude,
                LayerMask.GetMask("Wall")
            );
        }

        public override void ExitState() { }
    }
}