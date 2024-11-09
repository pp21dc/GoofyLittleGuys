using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AiController : MonoBehaviour
{
	private Transform player;
	[SerializeField] private float chaseRange = 10f;
	[SerializeField] private float attackRange = 2f;
	[SerializeField] private float attackBuffer = 1f;


	private LilGuyBase lilGuy;
	private Vector3 moveDirection = Vector3.zero;

	public Transform Player { get { return player; } }
	public float ChaseRange { get { return chaseRange; } }
	public float AttackRange { get { return attackRange; } }
	public float AttackBuffer { get { return attackBuffer; } }
	public Vector3 MoveDirection { get { return moveDirection; } set { moveDirection = value; } }	
	public LilGuyBase LilGuy { get { return lilGuy; } }

	public AiState currentState;
	public IdleState idleState;
	public ChaseState chaseState;
	public AttackState attackState;
	public DeadState deadState;

	public float DistanceToPlayer()
	{
		return Vector3.Distance(transform.position, player.position);
	}

	public void TransitionToState(AiState newState)
	{
		currentState?.ExitState();  // If current state isn't null, exit the current state
		currentState = newState;
		currentState.EnterState();
	}

	public void ShowLastHitPrompt()
	{
		// Get the player who last hit this AI
		GameObject lastHitPlayerObj = GetComponent<Hurtbox>().lastHit;

		if (lastHitPlayerObj != null)
		{
			PlayerBody playerBody = lastHitPlayerObj.GetComponent<PlayerBody>();
			if (playerBody != null)
			{
				playerBody.ShowLastHitPrompt(GetComponent<LilGuyBase>()); // Show the UI prompt on their screen
				playerBody.EnableUIControl();   // Transfer control to UI
			}
		}
	}

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
			controller.TransitionToState(controller.chaseState);
		}
		else if (controller.LilGuy.health <= 0)
		{
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
		float distanceToPlayer = controller.DistanceToPlayer();
		if (distanceToPlayer > controller.ChaseRange || controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			controller.TransitionToState(controller.idleState);
		}
		else if (distanceToPlayer <= controller.AttackRange && !controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			controller.TransitionToState(controller.attackState);
		}
		else if (controller.LilGuy.health <= 0)
		{
			controller.TransitionToState(controller.deadState);
		}
		else
		{
			ChasePlayer();
		}
	}

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
		if (attackTimer > 0) attackTimer -= Time.deltaTime;
		float distanceToPlayer = controller.DistanceToPlayer();
		if (distanceToPlayer > controller.AttackRange && !controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
			controller.TransitionToState(controller.chaseState);
		}
		else if (controller.LilGuy.health <= 0)
		{
			controller.TransitionToState(controller.deadState);
		}
		else
		{
			AttackPlayer();
		}
	}

	public void AttackPlayer()
	{
		if (attackTimer <= 0 && !controller.Player.GetComponent<PlayerBody>().InMinigame)
		{
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