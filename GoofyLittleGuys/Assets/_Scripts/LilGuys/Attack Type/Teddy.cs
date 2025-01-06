using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teddy : StrengthType
{
	[SerializeField] private GameObject specialFXPrefab;
	private GameObject instantiatedAoe = null;
	protected void SpawnConeAoe()
	{
		instantiatedAoe = Instantiate(aoeShape, attackPosition);
		instantiatedAoe.GetComponent<AoeHitbox>().AoeDamageMultiplier = aoeDamageMultiplier;
		instantiatedAoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(instantiatedAoe, aoeDestroyTime);
	}

	protected void SpawnThrustEffect()
	{
		Instantiate(specialFXPrefab, attackPosition.position, Quaternion.Euler(attackOrbit.rotation.eulerAngles.y, attackPosition.rotation.eulerAngles.y, 0));
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
