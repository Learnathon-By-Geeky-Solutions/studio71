namespace MachineGunner
{
    public interface IMachineGunnerState
    {
        void EnterState(MachineGunnerController controller);
        void UpdateState(MachineGunnerController controller);
        void ExitState(MachineGunnerController controller);
    }

    public enum MachineGunnerStateType
    {
        Idle,
        Alert,
        Suppress,
        Shoot,
        OverheatAndReload,
    }
}