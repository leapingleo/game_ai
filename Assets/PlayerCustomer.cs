using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomer : Shopper
{
    List<Vector2> path;
    Vector2 nextTarget;
    public bool turned = false;
    Vector3 storeTarget;
    public bool followMouse;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        visionDistance = 0.6f;
        Vector2 targetForce;

        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.41f;
        // Vector3 endVisionAt = detectVisionStartAt + transform.up;
        Vector3 endVisionAt = detectVisionStartAt + transform.up * visionDistance;
        Vector3 leftVisionEnd = detectVisionStartAt + Quaternion.Euler(0, 0, visionAngle) * transform.up * visionDistance;
        Vector3 rightVisionEnd = detectVisionStartAt + Quaternion.Euler(0, 0, -visionAngle) * transform.up * visionDistance;
        frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, visionDistance);
        RaycastHit2D leftVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, visionAngle) * transform.up, visionDistance);
        RaycastHit2D rightVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, -visionAngle) * transform.up, visionDistance);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);
        //left vision ray
        Debug.DrawLine(detectVisionStartAt, leftVisionEnd, Color.yellow);
        //right vision ray
        Debug.DrawLine(detectVisionStartAt, rightVisionEnd, Color.black);

        if (Input.GetMouseButtonDown(0))
        {
            target = cam.ScreenToWorldPoint(Input.mousePosition);
            storeTarget = target;
            finalTarget = target;
            visionDistance = 0f;
        }

        float distToDest = Vector2.Distance(transform.position, storeTarget);

        if (rightVision && visionDistance > 0f && distToDest > 0.5f)
        {
            Vector2 targetTurningPoint = rightVision.point + rightVision.normal * (visionDistance);
            nextWallFollow.transform.position = targetTurningPoint;
            target = targetTurningPoint;
        }
        else
        {
            if (Vector2.Distance(transform.position, target) < 0.02f)
            {
                target = storeTarget;
            }
        }
        if (leftVision  && visionDistance >0f && distToDest > 0.5f)
        {
            Vector2 targetTurningPoint = leftVision.point + leftVision.normal * (visionDistance);
            nextWallFollow.transform.position = targetTurningPoint;
            target = targetTurningPoint;
        }
        else
        {
            if (Vector2.Distance(transform.position, target) < 0.02f)
            {
                target = storeTarget;
            }
        }



        targetForce = Seek(target, 1, distToDest);
        ApplyForce(targetForce);
        UpdateMovement();
    }

    void UpdatePath()
    {
        if (path == null)
        {
            return;
        }

        Vector2 arrivingNext = path[0];
     //   Debug.Log(arrivingNext);

        if (Vector2.SqrMagnitude(arrivingNext - (Vector2)transform.position) < 0.1f)
        {
            path.RemoveAt(0);
            nextTarget = path[0];
        }
    }

    protected Vector2 AvoidObstacle(Vector3 velocity, float angle)
    {
        Vector3 displacement = transform.up;
        displacement = AdjustByAngle(displacement, angle, 1f);
       
        Vector3 wanderForce = velocity + displacement;
    
        return wanderForce;
    }
}