using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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

	private GameObject caughtFollower;
	private GameObject caughtingTarget;

	private Vector3 target;

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

	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
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

				float time = gain.magnitude;

				Vector3 direction = caughtingTarget.transform.up;
				direction.Normalize();

				target = caughtingTarget.transform.position + direction * time;


				RaycastHit2D raycast = Physics2D.Raycast(transform.position + gain.normalized * 0.6f, target - transform.position, time);

				// Debug.DrawRay(transform.position, target - transform.position, Color.red, 1 / 60f);

				if (!raycast || !raycast.transform.CompareTag("Obstacle"))
				{
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

					return;
				}
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

	GameObject LookForFollowerWithPaperRoll()
	{
		List<Transform> context = new List<Transform>();
		Collider2D[] contextColliders = Physics2D.OverlapCircleAll(transform.position, caughtingRadius);

		List<GameObject> customers = new List<GameObject>();

		foreach (Collider2D collider in contextColliders)
		{
			GameObject customer = collider.gameObject;

			if (customer.CompareTag("AICustomer") || customer.CompareTag("Follower"))
			{
				customers.Add(customer);
			}
		}

		customers = customers.OrderByDescending(f => (f.transform.position - transform.position).magnitude).ToList();
		customers.Reverse();

		if (customers.Count != 0)
		{
			return customers[0];
		}
		else
		{
			return null;
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (caughtFollower == null)
		{

			if (other.transform.CompareTag("Follower") || other.transform.CompareTag("AICustomer"))
			{
				if (other.transform.childCount > 0)
				{
					GameObject follower = other.gameObject;
					follower.SetActive(false);
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