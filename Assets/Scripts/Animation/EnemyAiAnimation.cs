using UnityEngine;
using patrolEnemy;

namespace Enemy_Anim
{
    public class EnemyAiAnimation : MonoBehaviour
    {
        private Animator _enemyAnimator;
        private EnemyAI _enemyAI;
        private string _currentAnimation;

     //EnemyAiAnimation depends on the internal state name of EnemyAI. If the names change, this class breaks. This is called tight coupling. My suggestion is to 
     // Have EnemyAI expose a more structured enum or identifier for state rather than a raw string, or decouple this logic using an event system or observer pattern.
     //like this
     // public enum EnemyState { Idle, Alert, Follow, Shoot, GrenadeThrow, Recover }
     // public EnemyState GetCurrentState() => _currentState;
     
    
        void Awake()
        {
            _enemyAnimator = GetComponent<Animator>();
            _enemyAI = GetComponent<EnemyAI>();
        }
    
        void Update()
        {
        //Update method doing too much things try to resolve this issue by not doing everything in update method, in unity you'll need to try so hard not to write code in update method.it's a good practice.
            HandleAnimation();
        }
    
        private void HandleAnimation()
        {
            //Smelly code: This line can messed up whole optimization in game, redundant null check in every frame which is bad, change it use in awake if needed
            if (_enemyAI == null || _enemyAnimator == null) return;
    
            // Check the current state and trigger the appropriate animation
            string stateName = _enemyAI.GetCurrentStateName(); 
    
            switch (stateName)
            {
            //Smelly Code: Hardcode (whole states is hardcoded which is bad, change it my suggestion is here to use Dictionary)
                case "IdleState":
                    PlayAnimation("Idle");
                    break;
    
                case "AlertState":
                    PlayAnimation("Alert");
                    break;
    
                case "FollowState":
                    PlayAnimation("Run");
                    break;
    
                case "ShootState":
                    PlayAnimation("Shoot");
                    break;
    
                case "GrenadeThrowState":
                    PlayAnimation("ThrowGrenade");
                    break;
    
                case "RecoverState":
                    PlayAnimation("CrouchIdle");
                    break;
            }
        }
    
        private void PlayAnimation(string newAnimation, float smoothTime = 0.1f, int layer = 0)
        {
            if (_enemyAnimator == null || newAnimation == _currentAnimation) return;
    
            _enemyAnimator.CrossFade(newAnimation, smoothTime, layer);
            _currentAnimation = newAnimation;
        }
    }
}
