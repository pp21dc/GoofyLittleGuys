using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AiController : MonoBehaviour
{
	[SerializeField] private float chaseRange = 10f;    // Range of which this AI will chase a target
	[SerializeField] private float attackRange = 2f;    // Range where this AI will attack a target

	[Tooltip("Time between attacks (in seconds).")]
	[SerializeField] private float attackBuffer = 1f;   // Time between this AI's attacks.


	private Transform player;                           // The transform of the closest player to this AI
	private LilGuyBase lilGuy;                          // Reference to this AI's stats
	private Vector3 moveDirection = Vector3.zero;

	public Transform Player { get { return player; } }
	public float ChaseRange { get { return chaseRange; } }
	public float AttackRange { get { return attackRange; } }
	public float AttackBuffer { get { return attackBuffer; } }
	public Vector3 MoveDirection { get { return moveDirection; } set { moveDirection = value; } }
	public LilGuyBase LilGuy { get { return lilGuy; } }

	// AI STATES
	public AiState currentState;
	public IdleState idleState;
	public ChaseState chaseState;
	public AttackState attackState;
	public DeadState deadState;

	// Start is called before the first frame update
	void Start()
	{
		idleState = new IdleState(this);
		chaseState = new ChaseState(this);
		attackState = new AttackState(this);
		deadState = new DeadState(this);

		lilGuy = gameObject.GetComponent<LilGuyBase>();

		TransitionToState(idleState);
	}

	// Update is called once per frame
	void Update()
	{
		if (moveDirection.x > 0) lilGuy.mesh.transform.localRotation = Quaternion.Euler(0, 180, 0);
		else if (moveDirection.x < 0) lilGuy.mesh.transform.localRotation = Quaternion.identity;
		player = FindClosestPlayer();
		currentState.UpdateState();
	}

	/// <summary>
	/// Calculates distance to closest player.
	/// </summary>
	/// <returns>float: The distance between the AI and the closest player.</returns>
	public float DistanceToPlayer()
	{
		return Vector3.Distance(transform.position, player.position);
	}

	/// <summary>
	/// Method that transitions from the AI's current state to the state provided.
	/// </summary>
	/// <param name="newState"> AiState: The new state the AI should transition to.</param>
	public void TransitionToState(AiState newState)
	{
		currentState?.ExitState();  // If current state isn't null, exit the current state
		currentState = newState;
		currentState.EnterState();
	}

	/// <summary>
	/// Method that is called on death to show the Last Prompt for the player who hit this AI last.
	/// </summary>
	public void ShowLastHitPrompt()
	{
		// Get the player who last hit this AI
		GameObject lastHitPlayerObj = GetComponent<Hurtbox>().lastHit;

		if (lastHitPlayerObj != null)
		{
			PlayerBody playerBody = lastHitPlayerObj.GetComponent<PlayerBody>();
			if (playerBody != null)
			{
				playerBody.ShowLastHitPrompt(GetComponent<LilGuyBase>());   // Show the UI prompt on their screen
				playerBody.EnableUIControl();                               // Transfer control to UI
			}
		}
	}

	/// <summary>
	/// Finds the closest player to this AI.
	/// </summary>
	/// <returns>Transform: the transform component of the closest player.</returns>
	private Transform FindClosestPlayer()
	{
		Transform currClosest = PlayerInput.all[0].transform;
		foreach (PlayerInput input in PlayerInput.all)
		{
			if (!input.GetComponent<PlayerBody>().InMinigame && Vector3.Distance(input.transform.position, transform.position) < Vector3.Distance(currClosest.transform.position, transform.position))
			{
				currClosest = input.transform;
			}
		}
		return currClosest;
	}
}

public abstract class AiState
{
	protected AiController controller;

	public AiState(AiController controller)
	{
		this.controller = controller;
	}

	public abstract void EnterState();

	public abstract void UpdateState();

	public abstract void ExitState();
}

public class IdleState : AiState
{
	public IdleState(AiController controller) : base(controller)
	{

	}

	public override void EnterState()
	{

	}

	public override void ExitState()
	{

	}

	public override void UpdateState()
	{
		if (controller.DistanceToPlayer() <= controller.ChaseRange && !controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			// Closest player is in chase range and currently not in a minigame.
			// Transition to CHASE.
			controller.TransitionToState(controller.chaseState);
		}
		else if (controller.LilGuy.health <= 0)
		{
			// The lil guy is dead.
			// Transition to DEAD
			controller.TransitionToState(controller.deadState);
		}
	}
}

public class ChaseState : AiState
{
	public ChaseState(AiController controller) : base(controller)
	{

	}
	public override void EnterState()
	{

	}

	public override void ExitState()
	{

	}

	public override void UpdateState()
	{

		if (controller.DistanceToPlayer() > controller.ChaseRange || controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			// Player is outside of chase range or they are currently in a minigame.
			// Transition to IDLE
			controller.TransitionToState(controller.idleState);
		}
		else if (controller.DistanceToPlayer() <= controller.AttackRange && !controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			// The lil guy is in attack range of the player and the player is not currently in a minigame.
			// Transition to ATTACK
			controller.TransitionToState(controller.attackState);
		}
		else if (controller.LilGuy.health <= 0)
		{
			// The lil guy is dead.
			// Transition to DEAD
			controller.TransitionToState(controller.deadState);
		}
		else
		{
			// Handle chasing player.
			ChasePlayer();
		}
	}

	/// <summary>
	/// Moves the AI towards the player's direction
	/// </summary>
	private void ChasePlayer()
	{
		controller.MoveDirection = (controller.Player.position - controller.transform.position).normalized;
		controller.transform.position = Vector3.MoveTowards(controller.transform.position, controller.Player.position, controller.GetComponent<LilGuyBase>().speed * Time.deltaTime);
	}
}

public class AttackState : AiState
{
	float attackTimer = 0;
	public AttackState(AiController controller) : base(controller)
	{

	}

	public override void EnterState()
	{

	}

	public override void ExitState()
	{

	}

	public override void UpdateState()
	{
		if (attackTimer > 0) attackTimer -= Time.deltaTime;	// Update attack cooldown timer if it's up.

		if (controller.DistanceToPlayer() > controller.AttackRange || controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			// The lil guy is outside of attack range or the player is currently in a minigame.
			// Transition to CHASE
			controller.TransitionToState(controller.chaseState);
		}
		else if (controller.LilGuy.health <= 0)
		{
			// The lil guy is dead.
			// Transition to DEAD
			controller.TransitionToState(controller.deadState);
		}
		else
		{
			// Handle attacking the player.
			AttackPlayer();
		}
	}

	/// <summary>
	/// Handles AI attack behaviour.
	/// </summary>
	public void AttackPlayer()
	{
		if (attackTimer <= 0 && !controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			// Currently not on an attack cooldown, and the player is currently not in a minigame, so attack them!
			controller.GetComponent<LilGuyBase>().Attack();
			attackTimer = controller.AttackBuffer;
		}
	}
}

public class DeadState : AiState
{
	public DeadState(AiController controller) : base(controller)
	{

	}

	public override void EnterState()
	{
		controller.ShowLastHitPrompt();
	}

	public override void ExitState()
	{

	}

	public override void UpdateState()
	{

	}
}