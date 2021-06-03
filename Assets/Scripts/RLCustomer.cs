using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class RLCustomer : Agent
{
    public GameObject entranceManager;
    public GameObject exitManager;
    public GameObject shelfLocationManager;
    public GameObject paperRollPrefab;
    public GameObject emptyShelfPrefab_0;
    // public float sight = 3.0f;
    public float maxSpeed = 2.6f;
    public float stealRange = 1.5f;
    public float fetchRange = 0.1f;
    public float exitRange = 0.2f;
    public float caughtRange = 0.2f;
    public float rollReward = 0.5f;
    public float reachShelfReward = 0.5f;
    public float exitReward = 1.0f;
    public float caughtPenalty = -1.0f;
    public float outSideWithoutRollPenalty = -0.5f;
    public float collisionPenalty = -0.1f;
    public float approachExitReward = 0.01f;
    public float awayFromExitPenalty = -0.01f;
    public float approachShelfReward = 0.01f;
    public float awayFromShelfPenalty = -0.01f;
    public float gameDuration = 60;

    private Vector3 spawnLocation;
    private Vector3 exitLocation1;
    private Vector3 exitLocation2;
    private float velocity;
    private Vector3 securityPosition;
    protected int currentRollsOnHand;
    private readonly Vector2 boxMin = new Vector2(-3.5f, -11.5f);
    private readonly Vector2 boxMax = new Vector2(18.5f, 9.5f);
    private float time;
    private float shortestDistanceToExit;
    private float shortestDistanceToShelf;
    private List<GameObject> shelves;

    public override void Initialize()
    {
      //Destroy(gameObject, 30f);
        spawnLocation = entranceManager.transform.GetChild(Random.Range(0, entranceManager.transform.childCount)).position;
        exitLocation1 = exitManager.transform.GetChild(0).transform.position;
        exitLocation2 = exitManager.transform.GetChild(1).transform.position;
        shortestDistanceToExit = float.MaxValue;
        shortestDistanceToShelf = float.MaxValue;
        SetResetParameters();
        InitializeEmptyShelves();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // current position of the AI customer
        sensor.AddObservation(transform.position.x * 0.1f);
        sensor.AddObservation(transform.position.y * 0.1f);
        // number of rolls
        sensor.AddObservation(currentRollsOnHand);

        // 3 shelf positions
        for (int i = 0; i < shelfLocationManager.transform.childCount; i++)
        {
            Vector3 shelfLocation = shelfLocationManager.transform.GetChild(i).transform.position;
            //arrive point should be below the shelf
            Vector3 arriveAtShelfLocation = new Vector3(shelfLocation.x, shelfLocation.y - 1.5f, shelfLocation.z);

            // present the distance as the reverse of the distance
            Vector3 distanceVector = (arriveAtShelfLocation - transform.position);
            Vector2 presentedPosition = GetPresentedPosition(distanceVector);
            sensor.AddObservation(presentedPosition.x);
            sensor.AddObservation(presentedPosition.y);
        }

        // two exit locations
        Vector3 exit1Distance = (exitLocation1 - transform.position);
        Vector2 exit1PresentedPosition = GetPresentedPosition(exit1Distance);
        sensor.AddObservation(exit1PresentedPosition.x);
        sensor.AddObservation(exit1PresentedPosition.y);

        Vector3 exit2Distance = (exitLocation2 - transform.position);
        Vector2 exit2PresentedPosition = GetPresentedPosition(exit2Distance);
        sensor.AddObservation(exit2PresentedPosition.x);
        sensor.AddObservation(exit2PresentedPosition.y);

        // total 13 observations (3 + 3 x 2 + 4)
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // movement
        var x = actionBuffers.ContinuousActions[0];
        var y = actionBuffers.ContinuousActions[1];
        Vector3 acceleration = Vector2.ClampMagnitude(new Vector2(x, y), maxSpeed);
        velocity += acceleration.magnitude;
        transform.position += acceleration * Time.deltaTime;

        // fetch roll
        Fetch();
        
        
        // steal
        // Steal();

        // out side of the shop without roll penalty
        // if (!IsInsideBox(transform.position))
        // {
        //     AddReward(outSideWithoutRollPenalty);
        // }
        
        float distToExit1 = Vector2.Distance(transform.position, exitLocation1);
        float distToExit2 = Vector2.Distance(transform.position, exitLocation2);
        float shortestDistance = distToExit1 < distToExit2 ? distToExit1 : distToExit2;
        if (currentRollsOnHand > 0)
        {
            if (shortestDistance < shortestDistanceToExit)
            {
                shortestDistanceToExit = shortestDistance;
                AddReward(approachExitReward);
            }
            else
            {
                AddReward(awayFromExitPenalty);
            }
        }
        else
        {
            float shortestDist = float.MaxValue;
            // 3 shelf positions
            for (int i = 0; i < shelves.Count; i++)
            {
                GameObject shelf = shelves[i];
                if (shelf.transform.childCount == 0)
                {
                    continue;
                }
                Vector3 shelfLocation = shelf.transform.position;
                //arrive point should be below the shelf
                Vector3 arriveAtShelfLocation = new Vector3(shelfLocation.x, shelfLocation.y - 1.5f, shelfLocation.z);
        
                // present the distance as the reverse of the distance
                float distance = (arriveAtShelfLocation - transform.position).magnitude;
                if (distance < shortestDist)
                {
                    shortestDist = distance;
                }
            }
            
                
            if (shortestDist < shortestDistanceToShelf)
            {
                AddReward(approachShelfReward);
            }
            else
            {
                AddReward(awayFromShelfPenalty);
            }
            shortestDistanceToShelf = shortestDist;
        }
        // succeed
        if (shortestDistance < exitRange && currentRollsOnHand > 0)
        {
            // Destroy(gameObject);
            AddReward(exitReward);
            EndEpisode();
        }
        
        // caught
        float distToSecurity = Vector2.Distance(transform.position, securityPosition);
        if (distToSecurity < caughtRange)
        {
            // Destroy(gameObject);
            SetReward(caughtPenalty);
            EndEpisode();
        }
    }
    
    // private void OnCollisionEnter(Collision collision)
    // {
    //     // Collided with the area boundary, give a negative reward
    //     AddReward(-.5f);
    // }
    
    public override void OnEpisodeBegin()
    {
        SetResetParameters();
    }

    public void SetResetParameters()
    {
        transform.position = spawnLocation;
        currentRollsOnHand = 0;
        velocity = 0;
        securityPosition = Vector3.zero;
        time = gameDuration;

        if (transform.childCount > 0 && transform.GetChild(0).gameObject != null)
        {
            GameObject roll = transform.GetChild(0).gameObject;
            Destroy(roll);
        }
    }

    private void Fetch()
    {
        // can have no more than one roll
        if (currentRollsOnHand > 0)
        {
            return;
        }
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, fetchRange);
        foreach (var c in hitColliders)
        {
            if (c.transform.CompareTag("Shelf"))
            {
                // train the first shelf
                if (Mathf.Approximately(c.transform.position.x, shelfLocationManager.transform.GetChild(0).transform.position.x) &&
                    Mathf.Approximately(c.transform.position.y, shelfLocationManager.transform.GetChild(0).transform.position.y))
                {
                    Debug.Log("shelf 0 reached");
                    // return;
                }
                if (c.transform.childCount > 0)
                {
                    // don't reduce the amount of rolls otherwise the rolls will be out of stock quickly
                    // GameObject roll = Instantiate(paperRollPrefab, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.Euler(Vector3.zero));
                    // roll.transform.parent = transform;
                    
                    currentRollsOnHand++;
                    AddReward(rollReward);
                    GameObject roll = c.transform.GetChild(0).gameObject;
                    roll.transform.parent = transform;
                    roll.transform.position = transform.position;
                }
            }
        }
    }
    
    private void Steal()
    {
        // can have no more than one roll
        if (currentRollsOnHand > 0)
        {
            return;
        }
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, stealRange);
        foreach (var c in hitColliders)
        {
            if (c.CompareTag("AICustomer") || c.CompareTag("Follower"))
            {
                if (c.transform.childCount > 0 && c.transform.GetChild(0).gameObject != null)
                {
                    GameObject roll = c.transform.GetChild(0).gameObject;
                    roll.GetComponent<SpriteRenderer>().color = Color.red;
                    roll.layer = 10;
                    roll.transform.parent = transform;
                    roll.transform.position = transform.position;
                    currentRollsOnHand++;

                    c.GetComponent<RLCustomer>().currentRollsOnHand--;
                }
            }
        }
    }

    private Vector2 GetPresentedPosition(Vector3 distance)
    {
        Vector3 normalizedPosition = distance.normalized;
        float d = distance.magnitude;
        float k = 0.3f;
        double ekd = Math.Pow(Math.E, -k * d);
        float presentedX = (float)ekd * normalizedPosition.x;
        float presentedY = (float)ekd * normalizedPosition.y;
        
        return new Vector2(presentedX, presentedY);
    }
    
    private bool IsInsideBox(Vector2 point)
    {
        float x = point.x;
        float y = point.y;
        return (x >= boxMin.x) && (x <= boxMax.x) &&
               (y >= boxMin.y) && (y <= boxMax.y);
    }

    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     AddReward(collisionPenalty);
    // }

    public void Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            EndEpisode();
            Initialize();
        }
    }
    
    private void InitializeEmptyShelves()
    {
        if (shelves != null)
        {
            for (int i = 0; i < shelves.Count; i++)
            {
                GameObject go = shelves[i];
                Destroy(go);
            }
        }
        
        shelves = new List<GameObject>();
        
        for (int i = 0; i < shelfLocationManager.transform.childCount; i++)
        {
            GameObject go = Instantiate(emptyShelfPrefab_0, shelfLocationManager.transform.GetChild(i).transform.position,
                Quaternion.Euler(Vector3.zero));
            go.name = "Empty Shelf " + i;
            shelves.Add(go);
        }
    }

    private bool AllShelvesEmpty()
    {
        if (shelves == null)
        {
            return true;
        }
        
        for (int i = 0; i < shelves.Count; i++)
        {
            GameObject go = shelves[i];

            if (go.transform.childCount > 0)
            {
                return false;
            }
        }

        return true;
    }
}
