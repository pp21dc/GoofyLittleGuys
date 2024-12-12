using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tricerabox : StrengthType
{
	[Header("Tricerabox Specific Parameters")]
	[SerializeField] private Transform waveAoePosition;
	[SerializeField] private float waveAoeDamage;
	[SerializeField] private float waveAoeLifetime;

	GameObject aoe;
	GameObject waveAoe;

	public void SpawnPunchAoe()
	{
		aoe = Instantiate(aoeShape, attackPosition);
		AoeHitbox hitbox = aoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamage = aoeDamage;
		hitbox.Init(gameObject);
		Destroy(aoe, aoeDestroyTime);
	}

	public void SpawnWaveAoe()
	{
		waveAoe = Instantiate(aoeShape, waveAoePosition);
		AoeHitbox hitbox = waveAoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamage = waveAoeDamage;
		waveAoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(waveAoe, waveAoeLifetime);

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
	protected override void OnEndSpecial()
	{
		if (aoe != null) Destroy(aoe);
		if (waveAoe != null) Destroy(waveAoe);
		base.OnEndSpecial();
	}
}
