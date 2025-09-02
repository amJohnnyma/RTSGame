/*
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
    public float worldRadius;
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
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EntitySurfaceMove : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Collider worldCollider;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float surfaceOffset = 0.1f;   // keep entity slightly above ground
    public float raycastDownDist = 5f;
    public float stopFollowDist = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // we’ll handle sticking to surface manually
    }

    void FixedUpdate()
    {
        if (target == null || worldCollider == null || (target.position - transform.position).magnitude < stopFollowDist)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // --- Step 1: Find ground point & normal ---
        Vector3 downDir = -transform.up;
        if (!Physics.Raycast(transform.position + downDir * 0.1f, downDir, out RaycastHit hit, raycastDownDist, 1 << worldCollider.gameObject.layer))
        {
            // Fallback: project to collider anyway
            return;
        }

        Vector3 groundPoint = hit.point;
        Vector3 groundNormal = hit.normal;

        // --- Step 2: Compute movement direction ---
        Vector3 toTarget = (target.position - groundPoint).normalized;
        Vector3 moveDir = Vector3.ProjectOnPlane(toTarget, groundNormal).normalized;

        // --- Step 3: Apply velocity ---
        Vector3 desiredVel = moveDir * moveSpeed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVel, Time.fixedDeltaTime * turnSpeed);

        // --- Step 4: Stick to ground ---
        rb.MovePosition(groundPoint + groundNormal * surfaceOffset);

        // --- Step 5: Align upright with ground ---
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * turnSpeed));
    }
}
