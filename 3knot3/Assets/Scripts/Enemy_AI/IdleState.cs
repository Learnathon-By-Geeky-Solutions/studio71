using UnityEngine;

public class IdleState : IEnemyState
{
    private bool alertTriggered = false;

    public void EnterState(EnemyAI enemy)
    {
        enemy.Agent.speed = enemy.moveSpeed * 0.5f;
        Debug.Log("Enemy is patrolling...");
    }

    public void UpdateState(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

        if (distance <= enemy.detectionRange && !alertTriggered)
        {
            alertTriggered = true;
            enemy.SetState(new AlertState());
        }
        else if (!enemy.Agent.hasPath || enemy.Agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = enemy.transform.position + Random.insideUnitSphere * 5f;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                enemy.Agent.SetDestination(hit.position);
            }
        }
    }
}