using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackballCamera : MonoBehaviour
{

    public Transform target;
    public float distance = 300f;
    public float sensitivity = 2f;
    public float zoomSpeed;
    public float minDistance = 2f;
    public float maxDistance = 50f;

    private Quaternion rotation;

    void Start()
    {
        if (target == null)
        {
            GameObject pivot = new GameObject("camera target");
            pivot.transform.position = Vector3.zero;
            target = pivot.transform;
        }

        rotation = transform.rotation;
        distance = Vector3.Distance(transform.position, target.position);

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;

            Quaternion yaw = Quaternion.AngleAxis(dx * sensitivity * distance, up);
            Quaternion pitch = Quaternion.AngleAxis(-dy * sensitivity * distance, right);

            rotation = yaw * pitch * rotation;

        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if(scrollInput > 0)
            distance -= (distance / zoomSpeed) * Time.deltaTime;
        if(scrollInput < 0)
            distance += (distance / zoomSpeed) * Time.deltaTime;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.SetPositionAndRotation(target.position + offset, rotation);
    }
}
