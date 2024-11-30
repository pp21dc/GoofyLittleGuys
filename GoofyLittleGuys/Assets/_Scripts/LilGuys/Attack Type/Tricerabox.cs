using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tricerabox : StrengthType
{
	[Header("Tricerabox Specific Parameters")]
	[SerializeField] private Transform waveAoePosition;
	[SerializeField] private float waveAoeDamage;
	[SerializeField] private float waveAoeLifetime;
	[SerializeField] private float aoeLifetime;

	public void SpawnPunchAoe()
	{
		GameObject aoe = Instantiate(aoeShape, attackPosition);
		AoeHitbox hitbox = aoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamage = aoeDamage;
		hitbox.Init(gameObject);
		Destroy(aoe, aoeLifetime);
	}

	public void SpawnWaveAoe()
	{
		GameObject waveAoe = Instantiate(aoeShape, waveAoePosition);
		AoeHitbox hitbox = waveAoe.GetComponent<AoeHitbox>();
		hitbox.AoeDamage = waveAoeDamage;
		waveAoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(waveAoe, waveAoeLifetime);

	}
}
