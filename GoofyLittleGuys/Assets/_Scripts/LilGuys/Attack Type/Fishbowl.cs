using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishbowl : StrengthType
{
	[Header("Fishbowl Specific")]
	[SerializeField] private float minChargeTime = 0.5f;
	[SerializeField] private float maxChargeTime = 1.5f;
	[SerializeField] private float aoeMaxSize = 5f;
	[SerializeField] private float waveDestroyTime = 2f;
	[SerializeField] private float waveMoveSpeed = 10f;
	[SerializeField] private float minKnockback = 10f;
	[SerializeField] private float maxKnockback = 25f;

	bool isCharging = false;
	private float chargeTime = 0f;
	private GameObject instantiatedAoe = null;

	protected override void Update()
	{
		base.Update();
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

	protected override void Special()
	{
		LockMovement = true;	
		base.Special();
		isCharging = false;
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
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
	public void SpawnWaveAoe()
	{
		instantiatedAoe = Instantiate(aoeShape, transform.position, Quaternion.identity);
		instantiatedAoe.GetComponent<FishbowlWaves>().Init(specialDuration, chargeTime, maxChargeTime, minChargeTime);
		foreach(Transform child in  instantiatedAoe.transform)
		{
			child.GetComponent<AoeHitbox>().AoeDamageMultiplier = aoeDamageMultiplier;
			child.GetComponent<AoeMovement>().Speed = waveMoveSpeed;
			child.GetComponent<AoeHitbox>().Init(gameObject);
			child.GetComponent<KnockbackHitbox>().KnockbackForce = Mathf.Lerp(minKnockback, maxKnockback, (chargeTime - minChargeTime)/(maxChargeTime - minChargeTime));

		}
	}
}
