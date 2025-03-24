using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spricket : SpeedType
{
	[Header("Spricket Specific")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject knockbackPrefab;
	[ColoredGroup][SerializeField] private Color chargeEffectColour;
	[ColoredGroup][SerializeField, Tooltip("Should the speed stat affect max distance?")] private bool applySpeedStat = false;
	[ColoredGroup][SerializeField] private float minChargeTime = 0.5f;
	[ColoredGroup][SerializeField] private float maxChargeTime = 1.5f;
	[ColoredGroup][SerializeField, Tooltip("Minimum distance Spricket dash can travel.")] private float minDashDistance = 10f;
	[ColoredGroup][SerializeField, Tooltip("Speed of which Spricket travels x distance.")] private float dashSpeed = 10f;
	[ColoredGroup][SerializeField] private float knockbackForceAmount = 60f;
	[ColoredGroup][SerializeField] private float knockbackDuration = 1f;

	[Header("Dash/Speed Curve Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField, Tooltip("Base max distance Spricket dash can travel.")] private float baseMaxDistance = 40f;
	[ColoredGroup][SerializeField, Tooltip("Maximum distance Spricket dash can travel after speed stat is applied.")] private float distanceThreshold = 100f;
	[ColoredGroup][SerializeField, Tooltip("How much the speed stat influences max distance.")] private float speedStatInfluence = 1;
	[ColoredGroup][SerializeField, Tooltip("Speed stat number for when distance gained per point in speed starts to fall off.")] private float maxDistanceFalloff = 20;

	[Header("Special FX Specific")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float spawnInterval = 0.01f;
	[ColoredGroup][SerializeField] private int maxAfterimages = 12;
	[ColoredGroup][SerializeField] private float fadeSpeed = 0.5f;
	[ColoredGroup][SerializeField] private Color emissionColour = new Color(1.00f, 0.82f, 0.25f, 1.0f);   // The yellow used for speed lil guys


	private GameObject instantiatedKnockback;
	private GameObject chargeEffect;
	private Coroutine dashCoroutine;

	private float dashDamageMultiplier = 1f;
	private float maxDashDistance;
	private float chargeTime = 0f;
	private bool isCharging = false;

	public bool IsCharging => isCharging;

	protected override void Update()
	{
		base.Update();
		if (playerOwner != null) movementDirection = playerOwner.MovementDirection;
		if (isCharging)
		{
			chargeTime += Time.deltaTime;
			chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);
			if (chargeEffect != null) chargeEffect.transform.localScale = Vector3.one * (0.5f + 2 * chargeTime);
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
				chargeEffect = Instantiate(FXManager.Instance.GetEffect("ChargeUp"), transform.position, Quaternion.identity, transform);
				chargeEffect.GetComponent<ParticleSystem>().startColor = chargeEffectColour;
				PlaySound("Spricket_Special_In");
			}
			LockMovement = true;
			// Decrement charges and reset cooldowns
			cooldownTimer = cooldownDuration;
			chargeTimer = chargeRefreshRate;
			currentCharges--;


			anim.ResetTrigger("SpecialAttackEnded");
			anim.ResetTrigger("EndCharge");
			anim.ResetTrigger("SpecialAttack");
			anim.SetTrigger("SpecialAttack");
			if (playerOwner == null) StopChargingSpecial();
		}
	}
	public override void StopChargingSpecial()
	{
		if (!isCharging) return;
		PlaySound("Spricket_Special_Out");
		if (chargeTime >= minChargeTime)
		{
			LockMovement = false;
			isCharging = false;
			Special();
		}
		else
		{
			dashCoroutine = StartCoroutine(WaitForChargeCompletion());
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
		Destroy(chargeEffect);
		// Decrement charges and reset cooldowns
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
		if (playerOwner)
			EventManager.Instance.StartAbilityCooldown(playerOwner.PlayerUI, cooldownDuration);
		anim.SetTrigger("EndCharge");
		anim.ResetTrigger("SpecialAttack");
		ApplySpeedBoost(spawnInterval, maxAfterimages, fadeSpeed, emissionColour);
		LockMovement = false;
		Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
		if (rb != null)
		{
			instantiatedKnockback = Instantiate(knockbackPrefab, transform.position, Quaternion.identity, attackOrbit.transform);
			instantiatedKnockback.transform.localRotation = Quaternion.identity;
			KnockbackHitbox h = instantiatedKnockback.GetComponent<KnockbackHitbox>();
			if (h == null) h = instantiatedKnockback.GetComponentInChildren<KnockbackHitbox>();
			h.KnockbackForce = knockbackForceAmount;
			h.KnockbackDuration = knockbackDuration;

			AoeHitbox a = instantiatedKnockback.GetComponent<AoeHitbox>();
			if (a == null) a = instantiatedKnockback.GetComponentInChildren<AoeHitbox>();
			a.AoeDamageMultiplier = dashDamageMultiplier;
			a.Init(gameObject);
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
			dashCoroutine = StartCoroutine(StopDashAfterDuration(rb, dashDuration));
		}
	}

	private IEnumerator StopDashAfterDuration(Rigidbody rb, float duration)
	{
		//PlaySound("Spricket_Special_Soar");
		yield return new WaitForSeconds(duration);

		// Call StopDash when the dash is complete
		if (playerOwner != null)
		{
			StopLoopingSound();
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

	public override void CancelSpecial()
	{
		if (isCharging || isDashing)
		{
			Destroy(chargeEffect);
			// Reset special state
			isCharging = false;
			isDashing = false;
			LockMovement = false;

			// Check if the Animator is in the "InSpecial" state and handle accordingly
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("InSpecial"))
			{
				anim.SetTrigger("EndCharge"); // Trigger EndCharge to progress the animation properly
			}
			// Reset animations
			anim.SetTrigger("SpecialAttackEnded");


			// Stop any ongoing coroutines
			StopAllCoroutines(); // Ensure all related coroutines are stopped

			// Destroy the knockback prefab if it exists
			if (instantiatedKnockback != null)
			{
				Destroy(instantiatedKnockback);
			}

			// Reset Rigidbody velocity
			Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.velocity = Vector3.zero;
			}
		}
	}

	protected override IEnumerator EndSpecial(bool stopImmediate = false)
	{
		if (!stopImmediate)
		{
			if (specialDuration >= 0) yield return new WaitForSeconds(specialDuration);
			else if (specialDuration == -1)
			{
				AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
				if (clip != null) yield return new WaitForSeconds(clip.length);
			}
		}
		if (anim != null)
		{
			anim.SetTrigger("SpecialAttackEnded");
		}
		LockAttackRotation = false;
		LockMovement = false;

		if (isCharging || isDashing)
		{
			Destroy(chargeEffect);
			// Reset special state
			isCharging = false;
			isDashing = false;
			LockMovement = false;

			// Check if the Animator is in the "InSpecial" state and handle accordingly
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("InSpecial"))
			{
				anim.SetTrigger("EndCharge"); // Trigger EndCharge to progress the animation properly
			}
			// Reset animations
			anim.SetTrigger("SpecialAttackEnded");


			// Stop any ongoing coroutines
			StopAllCoroutines(); // Ensure all related coroutines are stopped

			// Destroy the knockback prefab if it exists
			if (instantiatedKnockback != null)
			{
				Destroy(instantiatedKnockback);
			}

			// Reset Rigidbody velocity
			Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.velocity = Vector3.zero;
			}
		}
	}
	public override void MoveLilGuy(float s = 1)
	{
		if (!isDashing)
		{
			base.MoveLilGuy(s);
		}
	}
}
