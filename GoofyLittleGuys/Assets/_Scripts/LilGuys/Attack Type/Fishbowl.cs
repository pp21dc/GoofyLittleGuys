using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishbowl : StrengthType
{
	[Header("Fishbowl Specific")]
	[SerializeField] private float aoeMaxSize = 5f;
	[SerializeField] private float waveDestroyTime = 2f;
	[SerializeField] private float waveMoveSpeed = 10f;


	private GameObject[] instantiatedAoe = new GameObject[4];
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
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
	}

	public void SpawnWaveAoe()
	{
		for (int i = 0; i < instantiatedAoe.Length; i++)
		{
			Quaternion rotation = Quaternion.Euler(0, i * 90, 0);
			instantiatedAoe[i] = Instantiate(aoeShape, attackOrbit.position, rotation);
			instantiatedAoe[i].GetComponent<AoeHitbox>().AoeDamageMultiplier = aoeDamageMultiplier;
			instantiatedAoe[i].GetComponent<AoeMovement>().Speed = waveMoveSpeed;
			instantiatedAoe[i].GetComponent<AoeHitbox>().Init(gameObject);

			Destroy(instantiatedAoe[i], waveDestroyTime);
		}
	}
}
