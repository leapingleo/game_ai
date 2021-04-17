using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	protected GroupManager groupManager;

	public enum State { IDLE, FOLLOW, EXIT, TAKE_ROLL, SEARCH_ROLL, FETCH_ROLL, CAUGHT, SEARCH_STEAL_TARGET, STEAL, FLEE };
	public Collider2D Collider { get { return shopper_collider; } }
	public float visionDistance;
	public float maxSpeed;
	public float maxForce;
	public float visionAngle;
	public float slowDownRadius = 1f;
	//public GameObject nextWallFollow;
	protected Vector2 wallFollow;
	//public Transform destination;
	public Vector2 velocity;

	protected Character leader;
	protected Collider2D shopper_collider;
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
	protected int currentRollsOnHand;
	protected bool hasRoll;
	protected bool setNewDest = true;
	protected bool canFetchOneRollFromShelf = true;

	protected bool trySteal = false;

	protected Transform targetToStealFrom;
	public bool HasRoll { get { return hasRoll; } }

	public GameObject wallFollowMarker;
	public State state;

	public virtual void Start()
	{
		acceleration = Vector3.zero;
		//  velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		shopper_collider = GetComponent<Collider2D>();
		cam = Camera.main;
	}

	//destructor
	void OnDestroy()
	{
		if (groupManager != null)
		{
			groupManager.UnRegister(this);
		}
	}
	public void SetLeader(Character leader)
	{
		this.leader = leader;
	}

	public void SetGroupManager(GroupManager groupManager)
	{
		this.groupManager = groupManager;
	}

	public void SetState(State state)
    {
		this.state = state;
    }

	protected void SearchShelf()
	{
		if (!canFetchOneRollFromShelf)
			return;

		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f);
		foreach (var c in hitColliders)
		{
			if (c.transform.CompareTag("Shelf"))
			{
				if (c.transform.childCount > 0)
				{
					canFetchOneRollFromShelf = false;
					currentRollsOnHand++;
					GameObject roll = c.transform.GetChild(0).gameObject;
					roll.transform.parent = transform;
					roll.transform.position = transform.position;
				}
			}
		}
	}

	protected Transform StealTargetNearby()
	{
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(nextTarget, 15f);
		List<Transform> stealableTargets = new List<Transform>();

		foreach (var c in hitColliders)
		{
			if (c != GetComponent<Collider2D>() && 
				(c.CompareTag("AICustomer") || c.CompareTag("Follower")) && c.transform.childCount > 0)
			{
				Debug.Log("name " + c.name);
				stealableTargets.Add(c.transform);
			}
		}
		if (stealableTargets.Count > 0)
			return stealableTargets[Random.Range(0, stealableTargets.Count - 1)];

		return null;
	}

	protected void StealTarget()
	{
		if (trySteal)
			return;

		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
		foreach (var c in hitColliders)
		{
			if (c.CompareTag("AICustomer") || c.CompareTag("Follower"))
			{
				trySteal = true;
				if (c.transform.childCount > 0)
				{
					GameObject roll = c.transform.GetChild(0).gameObject;
					roll.transform.parent = transform;
					roll.transform.position = transform.position;
				}
			}
		}
	}
	/**
	protected void TakeRollsFromShelf()
	{
		availableRolls = new List<GameObject>();
		Collider2D[] neighboursColliders = Physics2D.OverlapCircleAll(transform.position, 2f);

		foreach (Collider2D c in neighboursColliders)
		{
			//only rolls add to list when it's not taken
			if (c.CompareTag("Roll") && !c.transform.parent.CompareTag("Follower")
				&& !c.transform.parent.CompareTag("Leader"))
			{
				availableRolls.Add(c.transform.gameObject);
			}
		}

		if (!HasRoll)
			TakeRoll();
	}
	**/

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

	protected virtual Vector2 UpdateMovement()
	{
		if (velocity.magnitude > 0.01f && velocity.magnitude < 0.03f)
		{
			slowdownFace = velocity;
		}

		transform.up = velocity;
		velocity += acceleration;
		velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

		transform.position += (Vector3)velocity * Time.deltaTime;

		//reset acceleration each call
		acceleration *= 0;

		return velocity;
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
		//if (setNewDest)
		//{
		//	setNewDest = false;
			path = GetComponent<TestMove>().SetNewPath(dest);

			if (path != null || path.Count > 0)
				nextTarget = path[path.Count - 1];
		//}

		//float d = Vector2.Distance(transform.position, dest);
		//ApplyForce(Seek(nextTarget, slowDownRadius, d));
		//UpdatePath();
		//UpdateMovement();
	}

	protected void MoveTowardsTarget(Vector2 target, float distToTarget)
	{
		ApplyForce(Seek(target, slowDownRadius, distToTarget));
		UpdatePath();
		UpdateMovement();
	}

	protected void SetNewDestinationSecurity(Vector3 dest)
	{
		if (setNewDest)
		{
			Debug.Log("In If");
			setNewDest = false;
			path = GetComponent<TestMove>().SetNewPath(dest);

			if (path != null && path.Count > 0)
			{
				nextTarget = path[path.Count - 1];
			}
		}
		float d = Vector2.Distance(transform.position, dest);
		//Debug.Log("Float d: " + d);
		ApplyForce(Seek(nextTarget, slowDownRadius, d));
		UpdatePath();
		UpdateMovement();
	}

	protected void ApplyForce(Vector2 force)
	{
		acceleration += force;
	}

	protected virtual void UpdatePath()
	{
		if (path == null || path.Count == 0)
		{
			return;
		}
		path.Reverse();


		Vector2 arrivingNext = path[0];

		if (Vector2.SqrMagnitude(arrivingNext - (Vector2)transform.position) < 0.05f * 0.05f)
		{
			path.RemoveAt(0);

			//	Arrived at destination, so there will be only one node in the path.
			//	After removed the one, the program will certainly crash if try to access element at index 0 in the array.
			//	In this case, the game object should be destroyed and removed from the scene.
			//	More importantly, the elements in the array should never be accessed ANY MORE in this case.

			//if (path.Count == 0)
			//{
			//groupManager.UnRegister(this);
			//Destroy(this.gameObject);
			//	return;
			//}
			if (path.Count > 0)
			{
				nextTarget = path[0];
				storeTarget = nextTarget;
			}
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
}