namespace PatrolEnemy
{
    public interface IEnemyState
    {
        void EnterState(EnemyController controller);
        void UpdateState(EnemyController controller);
        void ExitState(EnemyController controller);
        
    }
}