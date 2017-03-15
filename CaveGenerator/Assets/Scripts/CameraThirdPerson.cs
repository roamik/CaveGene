using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraThirdPerson : MonoBehaviour
{
    public bool lockCursor;

    public float mouseSensitivity = 10;
    public float distFromTarget = 6;

    public float zoomSpeed = 3f;

    public float zoomMin = 5f;
    public float zoomMax = 12f;

    public Transform target;
    
    public Vector2 pitchMinMax = new Vector2(0, 85);

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

	void LateUpdate ()
    {
        distFromTarget -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        if (distFromTarget <= zoomMin)
        {
            distFromTarget = zoomMin;
        }

        if (distFromTarget >= zoomMax)
        {
            distFromTarget = zoomMax;
        }

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * distFromTarget;
	}
}
