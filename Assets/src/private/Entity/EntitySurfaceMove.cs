using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class EntitySurfaceMove : MonoBehaviour
{

    [Header("References")]
    public Transform target;
    public Collider worldCollider;
    public Transform worldCenter;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float obstacleCheckDist = 5f;
    public float avoidAngle = 5f;

    private Rigidbody rb;
    private float worldRadius;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (worldCollider != null)
        {
            worldRadius = worldCollider.bounds.extents.magnitude;
        }
        rb.useGravity = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null || worldCollider == null) return;

        Vector3 currentPos = rb.position.normalized * worldRadius;
        Vector3 targetPos = target.position.normalized * worldRadius;


        Vector3 toTarget = Vector3.ProjectOnPlane(targetPos - currentPos, currentPos).normalized;
        Vector3 moveDir = toTarget;


        if (Physics.Raycast(currentPos, toTarget, out RaycastHit hit, obstacleCheckDist))
        {
            if (hit.collider == worldCollider)
            {
                Vector3 normal = currentPos.normalized;

                Vector3 leftDir = Quaternion.AngleAxis(-avoidAngle, normal) * toTarget;
                Vector3 rightDir = Quaternion.AngleAxis(avoidAngle, normal) * toTarget;


                float leftScore = Vector3.Dot((targetPos - currentPos).normalized, leftDir);
                float rightScore = Vector3.Dot((targetPos - currentPos).normalized, rightDir);

                moveDir = (leftScore > rightScore) ? leftDir : rightDir;

            }
        }
        Vector3 desiredV = moveDir * moveSpeed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredV, Time.fixedDeltaTime * turnSpeed);

        rb.position = rb.position.normalized * worldRadius;

    }
}
