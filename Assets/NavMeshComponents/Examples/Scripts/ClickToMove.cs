using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    NavMeshAgent m_Agent;
    RaycastHit m_HitInfo = new RaycastHit();

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore))
            {
                m_Agent.destination = m_HitInfo.point;

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = m_HitInfo.point;
            }
        }
    }
}
