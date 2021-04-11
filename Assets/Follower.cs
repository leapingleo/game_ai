using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : Shopper
{
    public Vector3 followDistance;
    public GameObject leader;
    private bool touchingWall = false;
   // private RaycastHit2D frontVision;
    public State state;
    public float wanderRaidus = 3f;

    public override void Start()
    {
        base.Start();
      //  state = State.FOLLOW;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 followLeaderAt = leader.transform.position - leader.transform.up * 1.5f;
        //  Vector2 targetInRadius = (Vector2)transform.position 
        //                              + (Vector2)( leader.transform.position - transform.position).normalized 
        //* (Vector2.Distance(transform.position, leader.transform.position) - 1.2f + 0.1f);
        Vector2 targetInRadius = leader.transform.position;

        float d = Vector2.Distance(transform.position, followLeaderAt);
        
        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.41f;
        Vector3 endVisionAt = detectVisionStartAt + transform.up * visionDistance;
        frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, visionDistance);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);

        float distToLeader = Vector2.Distance(transform.position, targetInRadius);

        //wall following by single front vision
        if (frontVision && frontVision.transform.CompareTag("Obstacle"))
        {
            touchingWall = true;
            Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.6f;
            nextWallFollow.transform.position = targetTurningPoint;
            target = targetTurningPoint;
        }
        else
        {
            if (Vector2.Distance(transform.position, target) < 0.02f)
            {
                target = targetInRadius;
                touchingWall = false;
            }
        }

        if (state == State.FOLLOW)
        {
            FollowLeader(distToLeader, target);
            LookingForRolls();
            if (availableRolls.Count > 0 && availableRolls[0] != null)
                state = State.TAKE_ROLL;
        }

        if (state == State.EXIT)
        {
            SetNewDestination(new Vector3(17.5f, -5.5f, 0));
        }

        if (state == State.TAKE_ROLL)
        {
            TakeRoll();
            state = State.EXIT;
        }
    }

    

    void FollowLeader(float d, Vector3 targetAt)
    {
        Vector2 targetForce;
        if (d > wanderRaidus)
        {
            maxSpeed = 3f;
            maxForce = 0.08f;

            if (!touchingWall)
            {
                targetForce = Seek(targetAt, slowDownRadius, d);
                ApplyForce(targetForce * 1f);
                Avoidance(nearbyMember, 2f);
                Cohesion(nearbyMember, 3f);
                Alignment(nearbyMember, 1f);
            }
            else
            {
                targetForce = Seek(targetAt, slowDownRadius, d) * 3f;
                ApplyForce(targetForce * 3f);
              //  Cohesion(nearbyMember, 1f);
            }
        }
        else
        {
            maxSpeed = 1f;
            maxForce = 0.04f;
            Avoidance(nearbyMember, 1f);
            //if (touchingWall)
            //{
            //    targetForce = Seek(targetAt, slowDownRadius, d);
            //    ApplyForce(targetForce);
            //}
        }
        AvoidObstacle("", 1f);
        UpdateMovement();
    }

    

    void Cohesion(List<Transform> nearbyMember, float factor)
    {
        Vector2 cohesion = Vector2.zero;
        int nNearby = 0;
        foreach (Transform neighbour in nearbyMember)
        {
            if (!neighbour.GetComponent<Shopper>().HasRoll) {
                float d = Vector2.Distance(transform.position, neighbour.position);
                if (d > 0f )
                {
                    cohesion += (Vector2)neighbour.position;
                    nNearby++;
                }
            }
        }
        if (nNearby > 0)
        {
            cohesion /= nNearby;

            Vector2 desired = cohesion - (Vector2)transform.position;
            desired = desired.normalized * maxSpeed;

            Vector2 steer = desired - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            ApplyForce(steer * factor);
        }
        // cohesion = Vector2.Lerp(velocity, cohesion, agentSmoothTime);
    }

    void Alignment(List<Transform> nearbyMember, float factor)
    {
        Vector2 velocity = Vector2.zero;
        int nNearby = 0;

        foreach (Transform neighbour in nearbyMember)
        {
            float d = Vector2.Distance(transform.position, neighbour.position);
            if (d > 0f && d < 3f)
            {
                velocity += neighbour.GetComponent<Shopper>().velocity;
                nNearby++;
            }
        }
        if (nNearby > 0)
        {
            velocity /= nNearby;
            velocity = velocity.normalized * maxSpeed;
            Vector2 steer = velocity - this.velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            ApplyForce(steer * factor);

        }
        // alignment = Vector2.Lerp(velocity, alignment, agentSmoothTime);
    }
}
