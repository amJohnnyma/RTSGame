using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AntController : MonoBehaviour
{
    [Header("References")]
    public Transform worldCenter; // The planet's center

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float turnSpeed = 5f;
    public float gravityStrength = 50f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // we'll apply custom gravity
    }

    void FixedUpdate()
    {
        if (worldCenter == null) return;

        Vector3 currentPos = rb.position;
        Vector3 toCenter = (worldCenter.position - currentPos).normalized; // inward gravity
        Vector3 surfaceNormal = -toCenter; // outward normal

        // --- Apply custom gravity ---
        rb.AddForce(toCenter * gravityStrength, ForceMode.Acceleration);

        // --- Get WASD input ---
        float h = Input.GetAxis("Horizontal"); // A/D, Left/Right
        float v = Input.GetAxis("Vertical");   // W/S, Up/Down

        Vector3 inputDir = new Vector3(h, 0f, v);

        if (inputDir.sqrMagnitude > 0.01f)
        {
            // Convert input to world space relative to camera
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward = Vector3.ProjectOnPlane(camForward, surfaceNormal).normalized;
            camRight = Vector3.ProjectOnPlane(camRight, surfaceNormal).normalized;

            Vector3 moveDir = (camForward * v + camRight * h).normalized;

            // Apply velocity along tangent
            Vector3 desiredVelocity = moveDir * moveSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, Time.fixedDeltaTime * turnSpeed);
        }
        else
        {
            // No input → stop horizontal movement but keep sticking to surface
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * turnSpeed);
        }
    }
}