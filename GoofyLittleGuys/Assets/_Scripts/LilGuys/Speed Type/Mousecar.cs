using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousecar : SpeedType
{
	[Header("Mousecar Specific")]
	[SerializeField] private float speedBoostAmount = 10f;
	[SerializeField] private float speedBoostDuration = 7f;

	public override void StartChargingSpecial()
	{
		base.StartChargingSpecial();
	}
	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	}
	protected override void Special()
	{
		if (playerOwner != null)
		{
			playerOwner.TeamSpeedBoost += speedBoostAmount;
			foreach (LilGuyBase lilGuy in playerOwner.LilGuyTeam)
			{
				if (!lilGuy.isActiveAndEnabled) continue;
				lilGuy.ApplySpeedBoost(0.2f);
			}
		}
		else
		{
			speed += speedBoostAmount;
			ApplySpeedBoost(0.2f);
		}

		StartCoroutine(StopSpeedBoost(playerOwner != null));
		base.Special();
	}

	private IEnumerator StopSpeedBoost(bool playerOwned)
	{
		yield return new WaitForSeconds(speedBoostDuration);
		if (!playerOwned)
		{
			speed -= speedBoostAmount;
			RemoveSpeedBoost();
		}
		else
		{
			playerOwner.TeamSpeedBoost -= speedBoostAmount; 
			foreach (LilGuyBase lilGuy in playerOwner.LilGuyTeam)
			{
				lilGuy.RemoveSpeedBoost();
			}
		}
	}
}
