using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthType : LilGuyBase
{
	[SerializeField] protected GameObject aoeShape;  // Only visible in editor and only used when aoeType is set to "Custom". 
	[SerializeField] private float aoeMaxSize = 1;
	[SerializeField] private float aoeExpansionSpeed = 1;
	[SerializeField] public int aoeDamage = 1;

	public StrengthType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}


	public override void Special()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;   // Cooldown is up and there are no more charges available for usage.
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			base.Special();
			


			// Decrement charges and reset cooldowns
			cooldownTimer = cooldownDuration;
			chargeTimer = chargeRefreshRate;
			currentCharges--;
		}
	}
}
