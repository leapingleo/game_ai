using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopper : MonoBehaviour
{
    public enum State { FOLLOW, EXIT, TAKE_ROLL, LEAD };
    public Collider2D Collider { get { return collider; } }
    public float visionDistance;
    public float maxSpeed;
    public float maxForce;
    public float visionAngle;
    public float slowDownRadius = 1f;
    public GameObject nextWallFollow;
    public Transform destination;
    public Vector2 velocity;
   
    protected Collider2D collider;
    protected List<Transform> nearbyMember;
    protected RaycastHit2D frontVision;
    protected List<GameObject> availableRolls;
    protected List<Vector2> path;
    protected Vector2 finalTarget;
    protected Vector2 slowdownFace;
    protected Vector2 nextTarget;
    protected Vector2 acceleration;
    protected Vector2 target;
    protected Vector3 storeTarget;
    protected Camera cam;
    protected bool hasRoll;
    protected bool setNewDest = true;
    public bool HasRoll { get { return hasRoll; } }

    

    public virtual void Start()
    {
        acceleration = Vector3.zero;
      //  velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        collider = GetComponent<Collider2D>();
        cam = Camera.main;
    }

    protected virtual void Update()
    {

    }
    protected void LookingForRolls()
    {
        availableRolls = new List<GameObject>();
        Collider2D[] neighboursColliders = Physics2D.OverlapCircleAll(transform.position, 2f);

        foreach (Collider2D c in neighboursColliders)
        {
            //only rolls add to list when it's not taken
            if (c.CompareTag("Roll") && c.transform.parent == null)
            {
                availableRolls.Add(c.transform.gameObject);
            }
        }
    }

    protected void Avoidance(List<Transform> nearbyMember, float strength)
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

    protected void AvoidObstacle(string type, float strength)
    {
        /**
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
        **/
        Vector2 avoidance = Vector2.zero;
        Collider2D[] neighboursColliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        int nNearby = 0;

        foreach (Collider2D c in neighboursColliders)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d > 0f && d < 1f)
            {
                Vector2 diff = (Vector2)(transform.position - c.transform.position);
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

    protected void TakeRoll()
    {
        availableRolls[0].transform.parent = transform;
        availableRolls[0].transform.position = transform.position;
        availableRolls[0].transform.GetComponent<BoxCollider2D>().enabled = false;
        hasRoll = true;
    }

    protected void SetNewDestination(Vector3 dest)
    {
        if (setNewDest)
        {
            setNewDest = false;
            path = GetComponent<TestMove>().SetNewPath(dest);
            nextTarget = path[path.Count - 1];
        }
        float d = Vector2.Distance(transform.position, dest);
        ApplyForce(Seek(nextTarget, slowDownRadius, d));
        UpdatePath();
        UpdateMovement();
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
        path.Reverse();

        Vector2 arrivingNext = path[0];

        if (Vector2.SqrMagnitude(arrivingNext - (Vector2)transform.position) < 0.05f)
        {
            path.RemoveAt(0);
            nextTarget = path[0];
            storeTarget = nextTarget;
        }
    }

    protected Vector2 Seek(Vector2 target, float r)
    {
        Vector2 desired = target - (Vector2)transform.position;
        float d = desired.magnitude;
        desired = desired.normalized;
        desired *= maxSpeed;

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
            //float mappedSpeed = Mathf.Lerp(0f, maxSpeed, d);
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

    /**
    protected Vector2 AdjustByAngle(Vector2 v, float delta, float r)
    {
        return new Vector2(Mathf.Cos(delta) * r, Mathf.Sin(delta) * r);
    }

    protected Vector2 AvoidObstacle(Vector3 velocity, float angle)
    {
        Vector3 displacement = transform.up;
        displacement = AdjustByAngle(displacement, angle, 1f);

        Vector3 wanderForce = velocity + displacement;

        return wanderForce;
    }
    **/

}
