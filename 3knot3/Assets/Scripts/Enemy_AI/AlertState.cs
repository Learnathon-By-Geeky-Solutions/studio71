using UnityEngine;
using System.Collections;

public class AlertState : IEnemyState
{
    private bool isWaiting = false;

    public void EnterState(EnemyAI enemy)
    {
        enemy.Agent.SetDestination(enemy.transform.position);
        enemy.Animator.SetTrigger("Alert");
        Debug.Log("Enemy is alert! Pausing before chasing...");
        enemy.Agent.isStopped = true;

        enemy.StartCoroutine(AlertPause(enemy));
    }

    public void UpdateState(EnemyAI enemy)
    {
        // No actions while in alert mode
    }

    private IEnumerator AlertPause(EnemyAI enemy)
    {
        yield return new WaitForSeconds(3f);
        enemy.Agent.isStopped = false;
        enemy.SetState(new FollowState());
    }
}