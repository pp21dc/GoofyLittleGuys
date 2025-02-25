using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Phantaphant : SpeedType
{
	[Header("Phant-a-phant Specific")]
	[SerializeField, Tooltip("The maximum range that a target to dash to can be picked.")] private float dashTargetRange;
	[SerializeField] private float slowAmount = 20f;
	[SerializeField] private float slowDuration = 2f;
	private Transform targetPosition;
	private Vector3 directionToTarget;
	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}

	public void EndAfterDeathAnimation()
	{
		if (playerOwner == null) return;
		anim.SetTrigger("EndAfterDeath");
	}

	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			LocateClosestTarget();
			StopChargingSpecial();
		}
	}

	protected override void Special()
	{
		anim.ResetTrigger("SpecialAttackEnded");
		Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();

		if (rb != null && targetPosition != null)
		{
			// Get the latest position of the target right before teleporting
			Vector3 latestTargetPosition = targetPosition.position;

			// Instantly move Phant to the target’s last known position
			rb.MovePosition(latestTargetPosition);
			LilGuyBase targLilGuy = targetPosition.GetComponent<LilGuyBase>();
			Instantiate(FXManager.Instance.GetEffect("PhantaphantTeleport"), targLilGuy.transform.position, Quaternion.identity, targLilGuy.transform);
		}

		GameObject slowedEntity;
		PlayerBody body = targetPosition.GetComponent<PlayerBody>();
		LilGuyBase lilGuy = targetPosition.GetComponent<LilGuyBase>();

		if (body != null)
		{
			slowedEntity = body.gameObject;
		}
		else if (lilGuy != null)
		{
			slowedEntity = lilGuy.gameObject;
		}
		else return;

		EventManager.Instance.ApplyDebuff(slowedEntity, slowAmount, slowDuration, DebuffType.Slow);
		OnEndSpecial();
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
		targetPosition = null;
	}
	public override void StopChargingSpecial()
	{
		if (targetPosition == null) return;
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
		anim.ResetTrigger("SpecialAttackEnded");
		anim.SetTrigger("SpecialAttack");
		Instantiate(FXManager.Instance.GetEffect("PhantaphantTeleport"), transform.position, Quaternion.identity, transform);
		StartCoroutine(WaitForEndOfSpecial());
	}

	private IEnumerator WaitForEndOfSpecial()
	{
		AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "InSpecial");
		if (clip != null) yield return new WaitForSeconds(clip.length);
		Special();
	}

	private void LocateClosestTarget()
	{
		Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, dashTargetRange,
			playerOwner != null ? LayerMask.GetMask("WildLilGuys", "PlayerLilGuys") : LayerMask.GetMask("PlayerLilGuys"));

		float closestDistance = float.MaxValue;
		Transform closestTarget = null;
		Vector3 closestTargetVelocity = Vector3.zero;

		foreach (Collider collider in nearbyColliders)
		{
			if (collider.transform == transform) continue;
			LilGuyBase lilGuy = collider.GetComponent<LilGuyBase>();
			if (lilGuy == null || lilGuy.Health <= 0) continue;
			if (closestTarget != null && (closestTarget.GetComponent<LilGuyBase>().PlayerOwner != null && lilGuy.PlayerOwner == null)) continue;	// Prioritizes players possibly.
			float distance = Vector3.Distance(collider.transform.position, transform.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestTarget = collider.transform;

				// Get the most recent velocity
				Rigidbody rb = lilGuy.GetComponent<Rigidbody>();
				if (rb != null)
				{
					closestTargetVelocity = rb.velocity; // Get the real-time velocity from Rigidbody
				}
				else
				{
					// If no Rigidbody, use stored movement direction
					closestTargetVelocity = lilGuy.PlayerOwner != null ? lilGuy.PlayerOwner.CurrentVelocity : lilGuy.CurrentVelocity;
				}
			}
		}

		if (closestTarget != null)
		{
			targetPosition = closestTarget;
			AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "InSpecial");

			float teleportTime = clip.length;

			// Ensure velocity is non-zero before using it for prediction
			if (closestTargetVelocity.magnitude < 0.1f)
			{
				closestTargetVelocity = Vector3.zero; // Avoid bad predictions when the target is not moving
			}

			directionToTarget = PredictFuturePosition(closestTarget.position, closestTargetVelocity, teleportTime);
		}
	}

	#region Old Prediction Code (Remove in Future if Current is better)
	private Vector3 GetValidTeleportPosition(Vector3 predictedPosition)
	{
		Vector3 startPos = transform.position;
		Vector3 direction = (predictedPosition - startPos).normalized;
		float distance = Vector3.Distance(startPos, predictedPosition);

		RaycastHit hit;

		// Raycast to check if there's an obstacle between current position and predicted position
		if (Physics.Raycast(startPos, direction, out hit, distance, LayerMask.GetMask("PitColliders")))
		{
			// If there's an obstacle, adjust the teleport position to just before the hit point
			return hit.point - (direction * 1.5f); // Adjusts to be slightly before the wall
		}

		// No obstacles, return the predicted position
		return predictedPosition;
	}

	private Vector3 PredictFuturePosition(Vector3 currentPos, Vector3 velocity, float timeAhead)
	{
		Vector3 futurePos = currentPos + (velocity * timeAhead);
		return futurePos;
	}
	#endregion
}
