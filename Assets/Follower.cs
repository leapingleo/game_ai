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

    // Update is called once per frame
    void Update()
    {
        Vector2 followLeaderAt = leader.transform.position - leader.transform.up * 1.5f;
        //use squared magintude to save some performance cost
        //  float d = Vector2.SqrMagnitude((Vector2)transform.position - followLeaderAt);
        float d = Vector2.Distance(transform.position, followLeaderAt);
        Vector2 avoidance = Avoidance(nearbyMember);

        //dont want every follower to arrive at exact point behind the leader
        //as long as they are within the following radius
        if (d > followLeaderRadius)
        {
            ApplyForce(avoidance);
            ApplyForce(FollowLeader(followLeaderAt));
            maxForce = 0.08f;
            maxSpeed = 5f;
        }
        //when follower arrive behind the leader
        else
        {
            ApplyForce(Wander(velocity) * 0.05f);
            ApplyForce(avoidance);
            maxForce = 0.01f;
            maxSpeed = 1f;
        }
        UpdateMovement();
    }

    Vector2 Wander(Vector3 center)
    {
        Vector3 displacement = new Vector2(0, 1f);
        displacement = AdjustByAngle(displacement, wanderAngle, 0.1f);
        wanderAngle += UnityEngine.Random.Range(0.01f, 0.08f);
        Vector3 wanderForce = center + displacement;
        Debug.DrawLine(transform.position, transform.position + wanderForce);
        return wanderForce;
    }

    
   

    Vector2 FollowLeader(Vector3 target)
    {
        return Seek(target, 3f);
    }

    Vector2 Avoidance(List<Transform> nearbyMember)
    {
        if (nearbyMember == null)
        {
            return Vector2.zero;
        }

        Vector2 avoidance = Vector2.zero;
        int nNearby = 0;

        foreach (Transform neighbour in nearbyMember)
        {
            if (Vector2.Distance(transform.position, neighbour.position) < 1.2f)
            {
                avoidance += (Vector2)(transform.position - neighbour.position);
                nNearby++;
            }
        }
       
        if (nNearby > 0)
        {
            avoidance /= nNearby;
         
         //   avoidance = Vector2.Lerp(velocity, avoidance, 0.3f);
            //   avoidance = avoidance.normalized * maxSpeed;
            //  avoidance -= velocity;
             //   avoidance = Vector3.ClampMagnitude(avoidance, 0.2f);
           //    Debug.Log(avoidance);
        }
        avoidance = avoidance.normalized;
        return avoidance;
    }
}
