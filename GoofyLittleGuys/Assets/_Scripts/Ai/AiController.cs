using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AiController : MonoBehaviour
{
	private Transform player;
	[SerializeField] private float chaseRange = 10f;
	[SerializeField] private float attackRange = 2f;

	private LilGuyBase lilGuy;

	public Transform Player { get { return player; } }
	public float ChaseRange { get { return chaseRange; } }
	public float AttackRange { get { return attackRange; } }
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
		player = FindClosestPlayer();
		currentState.UpdateState();
	}

	private Transform FindClosestPlayer()
	{
		Transform currClosest = PlayerInput.all[0].transform;
		foreach (PlayerInput input in PlayerInput.all)
		{
			if (Vector3.Distance(input.transform.position, transform.position) < Vector3.Distance(currClosest.transform.position, transform.position))
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
		if (controller.DistanceToPlayer() <= controller.ChaseRange)
		{
			controller.TransitionToState(controller.chaseState);
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
		if (distanceToPlayer > controller.ChaseRange)
		{
			controller.TransitionToState(controller.idleState);
		}
		else if (distanceToPlayer <= controller.AttackRange)
		{
			controller.TransitionToState(controller.attackState);
		}
		else
		{
			ChasePlayer();
		}
	}

	private void ChasePlayer()
	{
		controller.transform.position = Vector3.MoveTowards(controller.transform.position, controller.Player.position, controller.GetComponent<LilGuyBase>().speed * Time.deltaTime);
	}
}

public class AttackState : AiState
{
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
		float distanceToPlayer = controller.DistanceToPlayer();
		if (distanceToPlayer > controller.AttackRange)
		{
			controller.TransitionToState(controller.chaseState);
		}
		else
		{
			AttackPlayer();
		}
	}
	public void AttackPlayer()
	{
		controller.GetComponent<LilGuyBase>().Attack();
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