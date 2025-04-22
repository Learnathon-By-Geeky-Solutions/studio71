using UnityEngine;
using SingletonManagers;
using System.Collections.Generic;
using System.Collections;
using Interaction;

using UnityEngine.UIElements;
using HealthSystem;

namespace Player
{
    public static class AnimationState
    {
        public const string Idle = "Idle";
        public const string IdleUpperBody = "Idle UpperBody";
        public const string Shoot = "Shoot";
        public const string Reload = "Reload";
        public const string GrenadeThrow = "Grenade Throw";
        public const string Death = "Death";
        public const string PickUp = "Pick Up";
        public const string Walk = "Walk";
        public const string Crouch = "Crouch";
        public const string Sprint = "Sprint";
        public const string Forward = "Forward";
        public const string Backward = "Backward";
        public const string Left = "Left";
        public const string Right = "Right";
        public const string ForwardLeft = "Forward Left";
        public const string BackwardLeft = "Backward Left";
        public const string ForwardRight = "Forward Right";
        public const string BackwardRight = "Backward Right";
    }
    public class PlayerAnimation : MonoBehaviour
    {
        //Animator related Variables.
        private Animator _playerAnimator;
        private string _currentAnimation;
        public Dictionary<string, float> AnimationLength { get; private set; } = new Dictionary<string, float>();

        //Bools to check different state.
        public bool IsBusy {get; private set;}
        public bool IsThrowingGrenade { get; set; }
        private bool _isPickingUp=false;
        public bool IsDead { get;private set;}
        private bool _isCrouching = false;

        //Reference to Other Player Scripts to work with them.
        private InteractionSystem _interactionSystem;
        private PlayerController _playerController;
        private Health _playerHealth;
        /// <summary>
        /// Initialize Variables for animation.
        /// </summary>
        private void Awake()
        {
            //Animator Initialization
            _playerAnimator = GetComponent<Animator>();
            if ( _playerAnimator == null) { print($"Animator not found on {gameObject.name}"); }

            //Caching Animation Lengths to return from animations
            CacheAnimationLength();

            //Interaction System Initialization
            _interactionSystem=GetComponent<InteractionSystem>();

            //Player Controller Initialization
            _playerController=GetComponent<PlayerController>();

            //Health System Initialization
            _playerHealth = gameObject.GetComponent<Health>();
            IsDead = false;

        }

        private void OnEnable()
        {
            InputHandler.Instance.OnCrouch += CrouchAnimation;
            InputHandler.Instance.OnReload += ReloadAnimation;
            InputHandler.Instance.OnGrenade += GrenadeAnimation;
            InputHandler.Instance.OnAttack += ShootAnimation;
            InputHandler.Instance.OnInteract += PickupAnimation;
        }

        private void OnDisable()
        {
            PlayAnimation(AnimationState.Idle, 0.1f, 0);
            InputHandler.Instance.OnCrouch -= CrouchAnimation;
            InputHandler.Instance.OnReload -= ReloadAnimation;
            InputHandler.Instance.OnGrenade -= GrenadeAnimation;
            InputHandler.Instance.OnAttack -= ShootAnimation;
            InputHandler.Instance.OnInteract -= PickupAnimation;
        }

        /// <summary>
        /// Update Used to detect look angle and execute movement animation.
        /// </summary>
        private void Update()
        {
            EightWayLocomotion();

            if (_isPickingUp || IsDead)
                return;

            MoveAnimation();

            if (_playerHealth.CurrentHealth <= 0)
            {
                DeathAnimation();
            }
        }

