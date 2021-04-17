using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityController : Character
{
	new private enum State { PATROLLING_GO, PATROLLING_BACK, ESCORTING };
	private State state;

	public Vector3 patrol_start;
	public Vector3 patrol_end;
	public float caughtingRadius;
	//public float caughtingSpeed = 3.0f;

	//public float speed;

	new Rigidbody2D rigidbody2D;

	Animator animator;

	private Character caughtFollower;
	private Character caughtingTarget;

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
					//gain *= maxSpeed;
					//gain *= caughtingSpeed;
					gain *= maxSpeed;

					//gain *= (1 - caughtingTarget.HandyRolls * 0.2f);

					transform.position += gain;

					gain.Normalize();
					animator.SetFloat("Move X", gain.x);
					animator.SetFloat("Move Y", gain.y);
				}
				else
				{
					float d = Vector2.Distance(transform.position, caughtingTarget.transform.position);
					ApplyForce(Seek(caughtingTarget.transform.position, 0.1f, d) * 3f);
					UpdateMovement();
				}

				return;
			}
		}

		switch (state)
		{
			case State.PATROLLING_GO:
				{
					SetNewDestinationSecurity(patrol_end);
					break;
				}

			case State.PATROLLING_BACK:
				{
					SetNewDestinationSecurity(patrol_start);
					break;
				}

			case State.ESCORTING:
				{
					SetNewDestinationSecurity(new Vector3(8.57f, -6.17f, 0));
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

	//	FSM
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

	Character LookForFollowerWithPaperRoll()
	{
		List<Transform> context = new List<Transform>();
		Collider2D[] contextColliders = Physics2D.OverlapCircleAll(transform.position, caughtingRadius);
		foreach (Collider2D collider in contextColliders)
		{
			Character follower = collider.gameObject.GetComponent<Character>();

			if (follower != null && follower.transform.childCount > 0)
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

			if (other.transform.CompareTag("Follower") || other.transform.CompareTag("AICustomer"))
			{
				if (other.transform.childCount > 0)
				{
					Character follower = other.transform.GetComponent<Character>();
					follower.enabled = false;
					follower.transform.parent = transform;
					follower.transform.position = transform.position;
					follower.transform.GetComponent<CircleCollider2D>().enabled = false;
					//follower.state = Character.State.CAUGHT;

					caughtFollower = follower;

					state = State.ESCORTING;
					setNewDest = true;
					SetNewDestinationSecurity(new Vector3(7.5f, -6.5f, 0));
				}
			}
		}
	}
}