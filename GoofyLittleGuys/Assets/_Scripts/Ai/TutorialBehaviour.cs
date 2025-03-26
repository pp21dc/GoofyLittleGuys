using Managers;
using System.Linq;
using System.Collections;
using UnityEngine;
using System.ComponentModel;

public class TutorialBehaviour : MonoBehaviour
{
	#region Public Variables & Serialize Fields
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject capturingPlayerRange;
	[ColoredGroup][SerializeField] private GameObject legendaryIcon; // Distance AI must move to not be considered stuck
	[ColoredGroup][SerializeField] private Transform home;

	[Header("AI Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private AIState currentState = AIState.Idle;
	[ColoredGroup][SerializeField] private bool isCatchable = true;
	[ColoredGroup][SerializeField] private float chaseRange = 10f;
	[ColoredGroup][SerializeField] private float attackRange = 1f;
	[ColoredGroup][SerializeField] private float attackBuffer = 2f;

	[Tooltip("Time in seconds after death until the lil guy despawns.")]
	[ColoredGroup][SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed

	[Tooltip("How long this lil guy can stay outside of their home camp before they return back to it.")]
	[ColoredGroup][SerializeField] private float maxTimeOutsideHomeSpawner = 3f;

	[Header("Hostility Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float initialHostility = -1;
	[ColoredGroup][SerializeField] private float maxHostility = 10f;
	[ColoredGroup][SerializeField] private float hostilityDecayRate = 0.5f; // Hostility points lost per second while idle
	[ColoredGroup][SerializeField] private float minAggroRadius = 2f; // Health % threshold to trigger flee when hostility < 5
	[ColoredGroup][SerializeField] private float maxAggroRadius = 20f; // Health % threshold to trigger flee when hostility < 5

	[Header("Personality Stats")]
	[HorizontalRule]
	[ColoredGroup]
	[SerializeField, DebugOnly, Tooltip("How likely they'll flee. Timid values < 5 won't flee, >= 5 will.")]
	private float timid;    // Timid value determines likelihood and threshold of fleeing

	[ColoredGroup]
	[SerializeField, DebugOnly, Tooltip("How far in the future lil guys plan their chase routes. Higher intelligence means higher likelihood for them to try to cut off the player's escape route.")]
	private float intelligence; // intelligence value determines how far in the future they'll plan their chase routes
	
	[ColoredGroup]
	[SerializeField, DebugOnly, Tooltip("How likely this Lil Guy will aggro other lil guys in range. Charisma > 5 will broadcast aggression to others, and the charisma value will determine it's range.")]
	private float charisma;


	[ColoredGroup][SerializeField, Tooltip("How fast in the future (seconds) will the AI predict player movement.")] private float fastestThinkSpeed = 0.25f;
	[ColoredGroup][SerializeField, Tooltip("How far in the future (seconds) will the AI predict player movement.")] private float slowestThinkSpeed = 2f;

	[Header("Wander Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float wanderIntervalMin = 5f;
	[ColoredGroup][SerializeField] private float wanderIntervalMax = 10f;
	[ColoredGroup][SerializeField] private float maxWanderRadius = 5f; // Radius within which to pick a random point to wander
	#endregion

	#region Private Variables
	private AIState previousState;
	private TutorialAiController controller;
	private SpawnerObj homeSpawner = null;
	private GameObject faintedEffect;
	private GameObject instantiatedPlayerRangeIndicator;
	private Collider lilGuyCollider;
	private Coroutine deathCoroutine = null;

	private Vector3 wanderTarget = Vector3.zero;
	private Vector3 lastPosition;
	private float levelUpdateTimer = 0;
	private float attackTime = 0;
	private float timeSpentFromHome = 0f;
	private float hostility;
	private float thinkSpeed;
	private float aggroRadius;
	private float nextWanderTime = -1f;
	private bool isIdle = false;
	private bool returnHome = false;
	private bool pickedLocation = false;

	#endregion

	#region Getters & Setters
	public SpawnerObj HomeSpawner { get { return homeSpawner; } set { homeSpawner = value; } }
	public float Charisma => charisma;
	public bool IsCatchable { get => isCatchable; set => isCatchable = value; }
	public float AttackRange { get => attackRange; set => attackRange = value; }
	public float ChaseRange { get => chaseRange; set => chaseRange = value; }
	public Transform Home { get => home; set => home = value; }
	public float TimeBeforeDestroyed { get => timeBeforeDestroyed; set => timeBeforeDestroyed = value; }
	#endregion
	private void Start()
	{
		controller = GetComponent<TutorialAiController>();
		lilGuyCollider = GetComponent<Collider>();
	}

	private void OnEnable()
	{
		hostility = initialHostility;
		timid = 0.1f; // Assign a random timid value between 0 and 10
		intelligence = 1; // Assign a random timid value between 0 and 10
		charisma = 1;
		aggroRadius = Mathf.Lerp(minAggroRadius, maxAggroRadius, charisma / 10);
		thinkSpeed = Mathf.Lerp(fastestThinkSpeed, slowestThinkSpeed, intelligence / 10);
	}

	private void Update()
	{
		if (isCatchable)
		{
			if (levelUpdateTimer <= GameManager.Instance.WildLilGuyLevelUpdateTick) levelUpdateTimer += Time.deltaTime;
			else
			{
				controller.LilGuy.UpdateLevel();
				controller.HealthBars.UpdateUI();
			}
		}
		if (legendaryIcon) legendaryIcon.SetActive(!isCatchable);
		// Reset attack buffer on AI.
		if (attackTime > 0) attackTime -= Time.deltaTime;

		if (home && !IsWithinCamp()) timeSpentFromHome += Time.deltaTime;
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
	private Waypoint FindClosestWaypoint()
	{
		Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
		Waypoint closest = null;
		float minDistance = Mathf.Infinity;

		foreach (Waypoint wp in allWaypoints)
		{
			float distance = Vector3.Distance(transform.position, wp.transform.position);
			if (distance < minDistance)
			{
				minDistance = distance;
				closest = wp;
			}
		}

		return closest;
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

		DebugManager.Log($"[{controller.LilGuy.GuyName}]: Changed to {state} State.", DebugManager.DebugCategory.AI, DebugManager.LogLevel.LOG);
		OnStateEnter(state); // Call OnStateEnter when state changes
	}

	private void OnStateEnter(AIState state)
	{
		switch (state)
		{
			case AIState.Wander:
				if (!pickedLocation)
				{
					wanderTarget = GetRandomWanderPoint();
					pickedLocation = true;
				}
				RaycastHit hit;
				if (Physics.Raycast(wanderTarget + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Ground")))
				{
					wanderTarget = hit.point;
				}
				break;
			case AIState.ReturnHome:
				wanderTarget = GetRandomWanderPoint();
				break;
		}
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
			Vector3 dir = (wanderTarget - transform.position).normalized;
			MoveLilGuyTowards(wanderTarget);

			if ((wanderTarget - transform.position).magnitude < 0.5f)
			{
				nextWanderTime = Time.time + Random.Range(wanderIntervalMin, wanderIntervalMax);
				pickedLocation = false;
			}
		}
	}

	private Vector3 GetRandomWanderPoint()
	{
		return home.position;
	}

	private void MoveLilGuyTowards(Vector3 target, float moveSpeedAdjustment = 1.0f)
	{
		Vector3 direction = (target - transform.position);
		float distance = direction.magnitude;

		direction = CheckForObstacles(direction);
		// Stop moving if within a small range
		if (distance < 0.5f)
		{
			controller.LilGuy.MovementDirection = Vector3.zero; // Stop movement
			controller.LilGuy.IsMoving = false;
			return;
		}

		// Normalize and move towards the target
		direction.Normalize();
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
			if (hostility >= 7 && controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && controller.LilGuy is StrengthType strengthLilGuy)
			{
				controller.LilGuy.StartChargingSpecial();
				attackTime = attackBuffer;
			}
			else if (controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && controller.LilGuy is DefenseType defenseLilGuy && controller.LilGuy.Health * 2 <= controller.LilGuy.MaxHealth)
			{
				controller.LilGuy.StartChargingSpecial();
				attackTime = attackBuffer;
			}
			else
			{
				// AI Combo Logic
				if (controller.LilGuy.CurrentComboCount == 0 || controller.LilGuy.CanChainAttack) // Allow chaining at the correct frames
				{
					controller.LilGuy.AttemptAttack();

					if (controller.LilGuy.CurrentComboCount >= controller.LilGuy.maxCombo)
					{
						attackTime = attackBuffer;
					}
				}
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
	private Vector3 CheckForObstacles(Vector3 inDirection)
	{
		RaycastHit hit;
		if (Physics.SphereCast(transform.position, 2f, inDirection.normalized, out hit, 3, LayerMask.GetMask("Obstacle", "PitColliders")))
		{
			Vector3 newDir;
			if (inDirection.x > 0f) newDir = Vector3.Cross(hit.normal, transform.up);
			else newDir = Vector3.Cross(transform.up, hit.normal);
			return Vector3.Lerp(inDirection, newDir, 1);
		}
		return inDirection;
	}

	private void HandleReturnHome()
	{
		lastPosition = transform.position;

		// 🔹 If close to home, stop return home behavior
		if (IsWithinCamp())
		{
			DebugManager.Log($"[{controller.LilGuy.GuyName}] Successfully returned home.", DebugManager.DebugCategory.AI);
			returnHome = false; // Exit return home state
			ChangeState(AIState.Wander); // Resume normal wandering
			return;
		}

		// 🔹 Prioritize moving home if no waypoint is currently set
		if (!returnHome || Vector3.Distance(transform.position, wanderTarget) < 1.0f)
		{
			wanderTarget = GetRandomWanderPoint(); // Pick new target inside home
		}

		MoveLilGuyTowards(wanderTarget);
	}
	
	private void HandleDead()
	{
		if (deathCoroutine == null)
		{
			deathCoroutine = StartCoroutine(Dead());
		}
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
		faintedEffect = Instantiate(FXManager.Instance.GetEffect("Fainted"), transform.position, Quaternion.identity, transform);
		controller.HealthUi.gameObject.SetActive(false);
		if (isCatchable)
		{
			instantiatedPlayerRangeIndicator = Instantiate(capturingPlayerRange, transform.position, Quaternion.identity, TutorialManager.Instance.transform);
			instantiatedPlayerRangeIndicator.GetComponent<CaptureZone>().Init(controller.LilGuy);
			float currTime = 0;
			while (currTime < timeBeforeDestroyed || controller.State != TutorialAiController.AIState.Tamed)
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
		return Physics.OverlapSphere(home.position, 1f).Contains(lilGuyCollider);
	}

	private void OnDestroy()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
	}
}