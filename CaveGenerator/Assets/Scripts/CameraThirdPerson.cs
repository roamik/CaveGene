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
    private Transform thePlayer;
    private Transform spotPosition;
    
    public Vector2 pitchMinMax = new Vector2(0, 85);

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;


    float yaw;
    float pitch;

    void Start()
    {
        thePlayer = GameObject.FindWithTag("Player").transform;
        
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
    }

	void LateUpdate ()
    {
        Vector3 characterOffset = thePlayer.position;
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

    private void CompensateForWalls (Vector3 fromObject, ref Vector3 toTarget)
    {
        RaycastHit wallHit = new RaycastHit();
        if(Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
        }
    }
}
