using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RunningAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    
    // Update is called once per frame
    void Update()
    {
        animator.SetBool("running", agent.velocity.magnitude > .01f);
    }
}
