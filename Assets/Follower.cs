using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : Shopper
{
    public Vector3 followDistance;
    public GameObject leader;
    public float followLeaderRadius;
    private Vector3 wanderDisplacementCenter;
    private float wanderAngle;
    private float x = 3f;
    private bool touchingWall = false;
    private RaycastHit2D frontVision;
    public State state;

    public override void Start()
    {
        base.Start();
      //  state = State.FOLLOW;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 followLeaderAt = leader.transform.position - leader.transform.up * 1.5f;
        //  Vector2 targetInRadius = (Vector2)transform.position + (Vector2)( leader.transform.position - transform.position).normalized * (Vector2.Distance(transform.position, leader.transform.position) - 1.2f + 0.1f);
        Vector2 targetInRadius = leader.transform.position;

        //use squared magintude to save some performance cost
        //  float d = Vector2.SqrMagnitude((Vector2)transform.position - followLeaderAt);
        float d = Vector2.Distance(transform.position, followLeaderAt);
      //  Vector2 avoidance = Avoidance(nearbyMember);
        
        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.41f;
        // Vector3 endVisionAt = detectVisionStartAt + transform.up;
        Vector3 endVisionAt = detectVisionStartAt + transform.up * visionDistance;
        frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, visionDistance);
        //  RaycastHit2D rightVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, -25) * transform.up, 0.6f);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);

        float distToDest = Vector2.Distance(transform.position, targetInRadius);

        //dont want every follower to arrive at exact point behind the leader
        //as long as they are within the following radius
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
            FollowLeader(distToDest, target);
            LookingForRolls();
            if (availableRolls.Count > 0)
                state = State.TAKE_ROLL;
        }

        if (state == State.EXIT)
        {
            if (GetComponent<TestMove>().enabled == false)
                GetComponent<TestMove>().enabled = true;

            float dist = Vector2.Distance(transform.position, GetComponent<TestMove>().FinalTarget());

            path = GetComponent<TestMove>().GetPath();
            UpdatePath();
            ApplyForce(Seek(nextTarget, 1f, dist));
        }

        if (state == State.TAKE_ROLL)
        {
            availableRolls[0].transform.parent = transform;
            availableRolls[0].transform.position = transform.position;
            state = State.EXIT;
        }

        UpdateMovement();
    }

    void LookingForRolls()
    {
        availableRolls = new List<GameObject>();
        Collider2D[] neighboursColliders = Physics2D.OverlapCircleAll(transform.position, 2f);

        foreach (Collider2D c in neighboursColliders)
        {
            if (c.CompareTag("Roll") && c.transform.parent == null)
            {
                availableRolls.Add(c.transform.gameObject);
            }
        }
    }

    void FollowLeader(float d, Vector3 targetAt)
    {
        Vector2 targetForce;
        if (d > 3f)
        {
            maxSpeed = 4f;
            maxForce = 0.08f;

            if (!touchingWall)
            {
                targetForce = Seek(targetAt, 1f, d);
                ApplyForce(targetForce * 1f);
                Avoidance(nearbyMember, 2f);
                Cohesion(nearbyMember, 2f);
                Alignment(nearbyMember, 1f);
            }
            else
            {
                targetForce = Seek(targetAt, 1f, d) * 3f;
                ApplyForce(targetForce * 3f);
                Cohesion(nearbyMember, 1f);
            }

        }
        else
        {
            maxSpeed = 2f;
            maxForce = 0.04f;
            Avoidance(nearbyMember, 1f);
            if (touchingWall)
            {
                targetForce = Seek(targetAt, 1f, d);
                ApplyForce(targetForce);
            }
        }
    }

    void Avoidance(List<Transform> nearbyMember, float strength)
    {

        Vector2 avoidance = Vector2.zero;
        int nNearby = 0;
        foreach (Transform neighbour in nearbyMember)
        {
            float d = Vector2.Distance(transform.position, neighbour.position);
            if (d > 0f && d < 1f)
            {
                Vector2 diff = (Vector2)(transform.position - neighbour.position);
                diff = diff.normalized;
                diff /= d;
                avoidance += diff;
                nNearby++;
            }
        }
        if (nNearby > 0)
        {
            avoidance /= nNearby;
            avoidance = avoidance.normalized * maxSpeed;

            Vector2 steer = avoidance - velocity;
            avoidance = Vector3.ClampMagnitude(steer, maxForce * strength);
            ApplyForce(avoidance * strength);

            //avoidance = Vector2.Lerp(velocity, avoidance, 0.3f);
        }
    }

    void Cohesion(List<Transform> nearbyMember, float factor)
    {
        Vector2 cohesion = Vector2.zero;
        int nNearby = 0;
        foreach (Transform neighbour in nearbyMember)
        {
            float d = Vector2.Distance(transform.position, neighbour.position);
            if (d > 0f)
            {
                cohesion += (Vector2)neighbour.position;
                nNearby++;
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
