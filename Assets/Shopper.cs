using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopper : MonoBehaviour
{
    public enum State { FOLLOW, EXIT, TAKE_ROLL };
    protected Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }
    public float visionDistance;
    public float maxSpeed;
    public float maxForce;
    public GameObject nextWallFollow;
    public Vector2 velocity;
    public float visionAngle;
    protected Vector2 acceleration;
    protected Camera cam;
    protected Vector2 target;
    public float slowDownRadius = 1f;

    protected List<Transform> nearbyMember;
    protected Vector2 finalTarget;
    protected Vector2 slowdownFace;

    protected RaycastHit2D frontVision;

    protected List<Vector2> path;
    protected Vector2 nextTarget;
    protected List<GameObject> availableRolls;

    public virtual void Start()
    {
        acceleration = Vector3.zero;
      //  velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        agentCollider = GetComponent<Collider2D>();
        cam = Camera.main;
    }

    // protected virtual void Update()
    // {


    // }



    protected void AvoidObstacle(string type, float strength)
    {
        if (!frontVision)
            return;

        Vector2 avoid = Vector2.zero;

        if (frontVision.transform.tag == type)
        {
            if (frontVision.transform.tag == "Leader")
                Debug.Log("HIt leader");

            avoid = ((Vector2)transform.position - (Vector2)frontVision.transform.position);
            avoid = avoid.normalized * maxSpeed;
            Vector2 steer = avoid - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce * strength);

            ApplyForce(steer * strength);
        }
    }

    protected void UpdateMovement()
    {
        if (velocity.magnitude > 0.01f && velocity.magnitude < 0.03f)
        {
            slowdownFace = velocity;
        }

       // if (velocity.magnitude <= 0f)
       // {
          //  transform.up = slowdownFace;
       // }else


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
    protected void UpdatePath()
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


    protected Vector2 Seek(Vector2 target, float r)
    {
        Vector2 desired = target - (Vector2)transform.position;
        float d = desired.magnitude;
        desired = desired.normalized;
        //slow down within some radius
        // if (d < r)
        // {
        //     float mappedSpeed = Mathf.Lerp(0f, maxSpeed, d);
        //     desired *= d / r * maxSpeed;
        //  }
        //  else
        //  {
        desired *= maxSpeed;
        // }

        Vector2 steer = desired - velocity;
        Vector2 steerClamp = Vector3.ClampMagnitude(steer, maxForce);

        return steerClamp;
    }

    //calculates the steering force towards a target
    protected Vector2 Seek(Vector2 target, float r, float dist)
    {
        Vector2 desired = target - (Vector2)transform.position;
        float d = desired.magnitude;
        desired = desired.normalized;
        //slow down within some radius
        if (dist < r)
        {
            float mappedSpeed = Mathf.Lerp(0f, maxSpeed, d);
            desired *= d / r * maxSpeed;

           
        }
        else
        {
            desired *= maxSpeed;
        }

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
