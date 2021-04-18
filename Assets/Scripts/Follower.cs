using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : Character
{
    public Vector3 followDistance;
    private bool touchingWall = false;
   // private RaycastHit2D frontVision;
    public float wanderRaidus = 3f;

	void Update()
    {
        Vector2 followLeaderAt = groupManager.Leader.transform.position - groupManager.Leader.transform.up * 1.5f;

        Vector2 targetInRadius = groupManager.Leader.transform.position;

        float d = Vector2.Distance(transform.position, followLeaderAt);
        
        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.5f;
        Vector3 endVisionAt = detectVisionStartAt + transform.up * visionDistance;
        frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up * 0.5f, visionDistance);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);

        float distToLeader = Vector2.Distance(transform.position, targetInRadius);

        //wall following by single front vision
        if (frontVision.collider != null && frontVision.collider.tag == "Obstacle")
        {
            maxSpeed = 0.5f;
            touchingWall = true;
            Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.6f;
            //nextWallFollow.transform.position = targetTurningPoint;
            GameObject marker = Instantiate(wallFollowMarker, targetTurningPoint, Quaternion.identity);
            Destroy(marker, 0.5f);

            wallFollow = targetTurningPoint;
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
            trySteal = false;
            canFetchOneRollFromShelf = true;
            FollowLeader(distToLeader, target);
        }


        if (state == State.FETCH_ROLL)
        {
            SearchShelf();
          //  if (availableRolls.Count > 0 && availableRolls[0] != null)
          //      TakeRoll();
          //state = State.EXIT;
          //  state = State.FOLLOW;
        }
        if (state == State.SEARCH_STEAL_TARGET)
        {
            if (StealTargetNearby() != null)
            {
                state = State.STEAL;
                targetToStealFrom = StealTargetNearby();

                SetNewDestination(targetToStealFrom.position);
            }
        }
        if (state == State.STEAL)
        {
            targetToStealFrom = StealTargetNearby();
            Debug.Log("tar " + targetToStealFrom.name);
            

            float dist = Vector2.Distance(transform.position, targetToStealFrom.position);
            MoveTowardsTarget(targetToStealFrom.position, dist);

            if (dist < 3f)
            {
                MoveTowardsTarget(targetToStealFrom.position, dist);
            }
            if (dist < 1f)
            {
                StealTarget();
            }
            
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
                Avoidance(groupManager.GetGroupMembers(), 2f);
                Cohesion(groupManager.GetGroupMembers(), 2f);
                Alignment(groupManager.GetGroupMembers(), 1f);
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
            Avoidance(groupManager.GetGroupMembers(), 1f);
            if (touchingWall)
            {
                targetForce = Seek(targetAt, slowDownRadius, d);
                ApplyForce(targetForce);
            }
        }
       // AvoidObstacle("", 1f);
        UpdateMovement();
    }

    void Cohesion(List<Transform> nearbyMember, float factor)
    {
        Vector2 cohesion = Vector2.zero;
        int nNearby = 0;
        foreach (Transform neighbour in nearbyMember)
        {
            if (!neighbour.GetComponent<Character>().HasRoll) {
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
                velocity += neighbour.GetComponent<Character>().velocity;
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
