using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour {

    [SerializeField]
    private float distanceAway;
    [SerializeField]
    private float distanceUp;
    [SerializeField]
    private float smooth;
    [SerializeField]
    private Transform followXform;


    private Vector3 lookDirection;
    private Vector3 targetPosition;

    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float camSmoothDampTime = 0.1f;

    private CamStates camstate = CamStates.Behind;

    public enum CamStates
    {
        Behind,
        Target,
        Free
    }

    // Use this for initialization
    void Start ()
    {
        followXform = GameObject.FindWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LateUpdate()
    {
        Vector3 characterOffset = followXform.position + new Vector3(0f, distanceUp, 0f);

        if(Input.GetAxis("Target") > 0.01f)
        {
            camstate = CamStates.Target;
        }
        else
        {
            camstate = CamStates.Behind;
        }

        switch (camstate)
        {
            case CamStates.Behind:
                {
                    lookDirection = characterOffset - this.transform.position;
                    lookDirection.y = 0;
                    lookDirection.Normalize();

                    targetPosition = characterOffset + followXform.up * distanceUp - lookDirection * distanceAway;
                    break;
                }
            case CamStates.Target:
                {
                    targetPosition = characterOffset + followXform.up * distanceUp - followXform.forward * distanceAway;
                    break;
                }
        }

        CompensateForWalls(characterOffset, ref targetPosition);

        smoothPosition (this.transform.position, targetPosition);
        transform.LookAt(followXform);
    }

    private void smoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
    {
        RaycastHit wallHit = new RaycastHit();
        if(Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
        }
    }
}
