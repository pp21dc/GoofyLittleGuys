using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishbowl : StrengthType
{
	[Header("Fishbowl Specific")]
	[SerializeField] private float aoeMaxSize = 5f;
	[SerializeField] private float waveDestroyTime = 2f;
	[SerializeField] private float waveMoveSpeed = 10f;


	private GameObject instantiatedAoe = null;
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		base.StartChargingSpecial();
	}

	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	}

	protected override void Special()
	{
		base.Special();
		Rigidbody rb = (playerOwner == null) ? RB : playerOwner.GetComponent<Rigidbody>();
		rb.isKinematic = true;
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
		Rigidbody rb = (playerOwner == null) ? RB : playerOwner.GetComponent<Rigidbody>();
		rb.isKinematic = false;
	}

	public void SpawnWaveAoe()
	{
		instantiatedAoe = Instantiate(aoeShape, transform.position, Quaternion.identity);
		instantiatedAoe.GetComponent<FishbowlWaves>().Init(specialDuration);
		foreach(Transform child in  instantiatedAoe.transform)
		{
			child.GetComponent<AoeHitbox>().AoeDamageMultiplier = aoeDamageMultiplier;
			child.GetComponent<AoeMovement>().Speed = waveMoveSpeed;
			child.GetComponent<AoeHitbox>().Init(gameObject);

		}
	}
}
