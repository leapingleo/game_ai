using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityController : Character
{
	new public enum State { PATROLLING_GO, PATROLLING_BACK, CAUGHTING, ESCORTING };
	public State state;

	public Vector3 patrol_start;
	public Vector3 patrol_end;
	public float caughtingRadius;
	public float caughtingSpeed = 4.0f;

	public float speed;

	new Rigidbody2D rigidbody2D;

	Animator animator;

	private GameObject caughtFollower;
	private GameObject caughtingTarget;

	// Start is called before the first frame update
	new void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
		rigidbody2D = GetComponent<Rigidbody2D>();
	}

	public void Initialize(Vector3 start, Vector3 end)
	{
		transform.position = start;
		patrol_start = start;
		patrol_end = end;
	}

	//test for git desktop

	void Update()
	{
		if (state != State.ESCORTING)
		{
			caughtingTarget = LookForFollowerWithPaperRoll();

			if (caughtingTarget != null)
			{
				state = State.CAUGHTING;

				//setNewDest = true;
				//SetNewDestination(caughtingTarget.transform.position);
				Vector3 gain = caughtingTarget.transform.position - transform.position;
				if (caughtingTarget.GetComponent<Character>().velocity.magnitude > 0.1f)
				{
					float time = gain.magnitude;
					Vector3 direction = caughtingTarget.transform.up;
					direction.Normalize();

					Vector3 target = caughtingTarget.transform.position + direction * time;

					gain = target - transform.position;
					gain.Normalize();
					gain *= Time.deltaTime;
					//gain *= caughtingSpeed;

					//transform.position += gain;

					//gain.Normalize();
					float d = Vector2.Distance(transform.position, target);
					ApplyForce(Seek(target, 0.1f, d) * 3f);
					UpdateMovement();
				} else
                {
					float d = Vector2.Distance(transform.position, caughtingTarget.transform.position);
					ApplyForce(Seek(caughtingTarget.transform.position, 0.1f, d) * 3f);
					UpdateMovement();
				}

				animator.SetFloat("Move X", gain.x);
				animator.SetFloat("Move Y", gain.y);
				
				Debug.Log("name " + caughtingTarget.name);
				return;
			}
		}

		switch (state)
		{
			case State.PATROLLING_GO:
			{
				SetNewDestinationSecurity(patrol_start);
				break;
			}

			case State.PATROLLING_BACK:
			{
				SetNewDestinationSecurity(patrol_end);
				break;
			}

			case State.ESCORTING:
			{
				SetNewDestinationSecurity(new Vector3(7.5f, -5.5f, 0));
				break;
			}

			default:
			break;
		}
	}

	protected override Vector2 UpdateMovement()
	{
		Vector2 velocity = base.UpdateMovement();

		transform.up = Vector3.zero;

		animator.SetFloat("Move X", velocity.x);
		animator.SetFloat("Move Y", velocity.y);

		return velocity;
	}

	void ArrivedWithState()
	{
		setNewDest = true;

		switch (state)
		{
			case State.PATROLLING_GO:
			{
				state = State.PATROLLING_BACK;
				break;
			}

			case State.PATROLLING_BACK:
			{
				state = State.PATROLLING_GO;
				break;
			}

			case State.ESCORTING:
			{
				if (caughtFollower != null)
				{
					Destroy(caughtFollower.gameObject);
					caughtFollower = null;
				}
				state = State.PATROLLING_GO;
				break;
			}

			default:
			break;
		}
	}

	protected override void UpdatePath()
	{
		if (path == null || path.Count == 0)
		{
			ArrivedWithState();
			return;
		}
			

		path.Reverse();

		Vector2 arrivingNext = path[0];

		if (Vector2.SqrMagnitude(arrivingNext - (Vector2)transform.position) < 0.05f)
		{
			path.RemoveAt(0);

			if (path.Count == 0)
			{
				ArrivedWithState();

				return;
			}

			nextTarget = path[0];
			storeTarget = nextTarget;
		}
	}

	GameObject LookForFollowerWithPaperRoll()
	{
		List<Transform> context = new List<Transform>();
		Collider2D[] contextColliders = Physics2D.OverlapCircleAll(transform.position, caughtingRadius);
		foreach (Collider2D collider in contextColliders)
		{
			//GameObject customer = collider.gameObject.GetComponent<Follower>();


			if (collider.CompareTag("AICustomer") && collider.transform.childCount > 0)
			{
				return collider.gameObject;
			}
		}
		return null;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		/**
		if (caughtFollower == null)
		{
			//Follower follower = other.gameObject.GetComponent<Follower>();

			if (other.transform.CompareTag("AICustomer"))
			{
				other.transform.parent = transform;
				other.transform.position = transform.position;
				other.transform.GetComponent<CircleCollider2D>().enabled = false;
				other.gameObject.GetComponent<Character>().state = Character.State.CAUGHT;

				caughtFollower = other.gameObject;

				state = State.ESCORTING;
				setNewDest = true;
				SetNewDestinationSecurity(new Vector3(8.5f, -6f, 0));
			}
		}
		**/
		if (caughtFollower == null)
		{
			if (other.collider.CompareTag("AICustomer") && other.collider.transform.childCount > 0)
			{
				/**
				caughtFollower = other.collider.gameObject;
				
				other.transform.parent = transform;
				other.transform.position = transform.position;
				other.transform.GetComponent<CircleCollider2D>().enabled = false;
				other.transform.GetComponent<Character>().enabled = false;
				state = State.ESCORTING;
				//setNewDest = true;
				//SetNewDestinationSecurity(new Vector3(7.5f, -5.5f, 0));
				//SetNewDestinationSecurity(new Vector3(8.5f, -6f, 0));
				**/
				Destroy(other.collider.gameObject);
				state = State.PATROLLING_GO;
			}
		}
	}


}