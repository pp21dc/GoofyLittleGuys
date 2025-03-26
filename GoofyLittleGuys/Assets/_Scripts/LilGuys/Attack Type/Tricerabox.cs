using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tricerabox : StrengthType
{
	[Header("Tricera-box Specific")]
	[HorizontalRule]
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private GameObject specialFXPrefab;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private GameObject waveAoePrefab;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float waveAoeDamageMultiplier;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float waveAoeLifetime;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float waveSpeed;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float slowAmount = 10f;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float slowDuration = 1f;

	private GameObject aoe;
	private GameObject waveAoe;

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
		PlayEffectSound(specialFXPrefab,"WindWall");
	}

	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		LockAttackRotation = true;
		CanStun = false;
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
		CanStun = true;
		return base.EndSpecial(stopImmediate);
	}
}
