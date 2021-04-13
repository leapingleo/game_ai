using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityController : Character
{
	new private enum State { PATROLLING_GO, PATROLLING_BACK, CAUGHTING, ESCORTING };
	private State state;

	public Vector3 patrol_start;
	public Vector3 patrol_end;
	public float caughtingRadius;
	public float caughtingSpeed = 3.0f;

	public float speed;

	new Rigidbody2D rigidbody2D;

	Animator animator;

	private Follower caughtFollower;
	private Follower caughtingTarget;

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
				gain.Normalize();
				gain *= Time.deltaTime;
				gain *= caughtingSpeed;

				transform.position += gain;

				gain.Normalize();
				animator.SetFloat("Move X", gain.x);
				animator.SetFloat("Move Y", gain.y);

				return;
			}
		}

		switch (state)
		{
			case State.PATROLLING_GO:
			{
				SetNewDestination(patrol_end);
				break;
			}

			case State.PATROLLING_BACK:
			{
				SetNewDestination(patrol_start);
				break;
			}

			case State.ESCORTING:
			{
				SetNewDestination(new Vector3(8.57f, -6.17f, 0));
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

	Follower LookForFollowerWithPaperRoll()
	{
		List<Transform> context = new List<Transform>();
		Collider2D[] contextColliders = Physics2D.OverlapCircleAll(transform.position, caughtingRadius);
		foreach (Collider2D collider in contextColliders)
		{
			Follower follower = collider.gameObject.GetComponent<Follower>();

			if (follower != null && follower.HasRoll)
			{
				return follower;
			}
		}
		return null;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (caughtFollower == null)
		{
			Follower follower = other.gameObject.GetComponent<Follower>();

			if (follower != null)
			{
				follower.transform.parent = transform;
				follower.transform.position = transform.position;
				follower.transform.GetComponent<CircleCollider2D>().enabled = false;
				follower.state = Character.State.CAUGHT;

				caughtFollower = follower;

				state = State.ESCORTING;
				setNewDest = true;
				SetNewDestination(new Vector3(8.57f, -6.17f, 0));
			}
		}
	}
}