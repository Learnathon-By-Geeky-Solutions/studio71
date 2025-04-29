using UnityEngine;
using System.Collections.Generic;
using PatrolEnemy;

namespace Enemy_Anim
{
    public class EnemyAiAnimation : MonoBehaviour
    {
        private Animator _enemyAnimator;
        private EnemyController _enemyController;
        [SerializeField] private string _currentAnimation;

        private Dictionary<EnemyController.EnemyStateType, string> _stateToAnimation;

        void Awake()
        {
            _enemyAnimator = GetComponent<Animator>();
            _enemyController = GetComponent<EnemyController>();

            if (_enemyAnimator == null || _enemyController == null)
            {
                Debug.LogError("EnemyAiAnimation: Missing Animator or EnemyController component.");
                enabled = false;
                return;
            }

            InitializeAnimationMappings();
        }

        void Update()
        {
            HandleAnimation();
        }

        private void InitializeAnimationMappings()
        {
            _stateToAnimation = new Dictionary<EnemyController.EnemyStateType, string>
            {
                { EnemyController.EnemyStateType.Idle, "Idle" },
                { EnemyController.EnemyStateType.Alert, "Alert" },
                { EnemyController.EnemyStateType.Follow, "Run" },
                { EnemyController.EnemyStateType.Shoot, "Shoot" },
                { EnemyController.EnemyStateType.GrenadeThrow, "ThrowGrenade" },
                { EnemyController.EnemyStateType.Recovery, "CrouchIdle" },
                { EnemyController.EnemyStateType.Death, "Death" }
            };
        }

        private void HandleAnimation()
        {
            if (_enemyController.currentState == null) return;

            EnemyController.EnemyStateType currentState = GetCurrentStateType();

            if (currentState == EnemyController.EnemyStateType.Shoot)
            {
                if (_enemyController.IsReloading)
                {
                    PlayAnimation("Reload");
                    return;
                }

                else if (!_enemyController.HasLineOfSight)
                {
                    PlayAnimation("Idle");
                    return;
                }
            }

            else if (currentState == EnemyController.EnemyStateType.Recovery)
            {
                if (_enemyController.IsRecoverReturing)
                {
                    PlayAnimation("Idle");
                    return;
                }
             
            }

            else if (currentState == EnemyController.EnemyStateType.Idle)
            {
                if (_enemyController.IsIdleAlert)
                {
                    PlayAnimation("Alert");
                    return;
                }
                else
                {
                    PlayAnimation("Idle");
                    return;
                }
            }

            if (_stateToAnimation.TryGetValue(currentState, out string animationName))
            {
                PlayAnimation(animationName);
            }
        }

        private EnemyController.EnemyStateType GetCurrentStateType()
        {
            if (_enemyController._isIdle) return EnemyController.EnemyStateType.Idle;
            if (_enemyController._isAlert) return EnemyController.EnemyStateType.Alert;
            if (_enemyController._isFollowing) return EnemyController.EnemyStateType.Follow;
            if (_enemyController._isShooting) return EnemyController.EnemyStateType.Shoot;
            if (_enemyController._isRecovering) return EnemyController.EnemyStateType.Recovery;
            if (_enemyController._isThrowingGrenade) return EnemyController.EnemyStateType.GrenadeThrow;
            if(_enemyController.IsDead) return EnemyController.EnemyStateType.Death;

            return EnemyController.EnemyStateType.Idle;
        }

        private void PlayAnimation(string newAnimation, float smoothTime = 0.1f, int layer = 0)
        {
            if (newAnimation == _currentAnimation) return;

            _enemyAnimator.CrossFade(newAnimation, smoothTime, layer);
            _currentAnimation = newAnimation;
        }
    }
}
