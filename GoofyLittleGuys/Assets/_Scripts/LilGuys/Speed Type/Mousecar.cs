using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousecar : SpeedType
{
	[Header("Mousecar Specific")]
	[SerializeField] private float speedBoostAmount = 10f;
	[SerializeField] private float speedBoostDuration = 7f;

	private float endSpeedBoostTime = Mathf.Infinity;

	protected override void Update()
	{
		base.Update();
		if (Time.time > endSpeedBoostTime)
		{
			if (playerOwner != null) playerOwner.TeamSpeedBoost = 0;
			else speed -= speedBoostAmount;

			endSpeedBoostTime = Mathf.Infinity;
		}
	}
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
		if (playerOwner != null) playerOwner.TeamSpeedBoost = speedBoostAmount;
		else speed += speedBoostAmount;

		endSpeedBoostTime = Time.time + speedBoostDuration;
		base.Special();
	}
}
