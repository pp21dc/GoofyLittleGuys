using Managers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

[RequireComponent(typeof(AiController))]
public class WildBehaviour : MonoBehaviour
{

	[SerializeField] private GameObject capturingPlayerRange;
	[SerializeField] private float chaseRange = 10f;
	[SerializeField] private float attackRange = 1f;
	[SerializeField] private float attackBuffer = 2f;
	[SerializeField] private AIState currentState = AIState.Idle;
	private AIState previousState;

	[Tooltip("How long this lil guy can stay outside of their home camp before they return back to it.")]
	[SerializeField] private float maxTimeOutsideHomeSpawner = 3f;

	[SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed

	[Header("Hostility Settings")]
	[SerializeField] private float initialHostility = -1;
	[SerializeField] private float maxHostility = 10f;
	[SerializeField] private float hostilityDecayRate = 0.5f; // Hostility points lost per second while idle
	[SerializeField] private float fleeHealthThreshold = 0.5f; // Health % threshold to trigger flee when hostility < 5
	[SerializeField] private float minAggroRadius = 2f; // Health % threshold to trigger flee when hostility < 5
	[SerializeField] private float maxAggroRadius = 20f; // Health % threshold to trigger flee when hostility < 5

	[Header("Personality Stats (DO NOT EDIT)")]
	[SerializeField, ReadOnly(true), Tooltip("How likely they'll flee. Timid values < 5 won't flee, >= 5 will.")]
	private float timid;    // Timid value determines likelihood and threshold of fleeing

	[SerializeField, ReadOnly(true), Tooltip("How far in the future lil guys plan their chase routes. Higher intelligence means higher likelihood for them to try to cut off the player's escape route.")]
	private float intelligence; // intelligence value determines how far in the future they'll plan their chase routes
	[SerializeField, ReadOnly(true), Tooltip("How likely this Lil Guy will aggro other lil guys in range. Charisma > 5 will broadcast aggression to others, and the charisma value will determine it's range.")]
	private float charisma;


	[SerializeField, Tooltip("How fast in the future (seconds) will the AI predict player movement.")] private float fastestThinkSpeed = 0.25f;
	[SerializeField, Tooltip("How far in the future (seconds) will the AI predict player movement.")] private float slowestThinkSpeed = 2f;

	[Header("Wander Settings")]
	[SerializeField] private float wanderIntervalMin = 5f;
	[SerializeField] private float wanderIntervalMax = 10f;
	[SerializeField] private float minWanderRadius = 2f; // Radius within which to pick a random point to wander
	[SerializeField] private float maxWanderRadius = 5f; // Radius within which to pick a random point to wander

	[SerializeField] private bool isCatchable = true;

	private GameObject instantiatedPlayerRangeIndicator;
	private AiController controller;
	private Coroutine deathCoroutine = null;
	private SpawnerObj homeSpawner = null;
	private Collider lilGuyCollider;

	private float attackTime = 0;
	private float timeSpentFromHome = 0f;
	private float hostility;
	private float thinkSpeed;
	private float aggroRadius;
	private float nextWanderTime = -1f;
	private bool isIdle = false;
	private bool returnHome = false;
	GameObject faintedEffect;
	public SpawnerObj HomeSpawner { get { return homeSpawner; } set { homeSpawner = value; } }
	public float Charisma => charisma;
	public bool IsCatchable { get => isCatchable; set => isCatchable = value; }

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
		intelligence = Random.Range(0f, 10f); // Assign a random timid value between 0 and 10
		charisma = Random.Range(0f, 10f);
		aggroRadius = Mathf.Lerp(minAggroRadius, maxAggroRadius, charisma / 10);
		thinkSpeed = Mathf.Lerp(fastestThinkSpeed, slowestThinkSpeed, intelligence / 10);
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
			ChangeState(AIState.Dead);
		}
		else if (returnHome)
		{
			ChangeState(AIState.ReturnHome);
		}
		else if (isCatchable && controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health <= controller.LilGuy.MaxHealth * Mathf.Lerp(0.125f, 0.5f, timid / 10f) && timid > 5)
		{
			ChangeState(AIState.Flee);
		}
		else if (controller.DistanceToPlayer() <= attackRange && hostility > 3f)
		{
			ChangeState(AIState.Attack);
		}
		else if (controller.DistanceToPlayer() <= chaseRange && hostility > 3f)
		{
			ChangeState(AIState.Chase);
		}
		else if (isCatchable && Time.time >= nextWanderTime)
		{
			ChangeState(AIState.Wander);
		}
		else
		{
			ChangeState(AIState.Idle);
		}

