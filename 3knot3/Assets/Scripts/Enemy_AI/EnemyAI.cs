using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform Player { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }

    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] public float detectionRange = 10f;
    [SerializeField] public float shootingRange = 5f;
    [SerializeField] public float minShootingDistance = 3f;
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float fieldOfView = 60f;

    private float nextFireTime;
    private IEnemyState currentState;

    public Transform FirePoint
    {
        get => firePoint;
        set => firePoint = value;
    }

    public GameObject BulletPrefab
    {
        get => bulletPrefab;
        set => bulletPrefab = value;
    }

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            Player = playerObject.transform;

        SetState(new IdleState());
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void SetState(IEnemyState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }

    public bool HasLineOfSight()
    {
        if (Player == null || FirePoint == null)
            return false;

        RaycastHit hit;
        Vector3 direction = (Player.position - FirePoint.position).normalized;
        if (Physics.Raycast(FirePoint.position, direction, out hit, shootingRange))
        {
            return hit.transform == Player;
        }
        return false;
    }

    public bool IsPlayerInFOV()
    {
        if (Player == null) return false;

        Vector3 directionToPlayer = (Player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        return angleToPlayer < fieldOfView * 0.5f;
    }

    public bool CanShoot() => Time.time > nextFireTime;

    public void ResetFireCooldown() => nextFireTime = Time.time + fireRate;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}