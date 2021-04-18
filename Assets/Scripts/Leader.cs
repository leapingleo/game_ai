using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Leader : Character
{
    public bool turned = false;
    public bool followMouse;
    // public State state;
    private Dictionary<Vector3, bool> shelfLocationKey = new Dictionary<Vector3, bool>();
    int searchCounter = 0;
    public GameObject shelfLocationManager;
    public GameObject exitManager;
    int currentVisitIndex;
    int desiredNumberOfRolls;
    private Vector3 exitLocation;
    private bool securityDetected = false;
    private Vector3 securityPosition = Vector2.zero;
    public Sprite[] characterSprites;


    public override void Start()
    {
        GetComponent<SpriteRenderer>().sprite = characterSprites[Random.Range(0, characterSprites.Length)];
        //storeTarget = destination.position;
        base.Start();
        SetupShelfLocations();
        desiredNumberOfRolls = 4;
        Debug.Log(desiredNumberOfRolls);
        //choose a random shelf 
        currentVisitIndex = RandomShelfIndex();
        SetNewDestination(shelfLocationKey.ElementAt(currentVisitIndex).Key);
        exitLocation = exitManager.transform.GetChild(Random.Range(0, exitManager.transform.childCount)).transform.position;
        maxSpeed = Random.Range(1f, 4f);
        maxForce = Random.Range(0.06f, 0.2f);
    }

    private void SetupShelfLocations()
    {
        for (int i = 0; i < shelfLocationManager.transform.childCount; i++)
        {
            Vector3 shelfLocation = shelfLocationManager.transform.GetChild(i).transform.position;
            //arrive point should be below the shelf
            Vector3 arriveAtShelfLocation = new Vector3(shelfLocation.x, shelfLocation.y - 1.5f, shelfLocation.z);
            shelfLocationKey.Add(arriveAtShelfLocation, false);
        }
    }

    //return a shelf index that hasnt been visited
    private int RandomShelfIndex()
    {
        int index = Random.Range(0, shelfLocationManager.transform.childCount);

        // bool isVisited = shelfLocationKey[shelfLocationKey.ElementAt(index).Key];

        while (shelfLocationKey[shelfLocationKey.ElementAt(index).Key])
        {
            index = Random.Range(0, shelfLocationManager.transform.childCount);
        }
        return index;
    }

    private void Update()
    {
        //WALL FOLLOWING IMPLEMENTATION line 20 ~ line 37
        //********hard coded raycast pos right now
        Vector3 detectVisionStartAt = transform.position + transform.up * 0.5f;
        Vector3 endVisionAt = detectVisionStartAt + transform.up;
        RaycastHit2D frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up * 0.5f, visionDistance);
        Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);
        float d = Vector2.Distance(transform.position, storeTarget);

        //wall following when not close to the final target
        if (frontVision && (frontVision.collider.CompareTag("Obstacle") || frontVision.collider.CompareTag("Shelf")) && d > 0.5f)
        {
            Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.6f;
            //nextWallFollow.transform.position = targetTurningPoint;
            nextTarget = targetTurningPoint;
        }
        else
        {
            if (Vector2.Distance(transform.position, nextTarget) < 0.02f)
            {
                nextTarget = storeTarget;
            }
        }

        detectSecurities();

        if (state == State.IDLE)
        {
            if (currentRollsOnHand < desiredNumberOfRolls)
                state = State.SEARCH_ROLL;
        }

        else if (state == State.SEARCH_ROLL)
        {
            //when there are more than one rolls on hand, and security detected
            if (securityDetected && currentRollsOnHand > 0)
            {
                state = State.FLEE;
                return;
            }
            //move to next shelf
            float distToShelf = Vector2.Distance(transform.position, shelfLocationKey.ElementAt(currentVisitIndex).Key);
            MoveTowardsTarget(nextTarget, distToShelf);

            //when arrived at the shelf
            if (distToShelf <= 0.1f)
            {
                //mark current shelf as visited when arrived at the current shelf
                shelfLocationKey[shelfLocationKey.ElementAt(currentVisitIndex).Key] = true;
                //this variable just to make everyone fetch one roll per shelf
                canFetchOneRollFromShelf = true;
                state = State.FETCH_ROLL;
            }
        }

        else if (state == State.FETCH_ROLL)
        {
            SearchShelf();

            //leave the shop as soon as desired amount of rolls are collected
            if (currentRollsOnHand >= desiredNumberOfRolls)
            {
                state = State.EXIT;
                SetNewDestination(exitLocation);
                return;
            }
            //back to search state when there are more rolls to get and not all shelves have been checked
            if (currentRollsOnHand < desiredNumberOfRolls && !CheckAllShelvesVisited())
            {
                state = State.SEARCH_ROLL;
                VisitNextShelf();
            }
            else //else need more rolls and all shelves checked, so search for a stealing target
            {
                state = State.SEARCH_STEAL_TARGET;
            }
        }
        else if (state == State.SEARCH_STEAL_TARGET)
        {
            targetToStealFrom = StealTargetNearby();

            //  float distToStealTarget = Vector2.Distance(transform.position, new Vector3(-0.5f, -5f, 0)
            //if the target is lost either taken by the security or left the store, then just leave the store
            if (targetToStealFrom == null)
            {
                state = State.EXIT;
                SetNewDestination(exitLocation);
               
                // if (targetToStealFrom.gameObject != null)
                //  {
                // nextTarget = targetToStealFrom.position;
               // storeTarget = targetToStealFrom.position;
                //  }
            }
            else
            {
                state = State.STEAL;
                SetNewDestination(storeTarget);
            }
        }
        else if (state == State.STEAL)
        {
            //  Debug.Log("target to steal " + targetToStealFrom.transform.name + ", " + targetToStealFrom.childCount);
            // when the aiming target's roll is stolen by another customer
            if (targetToStealFrom != null && targetToStealFrom.childCount < 1)
                targetToStealFrom = null;

            //if a stealable target is removed by the security or the target no longer has a roll, go back to search state
            if (targetToStealFrom == null || targetToStealFrom.childCount < 1)
            {
                state = State.SEARCH_STEAL_TARGET;
                return;
            }
            else
            {
                float dist = Vector2.Distance(transform.position, targetToStealFrom.position);
                MoveTowardsTarget(nextTarget, dist);

                //this creats a dash effect within 3 unit of radius
                if (dist < 3f)
                {
                    MoveTowardsTarget(storeTarget, dist);
                }
                if (dist < 1f)
                {
                    //at this point still check whether "targetToStealFrom" has left the store or "deleted" by the securities
                    if (targetToStealFrom == null || targetToStealFrom.childCount < 1)
                    {
                        state = State.SEARCH_STEAL_TARGET;
                    }
                    else
                    {
                        StealTarget();
                    }
                }
            }
            //whether a steal is successful or not just leave the store
            if (trySteal)
            {
                state = State.EXIT;
                SetNewDestination(exitLocation);
            }
        }
        else if (state == State.EXIT)
        {
            //still check for security on the way out
            if (securityDetected)
            {
                state = State.FLEE;
                return;
            }

            float distToExit = Vector2.Distance(transform.position, exitLocation);
            MoveTowardsTarget(nextTarget, distToExit);

            //remove once reached the exit
            if (distToExit < 0.2f)
                Destroy(gameObject);
        }
        else if (state == State.FLEE)
        {
            if (securityDetected)
                flee();
            else
            {
                state = State.EXIT;
                SetNewDestination(exitLocation);
            }
        }
    }

    private void VisitNextShelf()
    {
        //choose a random shelf to visit next
        currentVisitIndex = RandomShelfIndex();
        SetNewDestination(shelfLocationKey.ElementAt(currentVisitIndex).Key);
    }

    private bool CheckAllShelvesVisited()
    {
        foreach (var visited in shelfLocationKey.Values)
        {
            if (!visited)
                return false;
        }
        return true;
    }

    private void detectSecurities()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 3f, LayerMask.GetMask("Security"));

        if (hitColliders.Length == 0)
        {
            securityDetected = false;
            return;
        }

        GameObject gameObj = hitColliders[0].gameObject;

        securityDetected = true;
        securityPosition = gameObj.transform.position;
    }

    private void flee()
    {
        // Vector3 desireVelocity = (transform.position - securityPosition).normalized * maxSpeed;
        // Vector2 steer = (Vector2)desireVelocity - velocity;
        // steer = Vector3.ClampMagnitude(steer, maxForce * 2);
        // ApplyForce(steer);
        // UpdateMovement();

        Vector2 target = (Vector2)(transform.position - securityPosition);
        float dist = Vector2.Distance(transform.position, target);
        MoveTowardsTarget(target, dist);
    }
}