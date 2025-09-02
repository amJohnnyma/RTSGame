
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EntitySurfaceMove : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Collider worldCollider;

    [Header("Nearby Point Detection")]
    public float radius = 5f;
    public int checkPoints = 8;
    private Vector3[] lastNearbyPoints;
    private int chosenScore = 0;

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

        Vector3 tempTarget = GetNextTarget();


        Vector3 toTarget = (tempTarget - groundPoint).normalized;
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

    // Get next target (this is our intermediate target to get to the actual target)
    // -> Cast sphere collider
    // -> Get 8 points of collision
    // -> Use heuristic and probabalistic selection to choose target


    private Vector3 GetNextTarget()
    {
        // get nearby points
        // Assign scores
        // probabalistic decision
        //get nearby points
        lastNearbyPoints = NearbyPoints();
        float[] scores = AssignScores(lastNearbyPoints);
        int idx = BestScoreIndex(scores);
        return lastNearbyPoints[idx];
    }


    private Vector3[] NearbyPoints()
    {
        List<Vector3> points = new List<Vector3>();

        // directions around the sphere
        List<Vector3> dirs = GetSphereDirections(checkPoints);

        foreach (var dir in dirs)
        {
            // cast outward from center in each direction
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, radius, 1 << worldCollider.gameObject.layer))
            {
                points.Add(hit.point);
            }
        }

        return points.ToArray();
    }

    // Fibonacci sphere sampling
    private List<Vector3> GetSphereDirections(int samples)
    {
        List<Vector3> dirs = new List<Vector3>(samples);
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f)); // golden angle

        for (int i = 0; i < samples; i++)
        {
            float y = 1f - (i / (float)(samples - 1)) * 2f; // y goes from 1 to -1
            float radius = Mathf.Sqrt(1 - y * y);

            float theta = phi * i;

            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            dirs.Add(new Vector3(x, y, z));
        }
        return dirs;
    }

    private float[] AssignScores(Vector3[] t)
    {
        float[] scores = new float[t.Length];
        for (int i = 0; i < t.Length; i++)
        {
            float dist = Vector3.Distance(t[i], target.position);
            scores[i] = 1f / (dist + 0.001f);
        }
        return scores;
    }

    private int BestScoreIndex(float[] scores)
    {
        int bestIndex = 0;
        float bestScore = scores[0];
        for (int i = 1; i < scores.Length; i++)
        {
            if (scores[i] > bestScore)
            {
                bestScore = scores[i];
                bestIndex = i;
            }
        }

        chosenScore = bestIndex;
        return bestIndex;
    }

        // Gizmo Visualization
        // -------------------------
    private void OnDrawGizmos()
    {
        // Draw sphere sampling radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw last detected collision points
        if (lastNearbyPoints != null)
        {
            for (int i = 0; i < lastNearbyPoints.Length; i++)
            {
                if (i == chosenScore)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawSphere(lastNearbyPoints[i], 0.1f);
                Gizmos.DrawLine(transform.position, lastNearbyPoints[i]);
            }
        }
    }

}
