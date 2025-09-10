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
                behaviorHandler = new HarvestBehaviour();
                break;
            default:
                behaviorHandler = new ScoutBehavior(world);
                break;
        }



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

    public void SetTargetToggle()
    {
        // If mainTarget is null, pick a random entity in the world
        if (mainTarget == null)
        {
            var randomEntity = world.GetRandomPlacedEntity();
            if (randomEntity != null)
                mainTarget = randomEntity.transform;
        }

        // Scout logic
        if (behaviour == EntityBehaviour.SCOUT)
        {
            if (returningHome)
            {
                // Arrived home, now pick next target harvestable or wander
                EntityStats nextHarvestable = world.GetRandomPlacedHarvestable().GetComponent<EntityStats>();
                if (nextHarvestable != null)
                {
                    mainTarget = nextHarvestable.transform;
                    target = mainTarget;
                    returningHome = false;
                }
                else
                {
                    // No harvestables, pick random wander point
                    target = mainTarget; // default to mainTarget for wandering
                    returningHome = false;
                }
            }
            else
            {
                // Arrived at target (harvestable), go home next
                world.AddFoundHarvestable(target.transform.position, target.gameObject);
                target = home;
                returningHome = true;
            }
        }
        else if (behaviour == EntityBehaviour.HARVEST)
        {
            if (returningHome)
            {
                // Arrived home, now pick next target harvestable or wander

                EntityStats nextHarvestable = null;
                nextHarvestable = world.GetRandomFoundHarvestable().GetComponent<EntityStats>();

                if (nextHarvestable != null)
                {
                    mainTarget = nextHarvestable.transform;
                    target = mainTarget;
                    returningHome = false;
                }
                else
                {
                    // No harvestables, pick random wander point
                    target = mainTarget; // default to mainTarget for wandering
                    returningHome = false;
                }

                this.GetComponent<Inventory>().GiveItemToOther("Red_Flower", int.MaxValue, home.gameObject.GetComponent<Inventory>());
            }
            else
            {
                // Arrived at target (harvestable), go home next
                mainTarget.gameObject.GetComponent<Inventory>().GiveItemToOther("Red_Flower", 1, this.GetComponent<Inventory>());
                if (mainTarget.GetComponent<EntityInventory>().IsEmpty("Red_Flower"))
                {
                    Debug.Log("EMPTY");
                    world.DestroyFoundHarvestable(mainTarget.transform.position);

                    
                }

                target = home;
                returningHome = true;
            }

        }
        else
        {
            // Default behavior for other entities
            if (returningHome)
            {
                target = mainTarget;
                returningHome = false;
            }
            else
            {
                target = home;
                returningHome = true;
            }
        }
    }

}
