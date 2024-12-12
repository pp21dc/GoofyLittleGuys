using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spricket : SpeedType
{
	[Header("Spricket Specific")]
	[SerializeField] private float minChargeTime = 0.5f;
	[SerializeField] private float maxChargeTime = 1.5f;
	[SerializeField] private float dashDistance = 5f;

	bool isCharging = false;
	public bool IsCharging => isCharging;
	private float chargeTime = 0f;



	protected override void Update()
	{
		base.Update();
		if (playerOwner != null) movementDirection = playerOwner.MovementDirection;
		if (isCharging)
		{
			chargeTime += Time.deltaTime;
			if (chargeTime > minChargeTime) chargeTime = maxChargeTime;
		}
	}
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			if (!isCharging)
			{
				isCharging = true;
				chargeTime = 0;
			}

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
		if (chargeTime >= minChargeTime)
		{
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
		Special();
	}
	public override void Special()
    {
		// Decrement charges and reset cooldowns
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
		anim.SetTrigger("EndCharge");
        anim.ResetTrigger("SpecialAttack");
        Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
		Debug.Log(rb);
		if (rb != null)
		{
			float finalDistance = (dashDistance * speed) + ((chargeTime - minChargeTime) * 2);
			if (playerOwner != null) playerOwner.StartDash();
			else isDashing = true;
			// Apply force to initiate the dash
			rb.AddForce(movementDirection * finalDistance, ForceMode.Impulse);

			// Reset charging variables
			chargeTime = 0;
			isCharging = false;

			// Calculate the duration of the dash
			float dashDuration = finalDistance / rb.velocity.magnitude;

			// Start coroutine to stop dash after the duration
			StartCoroutine(StopDashAfterDuration(rb, dashDuration));
		}
	}

	private IEnumerator StopDashAfterDuration(Rigidbody rb, float duration)
	{
		yield return new WaitForSeconds(0.25f);

		// Call StopDash when the dash is complete
		if (playerOwner != null)
		{
			playerOwner.StopDash();
		}
		else isDashing = false;
        anim.SetTrigger("SpecialAttackEnded");
        anim.ResetTrigger("EndCharge");
        rb.velocity = Vector3.zero;
	}

	public override void MoveLilGuy()
	{
		if (!isDashing)
		{
			base.MoveLilGuy();
		}
	}
}
