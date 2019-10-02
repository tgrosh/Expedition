using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float meleeRange;

    NavMeshAgent agent;
    List<GameObject> targets = new List<GameObject>();
    float agentStoppingDistance;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentStoppingDistance = agent.stoppingDistance;

        NavMeshHit closestHit;
        if (NavMesh.SamplePosition(transform.position, out closestHit, 100f, NavMesh.AllAreas))
        {
            transform.position = closestHit.position;
            agent.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targets.Count > 0)
        {
            agent.destination = targets[0].transform.position;
            agent.stoppingDistance = meleeRange;

            Attack attack = GetComponent<Attack>();
            if (attack != null)
            {
                Attackable attackable = targets[0].GetComponent<Attackable>();
                if (attackable != null)
                {
                    //this object can attack, and the target is attackable
                    attack.target = targets[0].gameObject;
                }
            }
        }
        else
        {
            agent.stoppingDistance = agentStoppingDistance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Player"))
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Player"))
        {
            targets.Remove(other.gameObject);
        }
    }
}
