using UnityEngine;
using SingletonManagers;
using System.Collections.Generic;
using System.Collections;
using Interaction;

using UnityEngine.UIElements;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        //Animator related Variables.
        private Animator _playerAnimator;
        private string _currentAnimation;
        public Dictionary<string, float> _animationLengths { get; private set; } = new Dictionary<string, float>();

        //Bools to check different state.
        public bool IsBusy {get; private set;}
        private bool _isPickingUp=false;
        private bool _isDead = false;
        private bool _isCrouching = false;

        //Weapon Variable
        private Weapon.Gun _equippedGun;


        //Reference to Controller and interaction system to work with them.
        private InteractionSystem _interactionSystem;
        private PlayerController _playerController;

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

            //Weapon Initialization for Animation
            _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();

            //Interaction System Initialization
            _interactionSystem=GetComponent<InteractionSystem>();

            //Player Controller Initialization
            _playerController=GetComponent<PlayerController>();

            _isDead = false;

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
            InputHandler.Instance.OnCrouch -= CrouchAnimation;
            InputHandler.Instance.OnReload -= ReloadAnimation;
            InputHandler.Instance.OnGrenade -= GrenadeAnimation;
            InputHandler.Instance.OnAttack -=ShootAnimation;
            InputHandler.Instance.OnInteract-= PickupAnimation;
        }

        /// <summary>
        /// Update Used to detect look angle and execute movement animation.
        /// </summary>
        private void Update()
        {
            EightWayLocomotion();
            if (_isPickingUp || _isDead) return;            
                MoveAnimation();

        }

        /// <summary>
        /// Different Animation Functions.
        /// </summary>
        private void MoveAnimation()
        {
            if (InputHandler.Instance.MoveDirection == Vector2.zero)
            {
                PlayAnimation(_isCrouching ? "Crouch" : "Idle", 0.1f, 0);
            }
            else
            {
                PlayAnimation(EightWayLocomotion(), 0.15f, 0);
            }
        }
        private void CrouchAnimation()
        {
            _isCrouching = !_isCrouching; // Toggle crouch state
            PlayAnimation(_isCrouching ? "Crouch" : EightWayLocomotion(), 0.1f, 0);
        }
        private void ShootAnimation(bool isPressed)
        {
            if (IsBusy) return;
            
                switch (isPressed)
                {
                    case true:
                        PlayAnimation("Shoot", 0.1f, 1);
                        break;
                    case false:
                        PlayAnimation("Idle UpperBody", 0.1f, 1);
                        break;
                }
            
        }
        private void PickupAnimation()
        {
            if (_interactionSystem._currentTarget == null) return;
            if (IsBusy || !_interactionSystem._currentTarget.CompareTag("Interactable")||_isDead) return;
            IsBusy = true;
            _isPickingUp = true;
            _playerController.enabled = false;
            PlayAnimationAndReturn("Pick Up", "Idle", 0.15f, 0);
            StartCoroutine(DelayedAction(_animationLengths["Pick Up"],
                () => { _isPickingUp = false; _playerController.enabled = true; }));
        }
        private void ReloadAnimation()
        {
            if (IsBusy||_isDead) return;
            IsBusy = true;     
            PlayAnimationAndReturn("Reload", "Idle UpperBody", 0.1f, 1);   
            
        }
        private void GrenadeAnimation()
        {
            if (IsBusy||_isDead) return;
            IsBusy = true;
            PlayAnimationAndReturn("Grenade Throw", "Idle UpperBody", .1f,1);
        }
        private void DeathAnimation()
        {
            _isDead = true;
            _playerController.enabled=false;
            PlayAnimation("Death", 0.1f, 0);
            _playerAnimator.SetLayerWeight(1, 0);
        }

        /// <summary>
        /// Functions that helps execute Animations.
        /// </summary>
        /// <returns></returns>
        private string EightWayLocomotion()
        {
            Vector3 movedirection = new Vector3(InputHandler.Instance.MoveDirection.x, 0, InputHandler.Instance.MoveDirection.y);
            movedirection.Normalize();
             float _angleForAnimation = Vector3.SignedAngle(transform.forward,movedirection,Vector3.up);
            string moveAnimationState = null;

            if (!_playerController._isSprinting && !_isCrouching)
            {
                if (_angleForAnimation is > -22.5f and <= 22.5f) moveAnimationState = "Walk Forward";
                else if (_angleForAnimation is > 22.5f and <= 67.5f) moveAnimationState = "Walk Forward Right";
                else if (_angleForAnimation is > 67.5f and <= 112.5f) moveAnimationState = "Walk Right";
                else if (_angleForAnimation is > 112.5f and <= 157.5f) moveAnimationState = "Walk Backward Right";
                else if (_angleForAnimation is > 157.5f or <= -157.5f) moveAnimationState = "Walk Backward";
                else if (_angleForAnimation is > -157.5f and <= -112.5f) moveAnimationState = "Walk Backward Left";
                else if (_angleForAnimation is > -112.5f and <= -67.5f) moveAnimationState = "Walk Left";
                else if (_angleForAnimation is > -67.5f and <= -22.5f) moveAnimationState = "Walk Forward Left";
            }
            else if (_playerController._isSprinting)
            {
                if (_angleForAnimation is > -22.5f and <= 22.5f) moveAnimationState = "Sprint Forward";
                else if (_angleForAnimation is > 22.5f and <= 67.5f) moveAnimationState = "Sprint Forward Right";
                else if (_angleForAnimation is > 67.5f and <= 112.5f) moveAnimationState = "Sprint Right";
                else if (_angleForAnimation is > 112.5f and <= 157.5f) moveAnimationState = "Sprint Backward Right";
                else if (_angleForAnimation is > 157.5f or <= -157.5f) moveAnimationState = "Sprint Backward";
                else if (_angleForAnimation is > -157.5f and <= -112.5f) moveAnimationState = "Sprint Backward Left";
                else if (_angleForAnimation is > -112.5f and <= -67.5f) moveAnimationState = "Sprint Left";
                else if (_angleForAnimation is > -67.5f and <= -22.5f) moveAnimationState = "Sprint Forward Left";
            }
            else if(_isCrouching)
            {
                if (_angleForAnimation is > -22.5f and <= 22.5f) moveAnimationState = "Crouch Forward";
                else if (_angleForAnimation is > 22.5f and <= 67.5f) moveAnimationState = "Crouch Forward Right";
                else if (_angleForAnimation is > 67.5f and <= 112.5f) moveAnimationState = "Crouch Right";
                else if (_angleForAnimation is > 112.5f and <= 157.5f) moveAnimationState = "Crouch Backward Right";
                else if (_angleForAnimation is > 157.5f or <= -157.5f) moveAnimationState = "Crouch Backward";
                else if (_angleForAnimation is > -157.5f and <= -112.5f) moveAnimationState = "Crouch Backward Left";
                else if (_angleForAnimation is > -112.5f and <= -67.5f) moveAnimationState = "Crouch Left";
                else if (_angleForAnimation is > -67.5f and <= -22.5f) moveAnimationState = "Crouch Forward Left";
            }

            return moveAnimationState;

        }
        private void PlayAnimation(string newAnimation,float SmoothFrame,int WorkingLayer)
        {
            if (_playerAnimator == null || newAnimation == _currentAnimation) return; 

            _playerAnimator.CrossFade(newAnimation, SmoothFrame,WorkingLayer);
            _currentAnimation = newAnimation;
        }
        private void PlayAnimationAndReturn(string animationName,string returnAnimation,float SmoothFrame,int WorkingLayer)
        {
            if (!_animationLengths.ContainsKey(animationName))
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
                _animationLengths[clip.name] = clip.length;
            }
        }
        private IEnumerator ReturnAnimation(string animationName,string returnAnimation,float SmoothFrame,int WorkingLayer)
        {
            yield return new WaitForSeconds(_animationLengths[animationName]);
            _playerAnimator.CrossFade(returnAnimation, SmoothFrame, WorkingLayer);
            IsBusy = false;
        }
        private static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

    }
}
