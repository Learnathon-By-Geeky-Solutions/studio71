using UnityEngine;
using System.Collections;

public class ShootState : IEnemyState
{
    private bool isReloading = false;

    public void EnterState(EnemyAI enemy)
    {
        enemy.Animator.SetBool("IsShooting", true);
        enemy.Agent.SetDestination(enemy.transform.position);
        Debug.Log("Enemy is aiming and shooting!");
    }

    public void UpdateState(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

        if (distance > enemy.shootingRange || !enemy.HasLineOfSight() || !enemy.IsPlayerInFOV())
        {
            enemy.SetState(new FollowState());
        }
        else
        {
            MaintainShootingDistance(enemy);
            RotateTowardsPlayer(enemy);
            if (!isReloading) Shoot(enemy);
        }
    }

    private void MaintainShootingDistance(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

        if (distance < enemy.minShootingDistance)
        {
            Vector3 directionAwayFromPlayer = (enemy.transform.position - enemy.Player.position).normalized;
            Vector3 targetPosition = enemy.Player.position + directionAwayFromPlayer * enemy.minShootingDistance;
            enemy.Agent.SetDestination(targetPosition);
        }
        else
        {
            enemy.Agent.SetDestination(enemy.transform.position);
        }
    }

    private void RotateTowardsPlayer(EnemyAI enemy)
    {
        Vector3 direction = (enemy.Player.position - enemy.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void Shoot(EnemyAI enemy)
    {
        if (enemy.CanShoot())
        {
            Debug.Log("Enemy is shooting!");
            enemy.ResetFireCooldown();
            GameObject bullet = Object.Instantiate(enemy.BulletPrefab, enemy.FirePoint.position, enemy.FirePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.linearVelocity = enemy.FirePoint.forward * 10f;
            Object.Destroy(bullet, 3f);

            enemy.StartCoroutine(Reload(enemy));
        }
    }

    private IEnumerator Reload(EnemyAI enemy)
    {
        isReloading = true;
        Debug.Log("Enemy is reloading...");
        yield return new WaitForSeconds(enemy.reloadTime);
        isReloading = false;
    }
}
