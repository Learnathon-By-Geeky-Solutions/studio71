using UnityEngine;

// Interface that all machine gunner states will implement

namespace MachineGunAI
{
    public interface IMachineGunnerState
    {
        void OnEnter();
        void UpdateState();
        void OnExit();
    }

// Interface for damageable objects
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}