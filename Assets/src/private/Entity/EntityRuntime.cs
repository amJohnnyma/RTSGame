using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EntityRuntime : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Transform home;
    public Transform mainTarget;
    public Collider worldCollider;

    public bool returningHome = true;


    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float surfaceOffset = 0.1f;
    public float raycastDownDist = 5f;
    public float stopFollowDist = 0.1f;

    [Header("Nearby Point Detection")]
    public float radius = 5f;
    public int checkPoints = 8;

    [HideInInspector] public Rigidbody rb;

    // Parallelized computation results
    [HideInInspector] public Vector3 nextMoveDir;
    [HideInInspector] public Vector3[] lastNearbyPoints;
    [HideInInspector] public int chosenScore;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        target = (target == null) ? home : target;
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
        //this will be replaced with whatever the purpose is of the Entity (fetch, retrieve, etc etc)
        if (target == mainTarget)
        {
            mainTarget.gameObject.GetComponent<Inventory>().GiveItemToOther("Red_Flower", 1, this.GetComponent<Inventory>());
        }
        else if (target == home)
        {
            this.GetComponent<Inventory>().GiveItemToOther("Red_Flower", int.MaxValue, home.gameObject.GetComponent<Inventory>());

        }
        else
        {
            Debug.Log("Error inv trans");

        }
        target = (!returningHome) ? home : mainTarget;

        returningHome = !returningHome;

    }
}
