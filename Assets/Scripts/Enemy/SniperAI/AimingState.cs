using UnityEngine;



namespace sniperAI
{
    public class AimingState : SniperState
    {
        private float aimTimer;
        private readonly Transform player;

        public AimingState(SniperAI sniper) : base(sniper) 
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        public override void EnterState()
        {
            Debug.Log($"{sniper.gameObject.name}: Aiming at player");
            aimTimer = 0f;
        }

        public override void UpdateState()
        {
            if (!player)
            {
                sniper.CurrentState = sniper.hiddenState;
                return;
            }

            sniper.transform.LookAt(player);
            aimTimer += Time.deltaTime;

            if (aimTimer >= sniper.aimTime)
            {
                sniper.CurrentState = sniper.firingState;
                sniper.CurrentState.EnterState();
            }
        }

        public override void ExitState() { }
    }
}