
using UnityEngine;

/*
    This file has a commented version with details about how each line works. 
    The commented version contains code that is easier and simpler to read. This file is minified.
*/

/// <summary>
/// Camera movement script for third person games.
/// This Script should not be applied to the camera! It is attached to an empty object and inside
/// it (as a child object) should be your game's MainCamera.
/// </summary>
public class CameraController : MonoBehaviour
{

    [Tooltip("Enable to move the camera by holding the right mouse button. Does not work with joysticks.")]
    public bool clickToMoveCamera = false;
    [Tooltip("Enable zoom in/out when scrolling the mouse wheel. Does not work with joysticks.")]
    public bool canZoom = true;
    [Space]
    [Tooltip("The higher it is, the faster the camera moves. It is recommended to increase this value for games that uses joystick.")]
    public float sensitivity = 5f;

    [Tooltip("Camera Y rotation limits. The X axis is the maximum it can go up and the Y axis is the maximum it can go down.")]
    public Vector2 cameraLimit = new Vector2(-45, 40);

    public float distanceFromPlayer = 5f; // Distance camera stays from player
    public float heightOffset = 2f;       // Height offset above player

    float mouseX;
    float mouseY;
    float offsetDistanceY;

    Transform player;

    void Start()
    {

        player = GameObject.FindWithTag("Player").transform;
        

        // Lock and hide cursor with option isn't checked
        if ( ! clickToMoveCamera )
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

    }


    void LateUpdate()
    {
        if (player == null) return;

        // Determine local "up" vector from sphere center
        Vector3 upDir = (player.position - Vector3.zero).normalized; // assuming sphere center at 0,0,0

        // Camera rotation
        if (clickToMoveCamera)
        {
            if (Input.GetAxisRaw("Fire2") != 0)
            {
                mouseX += Input.GetAxis("Mouse X") * sensitivity;
                mouseY += Input.GetAxis("Mouse Y") * sensitivity;
                mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);
            }
        }
        else
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY += Input.GetAxis("Mouse Y") * sensitivity;
            mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);
        }

        // Calculate rotation relative to player's local up
        Quaternion rotation = Quaternion.AngleAxis(mouseX, upDir) * Quaternion.AngleAxis(-mouseY, transform.right);

        // Position camera behind player along rotation
        Vector3 offset = rotation * new Vector3(0, 0, -distanceFromPlayer);
        transform.position = player.position + upDir * heightOffset + offset;

        // Make camera look at player
        transform.LookAt(player.position + upDir * heightOffset, upDir);

        // Zoom with scroll wheel
        if (canZoom && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            distanceFromPlayer -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
            distanceFromPlayer = Mathf.Clamp(distanceFromPlayer, 2f, 15f); // optional min/max zoom
        }
    }

}