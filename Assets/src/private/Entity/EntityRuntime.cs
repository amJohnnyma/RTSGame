using UnityEngine;

public enum EntityBehaviour
{
    DEFAULT,
    WANDER,
    SCOUT,
    ATTACK,
    HARVEST,
    TRANSPORT,
    DEFEND
}

[RequireComponent(typeof(Rigidbody))]
public class EntityRuntime : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Transform home;
    public Transform mainTarget;
    public Collider worldCollider;
    public World world;
    [HideInInspector] public Vector3 homePos;

    public bool returningHome = true;


    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float surfaceOffset = 0.1f;
    public float raycastDownDist = 5f;
    public float stopFollowDist = 0.1f;
    [SerializeField]
    public EntityBehaviour behaviour = EntityBehaviour.DEFAULT;

    [Header("Nearby Point Detection")]
    public float radius = 5f;
    public int checkPoints = 8;

    [HideInInspector] public Rigidbody rb;

    // Parallelized computation results
    [HideInInspector] public Vector3 nextMoveDir;
    [HideInInspector] public Vector3[] lastNearbyPoints;
    [HideInInspector] public int chosenScore;
    [HideInInspector] public IEntityBehaviour behaviorHandler;
    [HideInInspector] public Vector3 pendingTargetPos = Vector3.positiveInfinity;
    [HideInInspector] public Vector3 currentPos = Vector3.zero;

    [Header("Tasks")]
    [SerializeField] public TaskList taskList = new();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        target = (target == null) ? home : target;

        switch (behaviour)
        {
            case EntityBehaviour.SCOUT:
                behaviorHandler = new ScoutBehavior(world, radius);
                break;
            case EntityBehaviour.HARVEST:
                behaviorHandler = new HarvestBehaviour(world, radius / 2f);
                break;
            default:
                behaviorHandler = new ScoutBehavior(world);
                break;
        }

        homePos = home.transform.position;



    }

    void Start()
    {
        mainTarget = world.GetRandomPlacedEntity().transform;

    }

    private void OnDrawGizmos()
    {
        if (lastNearbyPoints == null) return;

        for (int i = 0; i < lastNearbyPoints.Length; i++)
        {
            Gizmos.color = (i == chosenScore) ? Color.blue : Color.red;
            Gizmos.DrawSphere(lastNearbyPoints[i], 0.1f);
            Gizmos.DrawLine(transform.position, lastNearbyPoints[i]);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void SetTargetHome()
    {

    }

    public void SetTargetTarget()
    {

    }

    public void TakeItem()
    {

    }

    public void SetTargetToggle()
    {
        // If mainTarget is null, pick a random entity in the world
        /*
        if (mainTarget == null)
        {
            var randomEntity = world.GetRandomPlacedEntity();
            if (randomEntity != null)
                mainTarget = randomEntity.transform;
        }
*/

    }

    public bool IsAtHome()
    {
        return Vector3.Distance(transform.position, home.transform.position) < stopFollowDist;
    }

    /*
public void SetTaskTargets(ITask task)
{
    switch (task.Type)
    {
        case TaskType.Home:
            target = home;
            mainTarget = home;
            break;
        case TaskType.Harvest:
        case TaskType.Scout:
        case TaskType.GoTo:
            target = task.TargetGameObject != null ? task.TargetGameObject.transform : null;
            mainTarget = target;
            break;
        default:
            target = home;
            mainTarget = home;
            break;
    }
}

    */


}
