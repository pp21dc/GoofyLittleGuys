using Managers;
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
	[SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed
	[SerializeField] private float decelerationTime = 0.2f;  // Time to stop

	[SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed
	[SerializeField] private float interactDistance = 2f;  // Time until the gameobject is destroyed

	private GameObject instantiatedPlayerRangeIndicator;
	private AiController controller;
	private Coroutine actionCoroutine = null;
	private Vector3 moveDirection = Vector3.zero;
	private Vector3 currentVelocity = Vector3.zero;
	private float attackTime = 0;

	private void Start()
	{
		controller = GetComponent<AiController>();
	}

	// Idle, Chase, Attack, Special, Death
	private void Update()
	{
		if (currentVelocity.x > 0) controller.LilGuy.Flip = true;
		else if (currentVelocity.x < 0) controller.LilGuy.Flip = false;

		// Reset attack buffer on AI.
		if (attackTime > 0) attackTime -= Time.deltaTime;

		// AI behaviours
		if (controller.LilGuy.Health <= 0)
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

	public void OnDisable()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
		StopAllCoroutines();

		if (controller == null) return;
		controller.ToggleInteractCanvas(false);
		controller.RB.isKinematic = false;
	}


	/// <summary>
	/// State that handles when the AI is idle.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Idle()
	{
		controller.LilGuy.IsMoving = false;
		while (controller.DistanceToPlayer() > chaseRange && controller.LilGuy.Health > 0)
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
		controller.RB.isKinematic = true;
		controller.RB.velocity = Vector3.zero;

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
		while (controller.DistanceToPlayer() <= attackRange && controller.LilGuy.Health > 0)
		{
			if (controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && attackTime <= 0 && controller.LilGuy is StrengthType strengthLilGuy)
			{
				strengthLilGuy.Special();
				attackTime = attackBuffer;
			}
			else if (controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && attackTime <= 0 && controller.LilGuy is DefenseType defenseLilGuy && controller.LilGuy.Health * 2 <= controller.LilGuy.MaxHealth)
			{
				defenseLilGuy.Special();

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
		while (controller.DistanceToPlayer() > attackRange && controller.DistanceToPlayer() <= chaseRange && controller.LilGuy.Health > 0)
		{
			Vector3 directionToPlayer = (controller.FollowPosition.position - controller.transform.position).normalized;
			if (controller.LilGuy is SpeedType speedLilGuy && controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0 && attackTime <= 0)
			{
				speedLilGuy.DashDirection = directionToPlayer;
				speedLilGuy.Special();
			}
			else
			{
				// Move the creature towards the player with smoothing
				Vector3 targetVelocity = directionToPlayer.normalized * (controller.LilGuy.Speed / 3f);
				// Smoothly accelerate towards the target velocity
				currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime / accelerationTime);
				// Apply the smoothed velocity to the Rigidbody
				controller.RB.velocity = new Vector3(currentVelocity.x, controller.RB.velocity.y, currentVelocity.z);
			}
			yield return null;
		}
		actionCoroutine = null;
	}
	private void OnDestroy()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
	}
}