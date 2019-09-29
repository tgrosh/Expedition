using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject target;
    public float range;
    public float attackInterval; //how many seconds per attack
    public Animator animator;

    float attackTimer;

    void Update()
    {
        if (target != null &&
            Vector3.Distance(transform.position, target.transform.position) <= range && 
            attackTimer >= attackInterval)
        {
            //attack
            animator.SetTrigger("attack");
            attackTimer = 0;
        } else
        {
            attackTimer += Time.deltaTime;
        }
    }
}
