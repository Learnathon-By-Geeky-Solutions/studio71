using UnityEngine;
using SingletonManagers;
using System.Collections.Generic;
using System.Collections;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator _playerAnimator;
        private string _currentAnimation;
        private Dictionary<string, float> _animationLengths = new Dictionary<string, float>();


        private bool _isCrouching = false;
        private void Awake()
        {
            _playerAnimator = GetComponent<Animator>();
            if ( _playerAnimator == null) { print($"Animator not found on {gameObject.name}"); }
            CacheAnimationLength();

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
        }

        // Update is called once per frame
        private void OnDisable()
        {
            InputHandler.Instance.OnCrouch -= CrouchAnimation;
            InputHandler.Instance.OnReload -= ReloadAnimation;
            InputHandler.Instance.OnGrenade -= GrenadeAnimation;
        }
        private void MoveAnimation()
        {

        }
        private void CrouchAnimation()
        {
            if (!_isCrouching)
            {
                _isCrouching = true;
                PlayAnimation("Crouch", 0.1f);
            }
            else { PlayAnimation("Idle", 0.15f);_isCrouching = false; }
        }


        private void ReloadAnimation()
        {
            PlayAnimationAndReturn("Reload","Idle",0.1f,1);
        }
        private void GrenadeAnimation()
        {
            PlayAnimationAndReturn("Grenade Throw", "Idle", .1f,1);
        }
        private void PlayAnimation(string newAnimation,float SmoothFrame)
        {
            if (_playerAnimator == null || newAnimation == _currentAnimation) return;

            _playerAnimator.CrossFade(newAnimation, SmoothFrame);
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
        }

    }
}
