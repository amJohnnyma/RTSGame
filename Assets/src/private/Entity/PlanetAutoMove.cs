using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetAutoMove : MonoBehaviour
{
    public Transform planetCenter;   // center of the planet
    public float planetRadius = 10f;
    public Transform target;         // target to move toward
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float gravityStrength = 50f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (target == null || planetCenter == null) return;

        Vector3 toCenter = (planetCenter.position - transform.position).normalized;
        Vector3 surfaceNormal = -toCenter; // outward normal

        // --- Apply inward gravity ---
        rb.AddForce(toCenter * gravityStrength, ForceMode.Acceleration);

        // --- Project target direction onto tangent plane ---
        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 tangentDir = Vector3.ProjectOnPlane(toTarget, surfaceNormal).normalized;

        // --- Move along tangent ---
        rb.MovePosition(rb.position + tangentDir * moveSpeed * Time.fixedDeltaTime);

        // --- Keep exactly on surface ---
        rb.position = planetCenter.position + (rb.position - planetCenter.position).normalized * planetRadius;

        // --- Rotate upright ---
        Quaternion targetRotation = Quaternion.LookRotation(tangentDir, surfaceNormal * -1f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}



/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetAutoMove : MonoBehaviour
{
    public Transform planetCenter;   // center of the planet
    public float planetRadius = 10f;
    public Transform target;         // target to move toward
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float gravityStrength = 50f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (target == null || planetCenter == null) return;

        Vector3 toCenter = (planetCenter.position - transform.position).normalized;
        Vector3 surfaceNormal = -toCenter; // outward normal

        // --- Apply inward gravity ---
        rb.AddForce(toCenter * gravityStrength, ForceMode.Acceleration);

        // --- Project target direction onto tangent plane ---
        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 tangentDir = Vector3.ProjectOnPlane(toTarget, surfaceNormal).normalized;

        // --- Move along tangent ---
        rb.MovePosition(rb.position + tangentDir * moveSpeed * Time.fixedDeltaTime);

        // --- Keep exactly on surface ---
        rb.position = planetCenter.position + (rb.position - planetCenter.position).normalized * planetRadius;

        // --- Rotate upright ---
        Quaternion targetRotation = Quaternion.LookRotation(tangentDir, surfaceNormal * -1f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}


*/