        /// <summary>
        /// Different Animation Functions.
        /// </summary>
        private void MoveAnimation()
        {
            if (InputHandler.Instance.MoveDirection == Vector2.zero)
            {
                PlayAnimation(_isCrouching ? AnimationState.Crouch : AnimationState.Idle, 0.1f, 0);
            }
            else
            {
                PlayAnimation(EightWayLocomotion(), 0.15f, 0);
            }
        }
        private void CrouchAnimation()
        {
            _isCrouching = !_isCrouching; // Toggle crouch state
            PlayAnimation(_isCrouching ? AnimationState.Crouch : EightWayLocomotion(), 0.1f, 0);
        }
        private void ShootAnimation(bool isPressed)
        {
            if (!IsBusy)
            {
                switch (isPressed)
                {
                    case true:
                        PlayAnimation(AnimationState.Shoot, 0.1f, 1);
                        break;
                    case false:
                        PlayAnimation(AnimationState.IdleUpperBody, 0.1f, 1);
                        break;
                }
            }
            
        }
        private void PickupAnimation()
        {
            if (_interactionSystem._currentTarget == null) return;
            if (IsBusy || !_interactionSystem._currentTarget.CompareTag("Interactable")||IsDead) return;
            IsBusy = true;
            _isPickingUp = true;
            _playerController.enabled = false;
            PlayAnimationAndReturn(AnimationState.PickUp, AnimationState.Idle, 0.15f, 0);
            StartCoroutine(DelayedAction(AnimationLength[AnimationState.PickUp],
                () => { _isPickingUp = false; _playerController.enabled = true; }));
        }
        private void ReloadAnimation()
        {
            if (IsBusy||IsDead) return;
            IsBusy = true;     
            PlayAnimationAndReturn(AnimationState.Reload, AnimationState.IdleUpperBody, 0.1f, 1);   
            
        }
        private void GrenadeAnimation()
        {
            if (IsBusy || IsDead || _playerController.GrenadeCount<=0) return;
            IsBusy = true;
            PlayAnimationAndReturn(AnimationState.GrenadeThrow, AnimationState.IdleUpperBody, 0.1f, 1);
            StartCoroutine(DelayedAction(AnimationLength[AnimationState.GrenadeThrow], () => { IsThrowingGrenade = false; }));

        }
        private void DeathAnimation()
        {
            IsDead = true;
            _playerController.enabled=false;
            PlayAnimation(AnimationState.Death, 0.1f, 0);
            _playerAnimator.SetLayerWeight(1, 0);
        }

        /// <summary>
        /// Functions that helps execute Animations.
        /// </summary>
        /// <returns></returns>
        private string EightWayLocomotion()
        {
            Vector3 moveDirection = new Vector3(InputHandler.Instance.MoveDirection.x, 0, InputHandler.Instance.MoveDirection.y).normalized;
            float angle = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);

            string statePrefix = GetMovementStatePrefix();
            string directionSuffix = GetDirectionFromAngle(angle);

            if (string.IsNullOrEmpty(statePrefix) || string.IsNullOrEmpty(directionSuffix))
                return null;

            return $"{statePrefix} {directionSuffix}";
        }
        private string GetMovementStatePrefix()
        {
            if (_playerController._isSprinting) return AnimationState.Sprint;
            if (_isCrouching) return AnimationState.Crouch;
            return AnimationState.Walk;
        }
        private static string GetDirectionFromAngle(float angle)
        {
            var directions = new (float min, float max, string direction)[]
            {
                (-22.5f, 22.5f, AnimationState.Forward),
                (22.5f, 67.5f, AnimationState.ForwardRight),
                (67.5f, 112.5f, AnimationState.Right),
                (112.5f, 157.5f, AnimationState.BackwardRight),
                (157.5f, 180f, AnimationState.Backward),
                (-180f, -157.5f, AnimationState.Backward),
                (-157.5f, -112.5f, AnimationState.BackwardLeft),
                (-112.5f, -67.5f, AnimationState.Left),
                (-67.5f, -22.5f, AnimationState.ForwardLeft),
            };

            foreach (var (min, max, dir) in directions)
            {
                if (angle > min && angle <= max)
                    return dir;
            }

            return null;
        }
        private void PlayAnimation(string newAnimation,float SmoothFrame,int WorkingLayer)
        {
            if (_playerAnimator == null || newAnimation == _currentAnimation) return; 

            _playerAnimator.CrossFade(newAnimation, SmoothFrame,WorkingLayer);
            _currentAnimation = newAnimation;
        }
        private void PlayAnimationAndReturn(string animationName,string returnAnimation,float SmoothFrame,int WorkingLayer)
        {
            if (!AnimationLength.ContainsKey(animationName))
            {
                Debug.LogWarning($"Animation '{animationName}' not found!");
                return;
            }
            _playerAnimator.CrossFade(animationName,SmoothFrame);
            StartCoroutine(ReturnAnimation(animationName, returnAnimation,SmoothFrame,WorkingLayer));
        }
        private void CacheAnimationLength()
        {
            foreach (AnimationClip clip in _playerAnimator.runtimeAnimatorController.animationClips)
            {
                AnimationLength[clip.name] = clip.length;
            }
        }
        private IEnumerator ReturnAnimation(string animationName,string returnAnimation,float SmoothFrame,int WorkingLayer)
        {
            yield return new WaitForSeconds(AnimationLength[animationName]);
            _playerAnimator.CrossFade(returnAnimation, SmoothFrame, WorkingLayer);
            IsBusy = false;
            if(animationName== AnimationState.GrenadeThrow)
            {
                IsThrowingGrenade = false;
            }
        }
        private static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

    }
}
