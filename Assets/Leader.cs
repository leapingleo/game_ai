using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Shopper
{
    List<Vector2> path;
    Vector2 nextTarget;
    public bool turned = false;
    Vector3 storeTarget;
    public GameObject nextWallFollow;
    public bool followMouse;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {

        Vector2 targetForce;
        float fov = 40f;

        if (Input.GetMouseButtonDown(0))
        {
            target = cam.ScreenToWorldPoint(Input.mousePosition);
            storeTarget = target;
        }
        path = GetComponent<TestMove>().GetPath();

        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.41f;
       // Vector3 endVisionAt = detectVisionStartAt + transform.up;
        Vector3 endVisionAt = detectVisionStartAt + transform.up;
        Vector3 rightVisionAt = detectVisionStartAt + Quaternion.Euler(0, 0, fov) * transform.up;
        RaycastHit2D frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, 1f);
        RaycastHit2D rightVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, fov) * transform.up, 1f);
        //  RaycastHit2D rightVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, -25) * transform.up, 0.6f);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);
       // Debug.DrawLine(detectVisionStartAt, rightVisionAt, Color.green);

        // Debug.DrawRay(detectVisionStartAt,transform.up);
        // Debug.DrawRay(detectVisionStartAt, Quaternion.Euler(0, 0, -25) * transform.up);


        if (frontVision)
        {
            Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.5f ;
            nextWallFollow.transform.position = targetTurningPoint;

            target = targetTurningPoint;

            Debug.DrawLine(frontVision.point, frontVision.point + frontVision.normal*1f, Color.yellow);
           

        } else
        {
            if (Vector2.Distance(transform.position, target) < 0.02f)
                target = storeTarget;
        }

        if (followMouse)
            targetForce = Seek(target, 1f);
        else
            targetForce = Seek(nextTarget, 1f);

        ApplyForce(targetForce);

        UpdatePath();
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
