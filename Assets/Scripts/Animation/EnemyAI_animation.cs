using UnityEngine;
using patrolEnemy;

public class EnemyAI_Animation : MonoBehaviour
{
    private Animator _enemyAnimator;
    private EnemyAI _enemyAI;
    private string _currentAnimation;

    void Awake()
    {
        _enemyAnimator = GetComponent<Animator>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    void Update()
    {
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (_enemyAI == null || _enemyAnimator == null) return;

        // Check the current state and trigger the appropriate animation
        string stateName = _enemyAI.GetCurrentStateName(); 

        switch (stateName)
        {
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