using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AiController))]
public class WildBehaviour : MonoBehaviour
{
	[SerializeField] private float chaseRange = 10f;
	[SerializeField] private float attackRange = 1f;
	[SerializeField] private float attackBuffer = 2f;
	[SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed
	[SerializeField] private float decelerationTime = 0.2f;  // Time to stop

	[SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed
	[SerializeField] private float interactDistance = 2f;  // Time until the gameobject is destroyed


	private AiController controller;
	private Coroutine actionCoroutine = null;
	private Vector3 moveDirection = Vector3.zero;
	private float attackTime = 0;

	private void Start()
	{
		controller = GetComponent<AiController>();
	}

	// Idle, Chase, Attack, Special, Death
	private void Update()
	{
		if (moveDirection.x > 0) controller.LilGuy.Flip = true;
		else if (moveDirection.x < 0) controller.LilGuy.Flip = false;

		// Reset attack buffer on AI.
		if (attackTime > 0) attackTime -= Time.deltaTime;

		// AI behaviours
		if (controller.LilGuy.health <= 0)
		{
			actionCoroutine ??= StartCoroutine(Dead());
		}
		if (controller.DistanceToPlayer() <= attackRange)
		{
			actionCoroutine ??= StartCoroutine(AttackPlayer());
		}
		else if (controller.DistanceToPlayer() <= chaseRange)
		{
			actionCoroutine ??= StartCoroutine(ChasePlayer());
		}
		else
		{
			actionCoroutine ??= StartCoroutine(Idle());
		}
	}


	/// <summary>
	/// State that handles when the AI is idle.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Idle()
	{
		while (controller.DistanceToPlayer() > chaseRange && controller.LilGuy.health > 0)
		{
			yield return null;
		}
		actionCoroutine = null;
	}


	/// <summary>
	/// State that handles when the AI dies.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Dead()
	{
		controller.LilGuy.IsMoving = false;
		controller.LilGuy.OnDeath();

		float currTime = 0;
		while (currTime < timeBeforeDestroyed)
		{
			currTime += Time.deltaTime;
			if (controller.DistanceToPlayer() <= interactDistance) controller.ToggleInteractCanvas(true);
			else controller.ToggleInteractCanvas(false);
			yield return null;
		}
		Destroy(gameObject);
		actionCoroutine = null;
	}

	/// <summary>
	/// State that handles attacking the player.
	/// </summary>
	/// <returns></returns>
	private IEnumerator AttackPlayer()
	{
		while (controller.DistanceToPlayer() <= attackRange && controller.LilGuy.health > 0)
		{
			if (controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && attackTime <= 0)
			{
				if (controller.LilGuy is StrengthType strengthLilGuy) strengthLilGuy.Special();
				else if (controller.LilGuy is DefenseType defenseLilGuy && controller.LilGuy.health * 2 <= controller.LilGuy.maxHealth) defenseLilGuy.Special();

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
		controller.LilGuy.IsMoving = true;
		while (controller.DistanceToPlayer() > attackRange && controller.LilGuy.health > 0)
		{
			Vector3 directionToPlayer = (controller.Player.position - controller.transform.position).normalized;

			// Calculate the horizontal distance to the player
			float horizontalDistance = Vector3.Distance(
				new Vector3(controller.transform.position.x, 0, controller.transform.position.z),
				new Vector3(controller.Player.position.x, 0, controller.Player.position.z)
			);

			// Smooth acceleration towards the player
			moveDirection = Vector3.Lerp(
				moveDirection,
				directionToPlayer,
				Time.deltaTime / accelerationTime
			);

			// Move the creature towards the player with smoothing
			controller.transform.position += moveDirection * controller.LilGuy.speed * Time.deltaTime;


			yield return null;
		}
		actionCoroutine = null;
	}
}