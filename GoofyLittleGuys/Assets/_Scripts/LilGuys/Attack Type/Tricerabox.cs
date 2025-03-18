using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tricerabox : StrengthType
{
	[Header("Tricerabox Specific")]
	[SerializeField] private GameObject specialFXPrefab;
	[SerializeField] private GameObject waveAoePrefab;
	[SerializeField] private float waveAoeDamageMultiplier;
	[SerializeField] private float waveAoeLifetime;
	[SerializeField] private float waveSpeed;
	[SerializeField] private float slowAmount = 10f;
	[SerializeField] private float slowDuration = 1f;

	GameObject aoe;
	GameObject waveAoe;

	public void SpawnPunchAoe()
	{
		aoe = Instantiate(aoeShape, attackPosition);
		AoeHitbox hitbox = aoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamageMultiplier = aoeDamageMultiplier;
		hitbox.SlowAmount = slowAmount;
		hitbox.SlowDuration = slowDuration;
		hitbox.Init(gameObject);
		Destroy(aoe, aoeDestroyTime);
	}

	public void SpawnWaveAoe()
	{
		waveAoe = Instantiate(waveAoePrefab, attackPosition.position + Vector3.up, attackOrbit.rotation);
		waveAoe.layer = gameObject.layer;
		waveAoe.GetComponent<AoeMovement>().Speed = waveSpeed;
		AoeHitbox hitbox = waveAoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamageMultiplier = aoeDamageMultiplier;
		hitbox.SlowAmount = slowAmount;
		hitbox.SlowDuration = slowDuration;
		waveAoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(waveAoe, waveAoeLifetime);

	}

	protected void SpawnThrustEffect()
	{
		Instantiate(specialFXPrefab, attackPosition.position, Quaternion.Euler(attackOrbit.rotation.eulerAngles.y, attackPosition.rotation.eulerAngles.y, 0));
	}

	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		LockAttackRotation = true;
		base.StartChargingSpecial();
	}
	public override void PlayDeathAnim(bool isWild = false)
	{
		if (waveAoe != null) Destroy(waveAoe);
		base.PlayDeathAnim(isWild);

	}
	protected override IEnumerator EndSpecial(bool stopImmediate = false)
	{
		if (aoe != null) Destroy(aoe);
		if (waveAoe != null) Destroy(waveAoe);
		return base.EndSpecial(stopImmediate);
	}
}
