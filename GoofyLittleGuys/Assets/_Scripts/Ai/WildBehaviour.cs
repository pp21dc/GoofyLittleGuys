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

	[SerializeField] private float timeBeforeDestroyed = 5f;  // Time until the gameobject is destroyed

	private GameObject instantiatedPlayerRangeIndicator;
	private AiController controller;
	private Coroutine actionCoroutine = null;
	private float attackTime = 0;

	private void Start()
	{
		controller = GetComponent<AiController>();
	}

	// Idle, Chase, Attack, Special, Death
	private void Update()
	{
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
		StopCoroutine(actionCoroutine);
		actionCoroutine = null;

		if (controller == null) return;
		controller.ToggleInteractCanvas(false);
		controller.LilGuy.RB.isKinematic = false;
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
			controller.LilGuy.MovementDirection = (controller.FollowPosition.position - controller.transform.position).normalized;
			if (controller.LilGuy is SpeedType && controller.LilGuy.CurrentCharges > 0 && controller.LilGuy.CooldownTimer <= 0)
			{
				Debug.Log("Special!");
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
	private void OnDestroy()
	{
		if (instantiatedPlayerRangeIndicator != null) Destroy(instantiatedPlayerRangeIndicator);
	}
}