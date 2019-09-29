using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    public GameObject targetMarkerPrefab;    
    GameObject marker;
    
    public void SetTarget()
    {
        marker = Instantiate(targetMarkerPrefab, transform.position, Quaternion.identity, transform);
    }

    public void ClearTarget()
    {
        Destroy(marker);
    }
}
