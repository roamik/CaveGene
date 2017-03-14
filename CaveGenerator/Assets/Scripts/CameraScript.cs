using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const float minimum_Y_angle = 10.0f;
    private const float maximum_Y_angle = 50.0f;

    public Transform lookAt;
    public Transform camTransform;

    private Camera cam;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    private float sensitivityX = 4.0f;
    private float sensitivityY = 1.0f;
    
    private float zoom;
    public float zoomSpeed = 3f;

    public float zoomMin = -4f;
    public float zoomMax = -12f;

    private void Start()
    {
        zoom = -10f;

        camTransform = transform;
        cam = Camera.main;
    }

    private void Update()
    {
        currentX += Input.GetAxis("Mouse X");
        currentY -= Input.GetAxis("Mouse Y");

        currentY = Mathf.Clamp(currentY, minimum_Y_angle, maximum_Y_angle);

        zoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        if (zoom > zoomMin)
        {
            zoom = zoomMin;
        }

        if(zoom < zoomMax)
        {
            zoom = zoomMax;
        }
        
    }

    private void LateUpdate()
    {
        Vector3 direction = new Vector3(0,0, zoom);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        camTransform.position = lookAt.position + rotation * direction;
        camTransform.LookAt(lookAt.position);
    }
}
