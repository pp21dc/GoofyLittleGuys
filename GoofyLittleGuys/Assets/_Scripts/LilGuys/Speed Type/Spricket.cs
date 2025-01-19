using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spricket : SpeedType
{
	[Header("Spricket Specific")]
	[SerializeField] private GameObject knockbackPrefab;
	[SerializeField] private float minChargeTime = 0.5f;
	[SerializeField] private float maxChargeTime = 1.5f;
	[SerializeField, Tooltip("Minimum distance Spricket dash can travel.")] private float minDashDistance = 10f;
	[SerializeField, Tooltip("Speed of which Spricket travels x distance.")] private float dashSpeed = 10f;
	[SerializeField, Tooltip("Should the speed stat affect max distance?")] private bool applySpeedStat = false;
	[SerializeField] private float knockbackForce = 100f;
	[SerializeField] private float knockbackDuration = 1f;

	[Header("Dash/Speed Curve Settings")]
	[SerializeField, Tooltip("Base max distance Spricket dash can travel.")] private float baseMaxDistance = 40f;
	[SerializeField, Tooltip("Maximum distance Spricket dash can travel after speed stat is applied.")] private float distanceThreshold = 100f;
	[SerializeField, Tooltip("How much the speed stat influences max distance.")] private float speedStatInfluence = 1;
	[SerializeField, Tooltip("Speed stat number for when distance gained per point in speed starts to fall off.")] private float maxDistanceFalloff = 20;
	[Header("Special FX Specific")]
	[SerializeField] private float spawnInterval = 0.01f;
	[SerializeField] private int maxAfterimages = 12;
	[SerializeField] private float fadeSpeed = 0.5f;
	[SerializeField] private Color emissionColour = new Color(1.00f, 0.82f, 0.25f, 1.0f);   // The yellow used for speed lil guys

	bool isCharging = false;

	private float maxDashDistance;
	public bool IsCharging => isCharging;
	private float chargeTime = 0f;
	private GameObject instantiatedKnockback;
	protected override void Update()
	{
		base.Update();
		if (playerOwner != null) movementDirection = playerOwner.MovementDirection;
		if (isCharging)
		{
			chargeTime += Time.deltaTime;
			chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);
			if (chargeTime >= maxChargeTime)
			{
				StopChargingSpecial();
			}
		}
	}
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			float speedInfluence = Mathf.Pow(speed, speedStatInfluence);
			maxDashDistance = baseMaxDistance + (distanceThreshold - baseMaxDistance) * (speedInfluence / (speedInfluence + maxDistanceFalloff));
			if (!isCharging)
			{
				isCharging = true;
				chargeTime = 0;
			}

			LockMovement = true;
			// Decrement charges and reset cooldowns
			cooldownTimer = cooldownDuration;
			chargeTimer = chargeRefreshRate;
			currentCharges--;


			anim.ResetTrigger("SpecialAttackEnded");
			anim.ResetTrigger("EndCharge");
			anim.SetTrigger("SpecialAttack");
			if (playerOwner == null) StopChargingSpecial();
		}
	}
	public override void StopChargingSpecial()
	{
		if (!isCharging) return;
		if (chargeTime >= minChargeTime)
		{
			LockMovement = false;
			isCharging = false;
			Special();
		}
		else
		{
			StartCoroutine(WaitForChargeCompletion());
		}
	}

	private IEnumerator WaitForChargeCompletion()
	{
		while (chargeTime < minChargeTime)
		{
			yield return null;
		}
		isCharging = false;
		Special();
	}
	protected override void Special()
	{
		// Decrement charges and reset cooldowns
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
		anim.SetTrigger("EndCharge");
		anim.ResetTrigger("SpecialAttack");
		ApplySpeedBoost(spawnInterval, maxAfterimages, fadeSpeed, emissionColour);
		LockMovement = false;
		Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
		Debug.Log(rb);
		if (rb != null)
		{
			instantiatedKnockback = Instantiate(knockbackPrefab, transform.position, Quaternion.identity, transform);
			KnockbackHitbox h = instantiatedKnockback.GetComponent<KnockbackHitbox>();
			h.KnockbackForce = knockbackForce;
			h.KnockbackDuration = knockbackDuration;
			float finalDistance = (Mathf.Lerp(minDashDistance, (applySpeedStat ? maxDashDistance : baseMaxDistance), (chargeTime - minChargeTime) / (maxChargeTime - minChargeTime)));
			if (playerOwner != null) playerOwner.StartDash();
			else isDashing = true;
			// Apply force to initiate the dash
			rb.velocity = movementDirection.normalized * dashSpeed;

			// Reset charging variables
			chargeTime = 0;
			isCharging = false;

			// Calculate the duration of the dash
			float dashDuration = finalDistance / dashSpeed;

			// Start coroutine to stop dash after the duration
			StartCoroutine(StopDashAfterDuration(rb, dashDuration));
		}
	}

	private IEnumerator StopDashAfterDuration(Rigidbody rb, float duration)
	{
		yield return new WaitForSeconds(duration);

		// Call StopDash when the dash is complete
		if (playerOwner != null)
		{
			playerOwner.StopDash();
		}
		else isDashing = false;
		anim.SetTrigger("SpecialAttackEnded");
		anim.ResetTrigger("EndCharge");
		RemoveSpeedBoost();
		rb.velocity = Vector3.zero;
		LockMovement = false;
		Destroy(instantiatedKnockback);
	}

	public override void MoveLilGuy()
	{
		if (!isDashing)
		{
			base.MoveLilGuy();
		}
	}
}
