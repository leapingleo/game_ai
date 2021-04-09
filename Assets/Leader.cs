using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Shopper
{
    public bool turned = false;
    Vector3 finalTarget;
    public bool followMouse;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {

        Vector2 targetForce;
        float fov = 40f;

        path = GetComponent<TestMove>().GetPath();

        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.41f;
        Vector3 endVisionAt = detectVisionStartAt + transform.up;
        Vector3 rightVisionAt = detectVisionStartAt + Quaternion.Euler(0, 0, fov) * transform.up;
        RaycastHit2D frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, 1f);
        RaycastHit2D rightVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, fov) * transform.up, 1f);
        
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);

        /**
        if (frontVision)
        {
            Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.5f;
           // nextWallFollow.transform.position = targetTurningPoint;

            target = targetTurningPoint;

            Debug.DrawLine(frontVision.point, frontVision.point + frontVision.normal * 1f, Color.yellow);
        }
        **/
        

        float distToDest = Vector2.Distance(transform.position, GetComponent<TestMove>().FinalTarget());

       // if (distToDest > 0)
       // {

            targetForce = Seek(nextTarget, 1f, distToDest);
            ApplyForce(targetForce);

            UpdatePath();
            UpdateMovement();
       // }
    }


    protected Vector2 AvoidObstacle(Vector3 velocity, float angle)
    {
        Vector3 displacement = transform.up;
        displacement = AdjustByAngle(displacement, angle, 1f);

        Vector3 wanderForce = velocity + displacement;

        return wanderForce;
    }
}