		HandleState();
	}

	private void FixedUpdate()
	{
		switch (currentState)
		{
			case AIState.Wander:
				HandleWander();
				break;
			case AIState.Chase:
				HandleChase();
				break;
			case AIState.Flee:
				HandleFlee();
				break;
			case AIState.ReturnHome:
				HandleReturnHome();
				break;
		}
	}

	public void OnDisable()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
		if (faintedEffect != null) Destroy(faintedEffect);
		StopAllCoroutines();

		if (controller == null) return;
		controller.ToggleInteractCanvas(false);
		controller.LilGuy.RB.isKinematic = false;
	}

	private void ChangeState(AIState state)
	{
		if (currentState == state) return;

		previousState = currentState;
		currentState = state;

		Debug.Log($"[{controller.LilGuy.GuyName}]: Changed to {state} State.");
	}

	private void HandleState()
	{
		switch (currentState)
		{
			case AIState.Idle:
				HandleIdle();
				break;
			case AIState.Attack:
				HandleAttack();
				break;
			case AIState.Dead:
				HandleDead();
				break;
		}
	}

	private void HandleIdle()
	{
		controller.LilGuy.IsMoving = false;
	}


	private void HandleWander()
	{
		if (Time.time >= nextWanderTime)
		{
			Vector3 wanderTarget = GetRandomWanderPoint();
			RaycastHit hit;
			if (!Physics.Raycast(wanderTarget + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Ground")))
			{
				wanderTarget = hit.point;
			}
			Vector3 dir = (wanderTarget - transform.position).normalized;
			MoveLilGuyTowards(wanderTarget);

			nextWanderTime = Time.time + Random.Range(wanderIntervalMin, wanderIntervalMax);
		}
	}

	private Vector3 GetRandomWanderPoint()
	{
		float angle = Random.Range(0f, Mathf.PI * 2f);
		Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Random.Range(minWanderRadius, maxWanderRadius);

		return homeSpawner.transform.position + offset;
	}

	private void MoveLilGuyTowards(Vector3 target, float moveSpeedAdjustment = 1.0f)
	{
		Vector3 direction = (target - transform.position).normalized;
		controller.LilGuy.MovementDirection = direction;
		controller.LilGuy.IsMoving = true;
		if (hostility >= 7f && controller.LilGuy is SpeedType && controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0)
		{
			controller.LilGuy.StartChargingSpecial();
		}
		else
		{
			controller.LilGuy.MoveLilGuy(moveSpeedAdjustment);
		}
	}

	private void HandleChase()
	{
		Vector3 targetPosition = PredictPlayerPosition();
		MoveLilGuyTowards(targetPosition);
	}

	private Vector3 PredictPlayerPosition()
	{
		PlayerBody body = controller.FollowPosition.GetComponent<PlayerBody>();

		// Get AI to Player vector
		Vector3 lilGuyToPlayer = controller.FollowPosition.position - controller.transform.position;

		// Get player's movement direction
		Vector3 playerMovementDir = body.MovementDirection.normalized;

		// If the player isn't moving, just go to their exact position
		if (playerMovementDir == Vector3.zero)
			return controller.FollowPosition.position;

		// Calculate dot product to determine positioning strategy
		float dot = Vector3.Dot(lilGuyToPlayer.normalized, playerMovementDir);

		Vector3 targetPosition;

		if (dot > 0.25f)  // AI is ahead of the player -> Charge directly
		{
			targetPosition = controller.FollowPosition.position;
		}
		else  // AI is behind or beside the player -> Predict escape path
		{
			// Predict the player's future position
			targetPosition = controller.FollowPosition.position + (playerMovementDir * body.MaxSpeed * thinkSpeed);

			// Smooth prediction to prevent overshooting
			targetPosition = Vector3.Lerp(controller.FollowPosition.position, targetPosition, 0.5f);
		}

		return targetPosition;
	}

	private void HandleAttack()
	{
		if (attackTime <= 0)
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
			else
			{
				controller.LilGuy.Attack();
				attackTime = attackBuffer;
			}
		}
	}

	private void HandleFlee()
	{
		// Get flee direction (opposite of player movement)
		Vector3 fleeDirection = (controller.transform.position - controller.FollowPosition.position).normalized;

		// Pick a target position in that direction at a safe distance
		Vector3 fleeTarget = controller.transform.position + fleeDirection * 5f;  // Adjust 5f as needed

		// Ensure AI doesn't run into obstacles (optional: raycast check)
		fleeTarget = ValidateFleePosition(fleeTarget, fleeDirection);

		// Move AI toward the flee target
		MoveLilGuyTowards(fleeTarget, 0.35f);
	}
	private Vector3 ValidateFleePosition(Vector3 fleeTarget, Vector3 fleeDirection)
	{
		RaycastHit hit;

		if (Physics.Raycast(controller.transform.position, fleeDirection, out hit, 5f, LayerMask.GetMask("PitColliders")))
		{
			// If there's an obstacle, pick a random perpendicular escape route
			Vector3 alternativeDirection = Vector3.Cross(Vector3.up, fleeDirection).normalized;
			fleeTarget = controller.transform.position + alternativeDirection * 5f;
		}

		return fleeTarget;
	}

	private void HandleReturnHome()
	{
		Vector3 homePosition = homeSpawner.transform.position;
		MoveLilGuyTowards(homePosition);

		if (IsWithinCamp()) returnHome = false;
	}

	private void HandleDead()
	{
		deathCoroutine ??= StartCoroutine(Dead());
	}

	/// <summary>
	/// Increase hostility when hit.
	/// </summary>
	/// <param name="amount">The amount of hostility this lil guy will gain.</param>
	public void IncreaseHostility(float amount)
	{
		hostility = Mathf.Min(maxHostility, hostility + amount);
	}

	public void AggroWildLilGuys()
	{
		Collider[] lilGuys = Physics.OverlapSphere(controller.LilGuy.RB.position, aggroRadius);
		foreach (Collider collider in lilGuys)
		{
			if (collider.GetComponent<AiController>() == null) continue;
			if (collider.GetComponent<AiController>().State == AiController.AIState.Wild)
			{
				WildBehaviour wild = collider.GetComponent<WildBehaviour>();
				if (wild == null) continue;
				wild.IncreaseHostility(Mathf.Lerp(0, 2, charisma));
			}
		}
	}

	private IEnumerator Dead()
	{
		isIdle = false;
		controller.LilGuy.IsMoving = false;
		controller.LilGuy.PlayDeathAnim(true);
		controller.LilGuy.RB.velocity = Vector3.zero;
		controller.LilGuy.RB.isKinematic = true;
		homeSpawner.RemoveLilGuyFromSpawns();
		faintedEffect = Instantiate(FXManager.Instance.GetEffect("Fainted"), transform.position, Quaternion.identity, transform);
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
		else
		{
			Hurtbox h = GetComponent<Hurtbox>();
			GameObject legendaryKillEffect = Instantiate(FXManager.Instance.GetEffect("LegendaryKill"), h.LastHit.transform.position + Vector3.forward, Quaternion.identity, h.LastHit.transform);
			h.LastHit.GameplayStats.KilledLegendary = true;
		}

		controller.LilGuy.SpawnDeathParticle();
		Destroy(gameObject);

	}

	private bool IsWithinCamp()
	{
		return Physics.OverlapSphere(homeSpawner.transform.position + homeSpawner.SpawnArea.center, homeSpawner.SpawnArea.radius).Contains(lilGuyCollider);
	}

	private void OnDestroy()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
	}

	
}

public enum AIState
{
	Idle,
	Wander,
	Chase,
	Attack,
	Flee,
	ReturnHome,
	Dead
}