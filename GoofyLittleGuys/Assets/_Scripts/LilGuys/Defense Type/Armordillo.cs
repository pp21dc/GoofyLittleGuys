using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armordillo : DefenseType
{
	[Header("Armordillo Specific")]
	[SerializeField] private float speedBoost = 30f;

	private float speedBoostTime;
	bool speedBoostActive = false;
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (speedBoostActive) return;
		speed += speedBoost;
		StartCoroutine(StopSpeedBoost(playerOwner != null));
		speedBoostActive = true;
	}
	public override void Special()
	{
		base.Special();
	}
	private IEnumerator StopSpeedBoost(bool playerOwned)
	{
		yield return new WaitForSeconds(specialDuration);
		speed -= speedBoost;
		speedBoostActive = false;
	}
}
