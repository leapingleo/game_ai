using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Character
{
    public bool turned = false;
    public bool followMouse;
    public State state;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        //WALL FOLLOWING IMPLEMENTATION line 20 ~ line 37
        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.41f;
        Vector3 endVisionAt = detectVisionStartAt + transform.up;
        RaycastHit2D frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, 1f);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);

        if (frontVision && frontVision.transform.CompareTag("Obstacle"))
        {
            Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.6f;
            //nextWallFollow.transform.position = targetTurningPoint;
            wallFollow = targetTurningPoint;
            nextTarget = targetTurningPoint;
        }
        else
        {
            if (Vector2.Distance(transform.position, nextTarget) < 0.02f)
            {
                nextTarget = storeTarget;
            }
        }

        if (state == State.TAKE_ROLL)
        {
            
            TakeRoll();
            setNewDest = true;
            state = State.EXIT;
        }

        if (state == State.LEAD)
        {
            path = GetComponent<TestMove>().GetPath();
            float distToDest = Vector2.Distance(transform.position, GetComponent<TestMove>().FinalTarget());

            ApplyForce(Seek(nextTarget, slowDownRadius, distToDest));
            UpdatePath();
            UpdateMovement();
            LookingForRolls();
          //  if (availableRolls.Count > 0 && availableRolls[0] != null)
          //      state = State.TAKE_ROLL;
        }

        if (state == State.EXIT)
        {
          //  SetNewDestination(new Vector3(17.5f, -5.5f, 0));
        }

        //reroute when something is blocked at next target pos and when not reached at the target pos yet
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(nextTarget, 0.5f);
        if (hitColliders.Length > 0 && Vector2.Distance(transform.position, nextTarget) > 0.03f)
            SetNewDestination(destination.position);
    }

    public bool ReachedTarget()
    {
        return Vector2.Distance(transform.position, destination.position) < 0.05f;
    }

    
}