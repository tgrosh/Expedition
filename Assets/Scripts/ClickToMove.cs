using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    public GameObject targetCirclePrefab;
    public LayerMask clickLayers;

    NavMeshAgent agent;
    RaycastHit hit = new RaycastHit();
    GameObject circle;
    GameObject target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (target != null)
        {
            agent.destination = target.transform.position;
        }

        if (agent.remainingDistance < agent.stoppingDistance && circle != null)
        {
            Destroy(circle);
        }

        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, clickLayers, QueryTriggerInteraction.Ignore))
            {
                Vector3 destination = hit.point;

                Targetable targetable = hit.collider.gameObject.transform.root.GetComponent<Targetable>();
                if (targetable == null)
                {
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(destination, path);

                    if (path.status != NavMeshPathStatus.PathPartial)
                    {
                        agent.destination = destination;
                        Destroy(circle);
                        circle = Instantiate(targetCirclePrefab);
                        circle.transform.position = destination;
                        target = null;
                    }
                } else
                {
                    target = targetable.gameObject;
                }
            }
        }
    }
}
