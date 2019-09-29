using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    public GameObject targetCirclePrefab;

    NavMeshAgent m_Agent;
    RaycastHit m_HitInfo = new RaycastHit();
    GameObject circle;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (m_Agent.remainingDistance < m_Agent.stoppingDistance && circle != null)
        {
            Destroy(circle);
        }

        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore))
            {
                m_Agent.destination = m_HitInfo.point;
                Destroy(circle);
                circle = Instantiate(targetCirclePrefab);
                circle.transform.position = m_Agent.destination;
            }
        }
    }
}
