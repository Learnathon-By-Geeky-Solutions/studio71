using UnityEngine;
using System.Collections.Generic;
using MachineGunner;

namespace MachineGunnerAnim
{
    public class MachineGunnerAnimationManager : MonoBehaviour
    {
        private Animator _animator;
        private MachineGunnerController _controller;
        [SerializeField] private string _currentAnimation;

        private Dictionary<MachineGunnerStateType, string> _stateToAnimation;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<MachineGunnerController>();

            if (_animator == null || _controller == null)
            {
                Debug.LogError("MachineGunnerAnimationManager: Missing Animator or Controller component.");
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
            _stateToAnimation = new Dictionary<MachineGunnerStateType, string>
            {
                { MachineGunnerStateType.Idle, "Idle" },
                { MachineGunnerStateType.Alert, "Idle" },
                { MachineGunnerStateType.Shoot, "Shoot" },
                { MachineGunnerStateType.Suppress, "Shoot" },
                { MachineGunnerStateType.OverheatAndReload, "Reload" },
                { MachineGunnerStateType.Death, "Death" },
            };
        }

        private void HandleAnimation()
        {
            if (_controller.CurrentState == null) return;

            MachineGunnerStateType currentState = GetCurrentStateType();

            if (currentState == MachineGunnerStateType.OverheatAndReload)
            {
                if (!_controller.IsOverheated)
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

        private MachineGunnerStateType GetCurrentStateType()
        {
            if (_controller.CurrentState is IdleState) return MachineGunnerStateType.Idle;
            if (_controller.CurrentState is AlertState) return MachineGunnerStateType.Alert;
            if (_controller.CurrentState is ShootState) return MachineGunnerStateType.Shoot;
            if (_controller.CurrentState is SuppressState) return MachineGunnerStateType.Suppress;
            if (_controller.CurrentState is OverheatAndReloadState) return MachineGunnerStateType.OverheatAndReload;
            if (_controller.CurrentState is DeathState) return MachineGunnerStateType.Death;


            return MachineGunnerStateType.Idle;
        }

        private void PlayAnimation(string newAnimation, float smoothTime = 0.1f, int layer = 0)
        {
            if (newAnimation == _currentAnimation) return;

            _animator.CrossFade(newAnimation, smoothTime, layer);
            _currentAnimation = newAnimation;
        }
    }
}
