using Managers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AiController))]
public class WildBehaviour : MonoBehaviour
{

	[SerializeField] private GameObject capturingPlayerRange;
	[SerializeField] private float chaseRange = 10f;
	[SerializeField] private float attackRange = 1f;
	[SerializeField] private float attackBuffer = 2f;

	[SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed

	[Header("Hostility Settings")]
	[SerializeField] private float maxHostility = 10f;
	[SerializeField] private float hostilityDecayRate = 0.5f; // Hostility points lost per second while idle
	[SerializeField] private float fleeHealthThreshold = 0.5f; // Health % threshold to trigger flee when hostility < 5

	[Header("Timid Settings")]
	[SerializeField] private float timid; // Timid value determines likelihood and threshold of fleeing

	[Header("Wander Settings")]
	[SerializeField] private float wanderIntervalMin = 5f;
	[SerializeField] private float wanderIntervalMax = 10f;
	[SerializeField] private float minWanderRadius = 2f; // Radius within which to pick a random point to wander
	[SerializeField] private float maxWanderRadius = 5f; // Radius within which to pick a random point to wander

	private GameObject instantiatedPlayerRangeIndicator;
	private AiController controller;
	private Coroutine actionCoroutine = null;
	private float attackTime = 0;
	private float hostility;
	private float initialHostility;
	private float nextWanderTime = -1f;
	private bool isIdle = false;
	private bool isWandering = false;

	private Queue<Vector3> unsafeDirections = new Queue<Vector3>();
	private int maxUnsafeDirections = 3; // Limit to store only a few recent directions


	private void Start()
	{
		controller = GetComponent<AiController>();
	}

	private void OnEnable()
	{
		initialHostility = Random.Range(0f, 5f);
		hostility = initialHostility;
		timid = Random.Range(0f, 10f); // Assign a random timid value between 0 and 10
	}

	private void Update()
	{
		// Reset attack buffer on AI.
		if (attackTime > 0) attackTime -= Time.deltaTime;

		// Decay hostility while idle
		if (isIdle && hostility > initialHostility)
		{
			hostility = Mathf.Max(initialHostility, hostility - hostilityDecayRate * Time.deltaTime);
		}

		// AI behaviours
		if (controller.LilGuy.Health <= 0)
		{
			actionCoroutine ??= StartCoroutine(Dead());
		}
		else if (controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health <= controller.LilGuy.MaxHealth * Mathf.Lerp(0.25f, 0.5f, timid / 10f))
		{
			actionCoroutine ??= StartCoroutine(Flee());
		}
		else if (controller.DistanceToPlayer() <= attackRange && hostility > 3f)
		{
			actionCoroutine ??= StartCoroutine(AttackPlayer());
		}
		else if (controller.DistanceToPlayer() <= chaseRange && hostility > 3f)
		{
			actionCoroutine ??= StartCoroutine(ChasePlayer());
		}
		else if (Time.time >= nextWanderTime)
		{
			actionCoroutine ??= StartCoroutine(Wander());
		}
		else
		{
			actionCoroutine ??= StartCoroutine(Idle());
		}
	}

	public void OnDisable()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
		StopAllCoroutines();

		if (controller == null) return;
		controller.ToggleInteractCanvas(false);
		controller.LilGuy.RB.isKinematic = false;
	}

	/// <summary>
	/// Increase hostility when hit.
	/// </summary>
	/// <param name="amount">The amount of hostility this lil guy will gain.</param>
	public void IncreaseHostility(float amount)
	{
		hostility = Mathf.Min(maxHostility, hostility + amount);
	}

	/// <summary>
	/// State that handles when the AI is idle.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Idle()
	{
		isIdle = true;
		controller.LilGuy.IsMoving = false;

		while (controller.DistanceToPlayer() > chaseRange && controller.LilGuy.Health > 0 && Time.time < nextWanderTime)
		{
			yield return null;
		}
		actionCoroutine = null;
	}

