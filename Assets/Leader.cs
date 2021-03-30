using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Shopper
{
    List<Vector2> path;
    Vector2 nextTarget;

    public override void Start()
    {
        base.Start();
        //  nextTarget = transform.position;
       // path = GetComponent<TestMove>().GetPath();
    }

    private void Update()
    {
      //  if (path == null)
            path = GetComponent<TestMove>().GetPath();

      //  if (!GetComponent<TestMove>().sameDest() || GetComponent<TestMove>().reset)
      //  {
      //      path = new List<Vector2>();
       //     path = GetComponent<TestMove>().GetPath();
       // }
       

        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.55f;
        Vector3 endVisionAt = detectVisionStartAt + transform.up * 0.55f;
        RaycastHit2D frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, 1);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);
        Vector2 avoidObstacle;

        if (frontVision)
        {
            Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.yellow);
           
            avoidObstacle = AvoidObstacle(velocity);
         //   ApplyForce(avoidObstacle);
        }

        RaycastHit2D leftVision = Physics2D.Raycast(transform.position, -transform.right, 1<<4,1);
      //  Debug.DrawLine(transform.position, transform.position + transform.right * -1f, Color.red);

        if (leftVision)
        {
          //    Vector3 followWall = leftVision.point + leftVision.normal;
          //     ApplyForce(Seek(followWall, 1f));
            Debug.DrawLine(leftVision.point, leftVision.point + leftVision.normal, Color.cyan);
            Debug.DrawLine(transform.position, leftVision.point, Color.black);
        }

        RaycastHit2D rightVision = Physics2D.Raycast(endVisionAt, transform.right, 1);
     //   Debug.DrawLine(endVisionAt, endVisionAt + transform.right, Color.blue);

        if (rightVision)
        {
       //     Debug.Log("right");
        }
        


        //  base.Update();
        


        //if (Input.GetMouseButtonDown(0))
        //{
        //    target = cam.ScreenToWorldPoint(Input.mousePosition);
        //}
        UpdatePath();
        Vector2 targetForce;
        Debug.Log(nextTarget);

        targetForce = Seek(nextTarget, 0);
        ApplyForce(targetForce);

        UpdateMovement();
    }

    void UpdatePath()
    {
        if (path == null || path.Count == 0)
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


    protected Vector2 AvoidObstacle(Vector3 velocity)
    {
        Vector3 displacement = new Vector2(0, 1f);
        displacement = AdjustByAngle(displacement, 45, 1f);
       
        Vector3 wanderForce = velocity + displacement;
    
        return wanderForce;
    }
    

   

    

   

   

   
}
