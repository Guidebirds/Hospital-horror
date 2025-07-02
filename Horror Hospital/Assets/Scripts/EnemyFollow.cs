using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public float viewDistance = 15f;
    public float viewAngle = 120f;
    public float chaseForgetTime = 3f;
    public float wanderInterval = 4f;
    public float wanderRadius = 10f;
    public LayerMask obstacleMask = -1;

    private NavMeshAgent agent;

    private enum State { Patrolling, Chasing }
    private State state = State.Patrolling;

    private float nextWanderTime = 0f;
    private float lastSeenTime = -Mathf.Infinity;

    public bool PlayerDetected { get; private set; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (target == null)
        {
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null)
                target = pm.transform;
        }

        Wander();
    }

    void Update()
    {
        if (target == null)
            return;

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
        else
        {
            if (!agent.hasPath || Time.time >= nextWanderTime)
                Wander();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dir = target.position - transform.position;
        if (dir.magnitude > viewDistance)
            return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f)
            return false;
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, dir.magnitude, obstacleMask))
        {
            if (!hit.transform.IsChildOf(target) && hit.transform != target)
                return false;
        }
        PlayerHiding hiding = target.GetComponent<PlayerHiding>();
        if (hiding != null && hiding.IsHiding && Time.time - lastSeenTime > 0.2f)
            return false;
        return true;
    }

    void Wander()
    {
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        nextWanderTime = Time.time + wanderInterval;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == target)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}