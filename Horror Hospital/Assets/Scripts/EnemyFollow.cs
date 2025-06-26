using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public float followDistance = 20f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (target == null)
        {
            playerMovement pm = FindObjectOfType<playerMovement>();
            if (pm != null)
                target = pm.transform;
        }
    }

    void Update()
    {
        if (target == null)
            return;

        PlayerHiding hiding = target.GetComponent<PlayerHiding>();
        bool targetVisible = hiding == null || !hiding.IsHiding;

        if (targetVisible && Vector3.Distance(transform.position, target.position) <= followDistance)
            agent.SetDestination(target.position);
        else
            agent.ResetPath();
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerHiding hiding = collision.gameObject.GetComponent<PlayerHiding>();
        if (hiding != null && !hiding.IsHiding)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}