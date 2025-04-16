using UnityEngine;

namespace MortarSystem
{
    #region Interfaces

    public interface IMortarState
    {
        void EnterState(MortarController mortar);
        void UpdateState(MortarController mortar);
        void ExitState(MortarController mortar);
    }

    #endregion
}