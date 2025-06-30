using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float viewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask = -1;

    [Header("Chase")]
    [SerializeField] private float chaseForgetTime = 3f;

    [Header("Patrol")]
    [SerializeField] private float wanderInterval = 4f;
    [SerializeField] private float wanderRadius = 10f;

    private NavMeshAgent agent;

    private enum State { Patrolling, Chasing }
    private State state = State.Patrolling;
    private float nextWanderTime = 0f;
    private float lastSeenTime = -Mathf.Infinity;

    public bool PlayerDetected { get; private set; }

    /* ---------- Initialisation ---------- */

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Replaces the deprecated FindObjectOfType
        if (target == null)
        {
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();   // ✅ new API
            // Or: FindAnyObjectByType<PlayerMovement>() for max speed
            if (pm) target = pm.transform;
        }
    }

    void Start() => Wander();

    /* ---------- Main Loop ---------- */

    void Update()
    {
        if (!target) return;

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            lastSeenTime = Time.time;
            state = State.Chasing;
            PlayerDetected = true;
        }
        else if (Time.time - lastSeenTime > chaseForgetTime)
        {
            PlayerDetected = false;
            state = State.Patrolling;
        }

        if (state == State.Chasing)
        {
            agent.SetDestination(target.position);
        }
        else if (!agent.hasPath || Time.time >= nextWanderTime)
        {
            Wander();
        }
    }

    /* ---------- Helpers ---------- */

    bool CanSeePlayer()
    {
        Vector3 dir = target.position - transform.position;

        if (dir.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized,
                            out RaycastHit hit, dir.magnitude, obstacleMask))
        {
            if (!hit.transform.IsChildOf(target) && hit.transform != target) return false;
        }

        if (target.TryGetComponent(out PlayerHiding hiding) &&
            hiding.IsHiding && Time.time - lastSeenTime > 0.2f)
        {
            return false;
        }

        return true;
    }

    void Wander()
    {
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        nextWanderTime = Time.time + wanderInterval;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == target)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}