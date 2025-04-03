using UnityEngine;
using patrolEnemy;

public class EnemyAI_Animation : MonoBehaviour
{
    private Animator _enemyAnimator;
    private EnemyAI _enemyAI;
    private string _currentAnimation;

    // Animation name constants
    private const string IDLE_ANIM = "Idle";
    private const string ALERT_ANIM = "Alert";
    private const string RUN_ANIM = "Run";
    private const string SHOOT_ANIM = "Shoot";
    private const string THROW_GRENADE_ANIM = "ThrowGrenade";
    private const string CROUCH_IDLE_ANIM = "CrouchIdle";
    private const string RELOAD_ANIM = "Reload"; // New constant for reload animation

    void Awake()
    {
        _enemyAnimator = GetComponent<Animator>();
        _enemyAI = GetComponent<EnemyAI>();

        if (_enemyAnimator == null) Debug.LogError("Animator component missing!");
        if (_enemyAI == null) Debug.LogError("EnemyAI component missing!");
    }

    void Update()
    {
        if (_enemyAI == null || _enemyAnimator == null) return;
        
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        // Check for reload first (as it might be a higher priority animation)
        if (_enemyAI.isReloading)
        {
            PlayAnimation(RELOAD_ANIM);
            return; // Exit early if reloading
        }
        
        else if (_enemyAI.idleState.isWaiting)
        {
            PlayAnimation(ALERT_ANIM);
            return;
        }

        string stateName = _enemyAI.GetCurrentStateName();
        if (string.IsNullOrEmpty(stateName)) return;

        string animationToPlay = stateName switch
        {
            "IdleState" => IDLE_ANIM,
            "AlertState" => ALERT_ANIM,
            "FollowState" => RUN_ANIM,
            "ShootState" => SHOOT_ANIM,
            "GrenadeThrowState" => THROW_GRENADE_ANIM,
            "RecoverState" => CROUCH_IDLE_ANIM,
            _ => _currentAnimation // default to current if no match
        };

        PlayAnimation(animationToPlay);
    }

    private void PlayAnimation(string newAnimation, float smoothTime = 0.1f, int layer = 0)
    {
        if (_enemyAnimator == null || 
            string.IsNullOrEmpty(newAnimation) || 
            newAnimation == _currentAnimation) 
            return;

        _enemyAnimator.CrossFade(newAnimation, smoothTime, layer);
        _currentAnimation = newAnimation;
    }
}