using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomer : Shopper
{
    public bool turned = false;

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
            visionDistance = 0f;
        }

        float distToDest = Vector2.Distance(transform.position, storeTarget);

        if (rightVision && rightVision.transform.CompareTag("Obstacle")&&
            visionDistance > 0f && distToDest > 0.5f)
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

        if (leftVision && leftVision.transform.CompareTag("Obstacle")
            && visionDistance >0f && distToDest > 0.5f)
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

}