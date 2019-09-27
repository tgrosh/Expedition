using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimatedAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    
    // Update is called once per frame
    void Update()
    {
        float currentVelocity = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("velocity", currentVelocity);
    }
}
