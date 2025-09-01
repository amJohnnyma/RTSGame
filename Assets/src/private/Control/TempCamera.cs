using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCamera : MonoBehaviour
{

    public float distance = 10.0f;
    public float speed = 50.0f;
    public float zoomSpeed;
    public float minDistance = 2f;
    public float maxDistance = 50f;

    private float yaw = 0f;
    private float pitch = 0f;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 dir = (transform.position - Vector3.zero).normalized;
        transform.position = dir * distance;
        transform.LookAt(Vector3.zero);
        
    }

    // Update is called once per frame
    void Update()
    {

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        yaw += h * speed * Time.deltaTime;
        pitch -= v * speed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);


        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 dir = rotation * Vector3.forward;

        transform.position = dir * distance;
        transform.LookAt(Vector3.zero);
        
    }
}
