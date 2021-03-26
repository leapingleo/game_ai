using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopper : MonoBehaviour
{
    protected Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }
    public float maxSpeed;
    public float maxForce;
    public Vector2 velocity;
    protected Vector2 acceleration;
    protected Camera cam;
    protected Vector2 target;
    public float slowDownRadius = 1f;

    protected List<Transform> nearbyMember;

    public virtual void Start()
    {
        acceleration = Vector3.zero;
        velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        agentCollider = GetComponent<Collider2D>();
        cam = Camera.main;
    }

   // protected virtual void Update()
   // {
        
        
   // }

    protected void UpdateMovement()
    {
        transform.up = velocity;
        velocity += acceleration;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += (Vector3)velocity * Time.deltaTime;

        //reset acceleration each call
        acceleration *= 0;

    }

    protected void ApplyForce(Vector2 force)
    {
        acceleration += force;
    }

    //calculates the steering force towards a target
    protected Vector2 Seek(Vector2 target, float r)
    {
        Vector2 desired = target - (Vector2)transform.position;
        float d = desired.magnitude;
        desired = desired.normalized;
        //slow down within some radius
      //  if (d < r)
      //  {
          //  float mappedSpeed = Mathf.Lerp(0f, maxSpeed, d);
       //     desired *= d / r * maxSpeed;
       // }
       // else
       // {
            desired *= maxSpeed;
        //}

        Vector2 steer = desired - velocity;
        Vector2 steerClamp = Vector3.ClampMagnitude(steer, maxForce);

        return steerClamp;
    }

   

    public void SetNearbyMembers(List<Transform> neighbours)
    {
        nearbyMember = neighbours;
    }

    protected Vector2 AdjustByAngle(Vector2 v, float delta, float r)
    {
        return new Vector2(Mathf.Cos(delta) * r, Mathf.Sin(delta) * r);
    }


}
