using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTarget : MonoBehaviour
{
    Targetable targetable;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore))
            {
                if (targetable != null)
                {
                    targetable.ClearTarget();
                }
                targetable = hit.collider.gameObject.transform.root.GetComponent<Targetable>();
                if (targetable != null)
                {
                    targetable.SetTarget();
                }
            }
        }
    }
}