	/// <summary>
	/// State that handles wandering.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Wander()
	{
		float angle = Random.Range(0f, Mathf.PI * 2f);                                      // Generate a random angle in radians												
		float radius = Random.Range(minWanderRadius, maxWanderRadius);                      // Generate a random radius between the minimum and maximum range						
		Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;       // Calculate the wander target in local space using polar coordinates																				
		Vector3 wanderTarget = transform.position + offset;                                 // Add the offset to the current position

		RaycastHit hit;
		if (Physics.Raycast(wanderTarget + Vector3.up * 10f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground")))
		{
			if (!IsNearPitEdge(hit.point))
			{
				wanderTarget = hit.point;
				while (Vector3.Distance(transform.position, wanderTarget) > 0.5f && controller.LilGuy.Health > 0 && hostility <= 3)
				{
					controller.LilGuy.MovementDirection = (wanderTarget - transform.position).normalized;
					controller.LilGuy.IsMoving = true;
					controller.LilGuy.MoveLilGuy();
					yield return null;
				}
			}

		}
		actionCoroutine = null;
		nextWanderTime = Time.time + Random.Range(wanderIntervalMin, wanderIntervalMax);
	}

	/// <summary>
	/// State that handles when the AI dies.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Dead()
	{
		isIdle = false;
		controller.LilGuy.IsMoving = false;
		controller.LilGuy.PlayDeathAnim(true);
		controller.LilGuy.RB.isKinematic = true;
		controller.LilGuy.RB.velocity = Vector3.zero;

		instantiatedPlayerRangeIndicator = Instantiate(capturingPlayerRange, transform.position, Quaternion.identity, Managers.SpawnManager.Instance.transform);
		instantiatedPlayerRangeIndicator.GetComponent<CaptureZone>().Init(controller.LilGuy);
		float currTime = 0;
		while (currTime < timeBeforeDestroyed)
		{
			currTime += Time.deltaTime;
			yield return null;
		}
		SpawnManager.Instance.RemoveLilGuyFromSpawns();
		Destroy(gameObject);
		actionCoroutine = null;
	}

	/// <summary>
	/// State that handles attacking the player.
	/// </summary>
	/// <returns></returns>
	private IEnumerator AttackPlayer()
	{
		isIdle = false;
		while (controller.DistanceToPlayer() <= attackRange && controller.LilGuy.Health > controller.LilGuy.MaxHealth * Mathf.Lerp(0.25f, 0.5f, timid / 10f) && hostility > 3)
		{
			if (hostility >= 7 && controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && attackTime <= 0 && controller.LilGuy is StrengthType strengthLilGuy)
			{
				controller.LilGuy.StartChargingSpecial();
				attackTime = attackBuffer;
			}
			else if (controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && attackTime <= 0 && controller.LilGuy is DefenseType defenseLilGuy && controller.LilGuy.Health * 2 <= controller.LilGuy.MaxHealth)
			{
				controller.LilGuy.StartChargingSpecial();
				attackTime = attackBuffer;
			}
			else if (attackTime <= 0)
			{
				controller.LilGuy.Attack();
				attackTime = attackBuffer;
			}
			yield return null;
		}
		actionCoroutine = null;
	}

	/// <summary>
	/// State that handles chasing the player.
	/// </summary>
	/// <returns></returns>
	private IEnumerator ChasePlayer()
	{
		isIdle = false;
		controller.LilGuy.IsMoving = true;
		while (controller.DistanceToPlayer() > attackRange && controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health > controller.LilGuy.MaxHealth * Mathf.Lerp(0.25f, 0.5f, timid / 10f) && hostility > 3)
		{
			controller.LilGuy.MovementDirection = (controller.FollowPosition.position - controller.transform.position).normalized;
			if (hostility >= 7f && controller.LilGuy is SpeedType && controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0)
			{
				controller.LilGuy.StartChargingSpecial();
			}
			else
			{
				controller.LilGuy.MoveLilGuy();
			}
			yield return null;
		}
		actionCoroutine = null;
	}

	/// <summary>
	/// State that handles fleeing behavior.
	/// </summary>
	/// <returns></returns>
	/// <summary>
	/// State that handles fleeing behavior.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Flee()
	{
		isIdle = false;
		controller.LilGuy.IsMoving = true;

		controller.LilGuy.MovementDirection = (controller.transform.position - controller.FollowPosition.position).normalized;
		while (controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health > 0)
		{
			Vector3 nextPos = controller.LilGuy.RB.position + (controller.LilGuy.MovementDirection);

			// Check if too close to a pit
			if (IsNearPitEdge(nextPos))
			{
				yield return StartCoroutine(FindValidFleeDirection(nextPos));
			}
			else
			{
				// No hit, adjust direction
				controller.LilGuy.MoveLilGuy();
			}

			yield return null;
		}

		actionCoroutine = null;
	}

	private IEnumerator FindValidFleeDirection(Vector3 dir)
	{
		int iterations = 12;
		float angleStep = Mathf.PI * 2f / iterations;
		float radius = 3f;
		RaycastHit hit;
		Vector3 bestDir = Vector3.zero;
		float bestWeight = float.NegativeInfinity;

		for (int i = 0; i < iterations; i++)
		{
			float angle = i * angleStep;
			Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
			Vector3 testPos = transform.position + offset;

			if (Physics.Raycast(testPos + Vector3.up * 10f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground", "PitColliders")))
			{
				if (hit.collider.gameObject.layer != LayerMask.NameToLayer("PitColliders") && dir.y - hit.point.y <= 10)
				{
					Vector3 potentialDir = (hit.point - transform.position).normalized;

					// Calculate a weight for this direction based on recent unsafe directions
					float weight = 1f - unsafeDirections.Sum(ud => Vector3.Dot(ud, potentialDir));
					if (weight > bestWeight)
					{
						bestWeight = weight;
						bestDir = potentialDir;
					}
				}
			}
		}

		if (bestWeight > float.NegativeInfinity)
		{
			dir = bestDir;
			controller.LilGuy.MovementDirection = dir;
			controller.LilGuy.MoveLilGuy();

			unsafeDirections.Enqueue(-dir);
			if (unsafeDirections.Count > maxUnsafeDirections) unsafeDirections.Dequeue();
		}
		yield return null;
	}

	// Helper Method: Detect proximity to pit edges
	private bool IsNearPitEdge(Vector3 position)
	{
		float pitDetectionRadius = 3f; // Distance to check for edges
		int iterations = 8; // Number of directions to check around the AI
		float angleStep = Mathf.PI * 2f / iterations;
		RaycastHit hit;

		for (int i = 0; i < iterations; i++)
		{
			// Calculate test direction
			float angle = i * angleStep;
			Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * pitDetectionRadius;
			Vector3 testPos = position + offset;

			// Visualize ray for debugging (optional)

			// Raycast downward to check if this is a valid edge
			if (Physics.Raycast(testPos + Vector3.up * 10f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground", "PitColliders")))
			{

				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PitColliders") || position.y - hit.point.y > 10)
				{
					// Near an edge or pit
					Vector3 fleeDirection = (position - testPos).normalized;
					controller.LilGuy.MovementDirection = fleeDirection;
					controller.LilGuy.MoveLilGuy();
					return true;
				}
			}
		}

		// No edge detected nearby
		return false;
	}

	private void OnDestroy()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
	}
}
