using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armordillo : DefenseType
{
	[Header("Armordillo Specific")]
	[SerializeField] private float speedBoost = 10f;

	private float speedBoostTime;
	bool speedBoostActive = false;
	public override void StartChargingSpecial()
	{
		if (!speedBoostActive)
		{
			speed += speedBoost;
		}

		if (playerOwner != null) playerOwner.MaxSpeed = speed;
		speedBoostActive = true;
		base.StartChargingSpecial();
	}
	public override void Special()
	{
		base.Special();
		speedBoostTime = Time.time + specialDuration;
	}

	protected override void Update()
	{
		base.Update();
		if (speedBoostActive && Time.time > speedBoostTime)
		{
			speed -= speedBoost;
			if (playerOwner != null) playerOwner.MaxSpeed = speed;
			speedBoostActive = false;
		}
	}
}
