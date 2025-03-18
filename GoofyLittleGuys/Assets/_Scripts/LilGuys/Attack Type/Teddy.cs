using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Teddy : StrengthType
{
	[SerializeField] private GameObject specialFXPrefab;
	[SerializeField] private float slowAmount = 3f;
	[SerializeField] private float slowDuration = 1f;
	private GameObject instantiatedAoe = null;
	protected void SpawnConeAoe()
	{
		Debug.LogWarning("I Actually spawned a hitbox");
		instantiatedAoe = Instantiate(aoeShape, attackPosition);
		AoeHitbox hitbox = instantiatedAoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamageMultiplier = aoeDamageMultiplier;
		hitbox.Init(gameObject);
		hitbox.SlowAmount = slowAmount;
		hitbox.SlowDuration = slowDuration;
		Destroy(instantiatedAoe, aoeDestroyTime);
	}

	protected void DestroyConeAoe()
	{
		if (instantiatedAoe != null) Destroy(instantiatedAoe);
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

	protected override IEnumerator EndSpecial(bool stopImmediate = false)
	{
		if (instantiatedAoe != null) Destroy(instantiatedAoe);
		return base.EndSpecial(stopImmediate);
	}
}
