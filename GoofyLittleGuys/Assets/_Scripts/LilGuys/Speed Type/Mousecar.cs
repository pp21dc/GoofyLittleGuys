using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousecar : SpeedType
{
    [Header("Mousecar Specific")]
	[HorizontalRule]
	[ColoredGroup(0.37f, 0.28089f, 0)][SerializeField] private float speedBoostAmount = 10f;
	[ColoredGroup(0.37f, 0.28089f, 0)][SerializeField] private float speedBoostDuration = 7f;


    [Header("Special FX Specific")]
	[HorizontalRule]
	[ColoredGroup(0.37f, 0.28089f, 0)][SerializeField] private Color emissionColour = new Color(1.00f, 0.82f, 0.25f, 1.0f);   // The yellow used for speed lil guys
	[ColoredGroup(0.37f, 0.28089f, 0)][SerializeField] private float spawnInterval = 0.2f;
	[ColoredGroup(0.37f, 0.28089f, 0)][SerializeField] private float fadeSpeed = 0.5f;
	[ColoredGroup(0.37f, 0.28089f, 0)][SerializeField] private int maxAfterimages = 12;

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
			// Teammate Mousecar - apply to player team
			playerOwner.Buffs.AddBuff(BuffType.TeamSpeedBoost, speedBoostAmount, speedBoostDuration, this);

			// Apply visuals to all teammates
			foreach (LilGuyBase lilGuy in playerOwner.LilGuyTeam)
			{
				if (!lilGuy.isActiveAndEnabled) continue;
				lilGuy.ApplySpeedBoost(spawnInterval, maxAfterimages, fadeSpeed, emissionColour);
			}
		}
		else
		{
			// Wild Mousecar - apply to self only
			Buffs.AddBuff(BuffType.TeamSpeedBoost, speedBoostAmount, speedBoostDuration, this);
			ApplySpeedBoost(spawnInterval, maxAfterimages, fadeSpeed, emissionColour);
		}

		base.Special();
	}

}
