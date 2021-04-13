using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }
    private Vector2 velocity;
    private Vector2 acceleration;
    public float maxSpeed = 0.8f;
    public float maxForce = 0.2f;
    public float cohesionRadius = 1f;
    private Vector2 currentVelocity;
    public float agentSmoothTime = 0.3f;
    public float avoidanceRadius = 0.5f;

    void Start()
    {
        acceleration = Vector3.zero;
        velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        agentCollider = GetComponent<Collider2D>();
    }

    public void run(List<Transform> neighbours)
    {
        Vector2 cohesion = Cohesion(neighbours);
        Vector2 alignment = Alignment(neighbours);
        Vector2 avoidance = Avoidance(neighbours);
        Debug.Log(avoidance);
        // alignment *= 2f;
        ApplyForce(cohesion);
        ApplyForce(alignment);
        ApplyForce(avoidance);
    }

    //return average nearbys' position and move towards it
    Vector2 Cohesion(List<Transform> neighbours)
    {
        if (neighbours.Count == 0)
        {
            return Vector2.zero;
        }

        Vector2 cohesion = Vector2.zero;

        foreach (Transform neighbour in neighbours)
        {
            cohesion += (Vector2)neighbour.position;
        }
        cohesion /= neighbours.Count;
       // cohesion = Vector2.Lerp(velocity, cohesion, agentSmoothTime);

        return Seek(cohesion);
    }

    Vector2 Alignment(List<Transform> neighbours)
    {
        if (neighbours.Count == 0)
        {
            return Vector2.zero;
        }

        Vector2 alignment = Vector2.zero;

        foreach (Transform neighbour in neighbours)
        {
            alignment += (Vector2)neighbour.gameObject.transform.GetComponent<Agent>().velocity;
        }
        alignment /= neighbours.Count;
      //  alignment = alignment.normalized * maxSpeed;
       // alignment -= velocity;
        alignment = Vector3.ClampMagnitude(alignment, maxForce);

        return alignment;
    }

    Vector2 Avoidance(List<Transform> neighbours)
    {
        if (neighbours.Count == 0)
        {
            return Vector2.zero;
        }

        Vector2 avoidance = Vector2.zero;
        int nNearby = 0;

        foreach (Transform neighbour in neighbours)
        {
            if (Vector2.Distance(transform.position,neighbour.position) < avoidanceRadius)
            {
                avoidance += (Vector2)(transform.position - neighbour.position);
                nNearby++;
            }
        }
        if (nNearby > 0)
        {
            avoidance /= (float)nNearby;
            // cohesion = Vector2.Lerp(velocity, cohesion, 0.3f);
         //   avoidance = avoidance.normalized * maxSpeed;
          //  avoidance -= velocity;
        //    avoidance = Vector3.ClampMagnitude(avoidance, 0.2f);
         //   Debug.Log(avoidance);
        }
        return avoidance;
    }

    public void UpdateMovement()
    {
        transform.up = velocity;
        velocity += acceleration;
        velocity = velocity.normalized;
        transform.position += (Vector3)velocity * Time.deltaTime;
        BoardWarp();
        //reset acceleration each call
        // velocity *= 0;
        acceleration *= 0;
    }

    void ApplyForce(Vector2 force)
    {
        acceleration += force;
    }

    //calculates the steering force towards a target
    Vector2 Seek(Vector2 target)
    {
        Vector2 desired = target - (Vector2)transform.position;
        desired = desired.normalized * maxSpeed;

        Vector2 steer = desired - velocity;
        Vector2 steerClamp = Vector3.ClampMagnitude(steer, maxForce);

        return steerClamp;
    }

    private void BoardWarp()
    {
        float newX = transform.position.x, newY = transform.position.y;

        if (transform.position.x > 8.8)
            newX = -8.8f;
        if (transform.position.x < -8.8)
            newX = 8.8f;

        if (transform.position.y > 4.9)
            newY = -4.9f;
        if (transform.position.y < -4.9)
            newY = 4.9f;


        transform.position = new Vector3(newX, newY, 0);
    }
}
