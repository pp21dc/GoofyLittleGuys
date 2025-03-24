using System.Collections;
using UnityEngine;

public class Teddy : StrengthType
{
	[Header("Teddy Specific")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject specialFXPrefab;
	[ColoredGroup][SerializeField] private float slowAmount = 3f;
	[ColoredGroup][SerializeField] private float slowDuration = 1f;

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
