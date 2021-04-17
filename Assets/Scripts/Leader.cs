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


    public override void Start()
    {
        //storeTarget = destination.position;
        base.Start();
        SetupShelfLocations();
        desiredNumberOfRolls = 4;
        Debug.Log(desiredNumberOfRolls);
        //choose a random shelf 
        currentVisitIndex = RandomShelfIndex();
        SetNewDestination(shelfLocationKey.ElementAt(currentVisitIndex).Key);
        exitLocation = exitManager.transform.GetChild(Random.Range(0, exitManager.transform.childCount)).transform.position;
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
        if (frontVision && (frontVision.collider.CompareTag("Obstacle")) && d > 0.5f)
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

        if (securityDetected)
        {
            if (state == State.EXIT || 
                (state == State.FETCH_ROLL && currentRollsOnHand > 0) ||
                (state == State.SEARCH_ROLL && currentRollsOnHand > 0))
            {
                state = State.FLEE;
            }
        }
        else
        {
            if (state == State.FLEE)
            {
                state = State.EXIT;
            }
        }

        if (state == State.IDLE)
        {
            if (currentRollsOnHand < desiredNumberOfRolls)
                state = State.SEARCH_ROLL;
        }

        if (state == State.SEARCH_ROLL)
        {
            float distToShelf = Vector2.Distance(transform.position, shelfLocationKey.ElementAt(currentVisitIndex).Key);
            MoveTowardsTarget(nextTarget, distToShelf);
            
            //when arrived at the shelf and not all shelves have been visited
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
            else //need more rolls and all shelves checked
            {
                state = State.SEARCH_STEAL_TARGET;
            }
        }
        else if (state == State.SEARCH_STEAL_TARGET)
        {
            //should have wander logic here then if a nearby victim is found then steal
          //  float distToStealTarget = Vector2.Distance(transform.position, new Vector3(-0.5f, -5f, 0)
            if (StealTargetNearby() != null)
            {
                state = State.STEAL;
                targetToStealFrom = StealTargetNearby();

               // if (targetToStealFrom.gameObject != null)
              //  {
                   // nextTarget = targetToStealFrom.position;
                    storeTarget = targetToStealFrom.position;
                    SetNewDestination(targetToStealFrom.position);
              //  }
            } else
            {
                state = State.EXIT;
                SetNewDestination(exitLocation);
            }
        }
        else if (state == State.STEAL)
        {
          //  Debug.Log("target to steal " + targetToStealFrom.transform.name + ", " + targetToStealFrom.childCount);
            //always check if the roll from the victim is taken by another customer
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
                Debug.Log("Next tar " + nextTarget);
             //   if (dist > 3f)
                    MoveTowardsTarget(nextTarget, dist);

                if (dist < 3f)
                {
                    MoveTowardsTarget(storeTarget, dist);
                }
                if (dist < 1f)
                {
                    StealTarget();
                }
            }

         
            if (trySteal)
            {
                state = State.EXIT;
                SetNewDestination(exitLocation);
            } 
        }
        else if (state == State.EXIT)
        {
            float distToExit = Vector2.Distance(transform.position, exitLocation);
            MoveTowardsTarget(nextTarget, distToExit);

            if (distToExit < 0.2f)
                Destroy(gameObject);
        }
        else if (state == State.FLEE)
        {
            flee();
        }
    }

    private void VisitNextShelf()
    {
        //choose a random shelf to visit next
        currentVisitIndex = RandomShelfIndex();
        SetNewDestination(shelfLocationKey.ElementAt(currentVisitIndex).Key);
    }

    

    private int NumberOfStealableNearby()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(nextTarget, 5f);
        int stealableTargets = 0;

        foreach (var c in hitColliders)
        {
            if (c != GetComponent<Collider2D>() && c.CompareTag("AICustomer") && c.transform.childCount > 0)
            {
                stealableTargets++;
            }
        }
        return stealableTargets;
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
        
        Vector2 target = (Vector2) (transform.position - securityPosition);
        float dist = Vector2.Distance(transform.position, target);
        MoveTowardsTarget(target, dist);
    }


    /**
        if (state == State.TAKE_ROLL)
        {
            TakeRoll();
            setNewDest = true;
            state = State.EXIT;
        }

        if (state == State.SEARCH_ROLL)
        {
            path = GetComponent<TestMove>().GetPath();
            float dist = Vector2.Distance(transform.position, GetComponent<TestMove>().FinalTarget());

            ApplyForce(Seek(nextTarget, slowDownRadius, dist));
            UpdatePath();
            UpdateMovement();

            if (dist <= 0.1f)
                state = State.FETCH_ROLL;



           // LookingForRolls();
           // if (availableRolls.Count > 0 && availableRolls[0] != null)
            //    state = State.TAKE_ROLL;
        }

        if (state == State.FETCH_ROLL)
        {
            //LookingForRolls();
           // TakeRoll();

           // if (availableRolls[0] != null)
           // {
           //     setNewDest = true;
           //     state = State.EXIT;
           // }else
           // {

           // }
        }

        if (state == State.EXIT)
        {
            SetNewDestination(new Vector3(17.5f, -5.5f, 0));
        }

        //reroute when something is blocked at next target pos and when not reached at the target pos yet
       // Collider2D[] hitColliders = Physics2D.OverlapCircleAll(nextTarget, 0.5f);
      // if (hitColliders.Length > 0 && Vector2.Distance(transform.position, nextTarget) > 0.03f)
       //     SetNewDestination(destination.position);
        **/

}