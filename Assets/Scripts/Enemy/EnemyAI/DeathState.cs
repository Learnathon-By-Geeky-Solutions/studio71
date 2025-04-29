using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

namespace PatrolEnemy
{
    public class DeathState : IEnemyState
    {
     

        public void EnterState(EnemyController controller)
        {
            Debug.Log("DEATH");
        }

        public void UpdateState(EnemyController controller)
        {       
            Debug.Log("DEATH");     
        }

        public void ExitState(EnemyController controller)
        {
             Debug.Log("DEATH");
        }
    }
}