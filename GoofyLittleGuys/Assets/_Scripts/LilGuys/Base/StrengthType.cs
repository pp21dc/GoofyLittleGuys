using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthType : LilGuyBase
{
	[SerializeField] protected GameObject aoeShape;  // Only visible in editor and only used when aoeType is set to "Custom". 
	[SerializeField] public int aoeDamage = 1;



	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;   // Cooldown is up and there are no more charges available for usage.
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			base.StartChargingSpecial();

			cooldownTimer = cooldownDuration;
			chargeTimer = chargeRefreshRate;
			currentCharges--;
		}
	}

	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	}
	public override void Special()
	{		
			base.Special();			
	}
}
