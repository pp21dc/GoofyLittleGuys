using Managers;
using System.Collections;
using UnityEngine;

public class SpeedType : LilGuyBase
{

	protected bool isDashing = false;
	public bool IsDashing => isDashing;

	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			base.StartChargingSpecial();
		}
	}
	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	}
	protected override void Special()
	{
		base.Special();
	}
}
