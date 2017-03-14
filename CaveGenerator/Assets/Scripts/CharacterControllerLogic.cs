using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerLogic : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float directionDampTime = .25f;
    [SerializeField]
    private ThirdPersonCam gamecam;
    [SerializeField]
    private float directionSpeed = 10f;
    [SerializeField]
    private float rotationDegreePerSecond = 120f;

    private float speed = 0.0f;
    private float direction = 0f;
    private float horizontal = 0.0f;
    private float vertical = 0.0f;
    private AnimatorStateInfo stateInfo;

    private int locomotionId = 0;

    void Start()
    {
        animator = GetComponent<Animator>();

        if(animator.layerCount >= 2)
        {
            animator.SetLayerWeight(1, 1);
        }

        locomotionId = Animator.StringToHash("Base Layer.Locomotion");
    }

    void Update()
    {
        if (animator)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            ControllToWorldSpace(transform, gamecam.transform, ref direction, ref speed);

            animator.SetFloat("Velocity", speed);
            animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);
            
        }
    }

    void FixedUpdate()
    {
        if(IsInLocomotion() && ((direction >= 0 && horizontal >=0) || (direction < 0 && horizontal < 0)))
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegreePerSecond * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            transform.rotation = (transform.rotation * deltaRotation);
        }
    }

    public void ControllToWorldSpace(Transform root, Transform camera, ref float directionOut, ref float speedOut)
    {
        Vector3 rootDirection = root.forward;

        Vector3 keyDirection = new Vector3(horizontal, 0, vertical);

        speedOut = keyDirection.sqrMagnitude;

        //get camera rotation
        Vector3 CameraDirection = camera.forward;
        CameraDirection.y = 0.0f; // kill Y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, CameraDirection);

        Vector3 moveDirection = referentialShift * keyDirection;
        Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

        float angleRootToMove = Vector3.Angle(rootDirection,moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

        angleRootToMove /= 180f;
        directionOut = angleRootToMove * directionSpeed;
    }

    public bool IsInLocomotion()
    {
        return stateInfo.fullPathHash == locomotionId;
    }

}
