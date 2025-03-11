using UnityEngine;

public class FollowState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        enemy.Agent.speed = enemy.moveSpeed;
        enemy.Animator.SetBool("IsChasing", true);
        Debug.Log("Enemy is now following the player!");
    }

    public void UpdateState(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

        if (distance > enemy.detectionRange)
        {
            enemy.SetState(new IdleState());
        }
        else if (distance <= enemy.shootingRange && enemy.HasLineOfSight() && enemy.IsPlayerInFOV())
        {
            enemy.SetState(new ShootState());
        }
        else
        {
            enemy.Agent.SetDestination(enemy.Player.position);
        }
    }
}