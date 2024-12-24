using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teddy : StrengthType
{
	private GameObject instantiatedAoe = null;
	protected void SpawnConeAoe()
	{
		instantiatedAoe = Instantiate(aoeShape, attackPosition);
		instantiatedAoe.GetComponent<AoeHitbox>().AoeDamageMultiplier = aoeDamageMultiplier;
		instantiatedAoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(instantiatedAoe, aoeDestroyTime);
	}
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		base.StartChargingSpecial();
		LockAttackRotation = true;
	}
	protected override void OnEndSpecial()
	{
		//if (instantiatedAoe != null) Destroy(instantiatedAoe);
		base.OnEndSpecial();
	}
}
