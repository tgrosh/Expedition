using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public bool available = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, .25f);
        Gizmos.DrawSphere(transform.position, .5f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
