using Managers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq.Expressions;

[RequireComponent(typeof(AiController))]
public class WildBehaviour : MonoBehaviour
{

	[SerializeField] private GameObject capturingPlayerRange;
	[SerializeField] private float chaseRange = 10f;
	[SerializeField] private float attackRange = 1f;
	[SerializeField] private float attackBuffer = 2f;
	[SerializeField] private string currentState = "Idle State";

	[Tooltip("How long this lil guy can stay outside of their home camp before they return back to it.")]
	[SerializeField] private float maxTimeOutsideHomeSpawner = 3f;

	[SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed

	[Header("Hostility Settings")]
	[SerializeField] private float initialHostility = -1;
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

	[SerializeField] private bool isCatchable = true;

	private GameObject instantiatedPlayerRangeIndicator;
	private AiController controller;
	private Coroutine actionCoroutine = null;
	private SpawnerObj homeSpawner = null;
	private Collider lilGuyCollider;

	private float attackTime = 0;
	private float timeSpentFromHome = 0f;
	private float hostility;
	private float nextWanderTime = -1f;
	private bool isIdle = false;
	private bool returnHome = false;

	public SpawnerObj HomeSpawner { get { return homeSpawner; } set { homeSpawner = value; } }



	private void Start()
	{
		controller = GetComponent<AiController>();
		lilGuyCollider = GetComponent<Collider>();
	}

	private void OnEnable()
	{
		if (initialHostility < 0) initialHostility = Random.Range(0f, 5f);
		hostility = initialHostility;
		timid = Random.Range(0f, 10f); // Assign a random timid value between 0 and 10
	}

	private void Update()
	{
		// Reset attack buffer on AI.
		if (attackTime > 0) attackTime -= Time.deltaTime;
		if (!IsWithinCamp()) timeSpentFromHome += Time.deltaTime;
		else timeSpentFromHome = 0f;
		if (timeSpentFromHome >= maxTimeOutsideHomeSpawner)
		{
			returnHome = true;
		}
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
		else if (isCatchable && returnHome)
		{
			actionCoroutine ??= StartCoroutine(ReturnHome());
		}
		else if (isCatchable && controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health <= controller.LilGuy.MaxHealth * Mathf.Lerp(0.125f, 0.5f, timid / 10f) && timid > 5)
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
		else if (isCatchable && Time.time >= nextWanderTime)
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

	private IEnumerator ReturnHome()
	{
		currentState = "Return Home State";
		Debug.Log($"{controller.LilGuy.GuyName}: Return Home State");
		float angle = Random.Range(0f, Mathf.PI * 2f);                                                  // Generate a random angle in radians						
		Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.5f * homeSpawner.SpawnRadius;  // Calculate the wander target in local space using polar coordinates																				
		Vector3 wanderTarget = homeSpawner.transform.position + offset;                                 // Add the offset to the current position

		RaycastHit hit;
		if (Physics.Raycast(wanderTarget + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Ground")))
		{
			if (!Physics.Raycast(controller.LilGuy.RB.position, wanderTarget, Vector3.Distance(transform.position, wanderTarget) + 2, LayerMask.GetMask("PitColliders")))
			{
				wanderTarget = hit.point;
				while (Vector3.Distance(transform.position, wanderTarget) > 0.5f && controller.LilGuy.Health > 0)
				{
					controller.LilGuy.MovementDirection = (wanderTarget - transform.position).normalized;
					controller.LilGuy.IsMoving = true;
					controller.LilGuy.MoveLilGuy();
					yield return null;
				}
			}

		}
		actionCoroutine = null;
		returnHome = false;
		timeSpentFromHome = 0;
	}

	/// <summary>
	/// State that handles when the AI is idle.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Idle()
	{
		isIdle = true;
		controller.LilGuy.IsMoving = false;

		currentState = "Idle State";
		Debug.Log($"{controller.LilGuy.GuyName}: Idle State");
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
		float angle = Random.Range(0f, Mathf.PI * 2f);                                                  // Generate a random angle in radians						
		Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * homeSpawner.SpawnRadius;  // Calculate the wander target in local space using polar coordinates																				
		Vector3 wanderTarget = homeSpawner.transform.position + offset;                                 // Add the offset to the current position

		RaycastHit hit;
		currentState = "Wander State";
		Debug.Log($"{controller.LilGuy.GuyName}: Wander State");
		if (Physics.Raycast(wanderTarget + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Ground")))
		{
			if (!Physics.Raycast(controller.LilGuy.RB.position, wanderTarget, Vector3.Distance(transform.position, wanderTarget) + 2, LayerMask.GetMask("PitColliders")))
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
		controller.LilGuy.RB.velocity = Vector3.zero;
		controller.LilGuy.RB.isKinematic = true;

		currentState = "Dead State";
		Debug.Log($"{controller.LilGuy.GuyName}: Dead State");
		homeSpawner.RemoveLilGuyFromSpawns();
		controller.HealthUi.gameObject.SetActive(false);
		if (isCatchable)
		{
			instantiatedPlayerRangeIndicator = Instantiate(capturingPlayerRange, transform.position, Quaternion.identity, Managers.SpawnManager.Instance.transform);
			instantiatedPlayerRangeIndicator.GetComponent<CaptureZone>().Init(controller.LilGuy);
			float currTime = 0;
			while (currTime < timeBeforeDestroyed)
			{
				currTime += Time.deltaTime;
				yield return null;
			}
		}
		controller.LilGuy.SpawnDeathParticle();
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
		currentState = "Attack State";
		Debug.Log($"{controller.LilGuy.GuyName}: Attack State");
		while (controller.DistanceToPlayer() <= attackRange)
		{
			if (isCatchable && (timid > 5 && controller.LilGuy.Health <= controller.LilGuy.MaxHealth * Mathf.Lerp(0.125f, 0.5f, timid / 10f) || hostility <= 3 || returnHome))
			{
				break;
			}

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
		currentState = "Chase State";
		Debug.Log($"{controller.LilGuy.GuyName}: Chase State");
		while (controller.DistanceToPlayer() > attackRange && controller.DistanceToPlayer() <= chaseRange)
		{
			if (isCatchable && (timid > 5 && controller.LilGuy.Health <= controller.LilGuy.MaxHealth * Mathf.Lerp(0.125f, 0.5f, timid / 10f) || hostility <= 3 || returnHome))
			{
				Debug.Log("For some reason we're here");
				break;
			}
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
	/// <summary>
	/// State that handles fleeing behavior.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Flee()
	{
		isIdle = false;
		controller.LilGuy.IsMoving = true;
		currentState = "Flee State";
		Debug.Log($"{controller.LilGuy.GuyName}: Flee State");
		while (controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health > 0 && !returnHome)
		{
			if (controller.FollowPosition == null) break;
			controller.LilGuy.MovementDirection = (controller.transform.position - controller.FollowPosition.position).normalized;
			controller.LilGuy.MoveLilGuy();
			yield return null;
		}
		actionCoroutine = null;
	}

	private bool IsWithinCamp()
	{
		return Physics.OverlapSphere(homeSpawner.transform.position, homeSpawner.SpawnRadius).Contains(lilGuyCollider);
	}

	private void OnDestroy()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
	}
}
