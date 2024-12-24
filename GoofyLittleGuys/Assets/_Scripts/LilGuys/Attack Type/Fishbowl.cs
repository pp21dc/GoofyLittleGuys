using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishbowl : StrengthType
{
	[Header("Fishbowl Specific")]
	[SerializeField] private float aoeMaxSize = 5f;
	[SerializeField] private float knockbackForce = 10f;


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
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
	}

	public void SpawnWaveAoe()
	{
		instantiatedAoe = Instantiate(aoeShape, attackPosition);
		instantiatedAoe.GetComponent<AoeHitbox>().AoeDamageMultiplier = aoeDamageMultiplier;
		instantiatedAoe.GetComponent<AoeHitbox>().KnockbackForce = knockbackForce;
		instantiatedAoe.GetComponent<AoeHitbox>().Init(gameObject);
		StartCoroutine(Expand());
	}

	private IEnumerator Expand()
	{
		Vector3 initialScale = instantiatedAoe.transform.localScale;
		Vector3 targetScale = new Vector3(aoeMaxSize, aoeMaxSize, aoeMaxSize);

		float elapsedTime = 0;
		while (elapsedTime < aoeDestroyTime)
		{
			instantiatedAoe.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / aoeDestroyTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		Destroy(instantiatedAoe);
	}
}
