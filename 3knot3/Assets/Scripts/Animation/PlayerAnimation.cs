using UnityEngine;
using SingletonManagers;
using System.Collections.Generic;
using System.Collections;
using Interaction;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator _playerAnimator;
        private string _currentAnimation;
        public Dictionary<string, float> _animationLengths { get; private set; } = new Dictionary<string, float>();
        public bool IsBusy {get; private set;}

        //Crouch Related Variables
        private bool _isCrouching = false;

        //Weapon Variable
        private Weapon.Gun _equippedGun;

        private InteractionSystem _interactionSystem;
        private void Awake()
        {
            //Animator Initialization
            _playerAnimator = GetComponent<Animator>();
            if ( _playerAnimator == null) { print($"Animator not found on {gameObject.name}"); }

            //Caching Animation Lengths to return from animations
            CacheAnimationLength();

            //Weapon Initialization for Animation
            _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();

            _interactionSystem=GetComponent<InteractionSystem>();

        }
        private void CacheAnimationLength()
        {
            foreach (AnimationClip clip in _playerAnimator.runtimeAnimatorController.animationClips)
            {
                _animationLengths[clip.name] = clip.length;
            }
        }



        private void OnEnable()
        {
            InputHandler.Instance.OnCrouch += CrouchAnimation;
            InputHandler.Instance.OnReload += ReloadAnimation;
            InputHandler.Instance.OnGrenade += GrenadeAnimation;
            InputHandler.Instance.OnAttack += ShootAnimation;
            InputHandler.Instance.OnInteract += Pickup;
        }

        private void OnDisable()
        {
            InputHandler.Instance.OnCrouch -= CrouchAnimation;
            InputHandler.Instance.OnReload -= ReloadAnimation;
            InputHandler.Instance.OnGrenade -= GrenadeAnimation;
            InputHandler.Instance.OnAttack -=ShootAnimation;
            InputHandler.Instance.OnInteract-= Pickup;
        }


        private void MoveAnimation()
        {

        }
        private void CrouchAnimation()
        {
            if (!_isCrouching)
            {
                _isCrouching = true;
                PlayAnimation("Crouch", 0.1f,0);
            }
            else{ PlayAnimation("Idle", 0.15f,0);_isCrouching = false;print("Stand"); }
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
        private void Pickup()
        {
            if (_interactionSystem._currentTarget == null) return;
            if (IsBusy || !_interactionSystem._currentTarget.CompareTag("Interactable")) return;
            IsBusy = true;
            PlayAnimationAndReturn("Pick Up", "Idle", 0.15f, 0);
        }
        private void ReloadAnimation()
        {
            if (IsBusy) return;
            IsBusy = true;     
            PlayAnimationAndReturn("Reload", "Idle UpperBody", 0.1f, 1);   
            
        }
        private void GrenadeAnimation()
        {
            if (IsBusy) return;
            IsBusy = true;
            PlayAnimationAndReturn("Grenade Throw", "Idle UpperBody", .1f,1);
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


        private IEnumerator ReturnAnimation(string animationName,string returnAnimation,float SmoothFrame,int WorkingLayer)
        {
            yield return new WaitForSeconds(_animationLengths[animationName]);
            _playerAnimator.CrossFade(returnAnimation, SmoothFrame, WorkingLayer);
            IsBusy = false;
        }

    }
}
