using UnityEngine;
using SingletonManagers;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator _playerAnimator;
        private string _currentAnimation;

        private bool _isCrouching = false;
        private void Awake()
        {
            _playerAnimator = GetComponent<Animator>();
            if ( _playerAnimator == null) { print($"Animator not found on {gameObject.name}"); }
        }

        private void OnEnable()
        {
            InputHandler.Instance.OnCrouch += CrouchAnimation;
        }

        // Update is called once per frame
        private void OnDisable()
        {
            InputHandler.Instance.OnCrouch -= CrouchAnimation;
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






        private void PlayAnimation(string newAnimation,float SmoothFrame)
        {
            if (_playerAnimator == null || newAnimation == _currentAnimation) return;

            _playerAnimator.CrossFade(newAnimation, SmoothFrame);
            _currentAnimation = newAnimation;
        }

    }
}
