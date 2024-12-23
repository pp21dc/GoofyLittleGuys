using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousecar : SpeedType
{
	[Header("Mousecar Specific")]
	[SerializeField] private float speedBoostAmount = 10f;
	[SerializeField] private float speedBoostDuration = 7f;

	private float endSpeedBoostTime = Mathf.Infinity;

	public override void StartChargingSpecial()
	{
		base.StartChargingSpecial();
	}
	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	}
	public override void Special()
	{
		if (playerOwner != null) playerOwner.TeamSpeedBoost += speedBoostAmount;
		else speed += speedBoostAmount;

		StartCoroutine(StopSpeedBoost(playerOwner != null));
		base.Special();
	}

	private IEnumerator StopSpeedBoost(bool playerOwned)
	{
		yield return new WaitForSeconds(speedBoostDuration);
		if (!playerOwned) speed -= speedBoostAmount;
		else playerOwner.TeamSpeedBoost -= speedBoostAmount;
	}
}
