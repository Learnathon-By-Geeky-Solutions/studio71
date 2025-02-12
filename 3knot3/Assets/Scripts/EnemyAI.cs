using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum State { Idle, Follow, Shoot }
    private State currentState;

    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float shootingRange = 5f;
    [SerializeField] private float minShootingDistance = 3f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float fieldOfView = 60f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private NavMeshAgent agent;
    private Animator animator;
    private float nextFireTime;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = State.Idle;
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // State Transitions
        if (distance > detectionRange)
            currentState = State.Idle;
        else if (distance <= detectionRange && distance > shootingRange)
            currentState = State.Follow;
        else if (distance <= shootingRange && HasLineOfSight() && IsPlayerInFOV())
            currentState = State.Shoot;
        else
            currentState = State.Follow;

        // Update Animator Parameters
        animator.SetBool("IsChasing", currentState == State.Follow);
        animator.SetBool("IsShooting", currentState == State.Shoot);

        // State Behavior
        switch (currentState)
        {
            case State.Idle:
                Patrol();
                break;
            case State.Follow:
                FollowPlayer();
                break;
            case State.Shoot:
                ShootPlayer();
                break;
        }
    }

    private void Patrol()
    {
        agent.speed = moveSpeed * 0.5f;
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void FollowPlayer()
    {
        agent.speed = moveSpeed;
        agent.SetDestination(player.position);
    }

    private void ShootPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // Maintain a minimum distance while shooting
        if (distance < minShootingDistance)
        {
            Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
            Vector3 targetPosition = player.position + directionAwayFromPlayer * minShootingDistance;
            agent.SetDestination(targetPosition);
        }
        else if (distance > shootingRange)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }

        // Rotate Toward Player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Fire if Ready
        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.linearVelocity = firePoint.forward * 10f;
            Destroy(bullet, 3f);
        }
    }

    private bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 direction = (player.position - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, direction, out hit, shootingRange))
        {
            return hit.transform == player;
        }
        return false;
    }

    private bool IsPlayerInFOV()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        return angleToPlayer < fieldOfView * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw shooting range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // Draw minimum shooting distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minShootingDistance);

        // Draw field of view
        Gizmos.color = Color.blue;
        Vector3 fovLine1 = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 fovLine2 = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

        // Draw line of sight
        if (player != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.green : Color.red;
            Gizmos.DrawLine(firePoint.position, player.position);
        }
    }
}