using UnityEngine;
using System.Collections.Generic;
using System;
using PatrolEnemy;

namespace Enemy_Anim
{
    [RequireComponent(typeof(Animator), typeof(EnemyController))]
    public class EnemyAnimationManager : MonoBehaviour
    {
        // Configuration
        [System.Serializable]
        public class AnimationMapping
        {
            public EnemyController.EnemyStateType state;
            public string animationName;
            public float transitionSpeed = 0.1f;
        }

        [SerializeField] private List<AnimationMapping> _animationMappings = new List<AnimationMapping>()
        {
            // Default mappings (editable in Inspector)
            new AnimationMapping{ state = EnemyController.EnemyStateType.Idle, animationName = "Idle" },
            new AnimationMapping{ state = EnemyController.EnemyStateType.Alert, animationName = "Alert" },
            new AnimationMapping{ state = EnemyController.EnemyStateType.Follow, animationName = "Run" },
            new AnimationMapping{ state = EnemyController.EnemyStateType.Shoot, animationName = "Shoot" },
            new AnimationMapping{ state = EnemyController.EnemyStateType.GrenadeThrow, animationName = "ThrowGrenade" },
            new AnimationMapping{ state = EnemyController.EnemyStateType.Recovery, animationName = "CrouchIdle" }
        };

        // Runtime
        private Animator _animator;
        private EnemyController _enemyAI;
        private Dictionary<EnemyController.EnemyStateType, AnimationMapping> _stateToAnimation;
        private string _currentAnimation;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _enemyAI = GetComponent<EnemyController>();

            _stateToAnimation = new Dictionary<EnemyController.EnemyStateType, AnimationMapping>();
            foreach (var mapping in _animationMappings)
            {
                _stateToAnimation[mapping.state] = mapping;
            }

            // Subscribe to state changes
            _enemyAI.OnStateChanged += HandleStateChange;
        }

        private void OnDestroy()
        {
            if (_enemyAI != null)
                _enemyAI.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(EnemyController.EnemyStateType newState)
        {
            if (_stateToAnimation.TryGetValue(newState, out var mapping))
            {
                PlayAnimation(mapping.animationName, mapping.transitionSpeed);
            }
            else
            {
                Debug.LogWarning($"No animation mapping for state: {newState}");
            }
        }

        private void PlayAnimation(string newAnimation, float smoothTime = 0.1f)
        {
            if (_animator == null || string.IsNullOrEmpty(newAnimation) || newAnimation == _currentAnimation) 
                return;

            _animator.CrossFade(newAnimation, smoothTime);
            _currentAnimation = newAnimation;
        }

       
    }
}