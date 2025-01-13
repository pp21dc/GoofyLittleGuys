using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toadstool : DefenseType
{
	[Header("Toadstool Specific")]
	[SerializeField] private GameObject gasPrefab;
	[SerializeField] private float shieldTime = 4f;
	[SerializeField] private float poisonDamage = 3;
	[SerializeField] private float poisonDuration = 2;
	[SerializeField] private float poisonDamageApplicationInterval = 0.5f;


	Rigidbody affectedRB;
	public float PoisonDamage => poisonDamage;
	public float PoisonDuration => poisonDuration;
	public float PoisonDamageApplicationInterval => poisonDamageApplicationInterval;

	protected override void OnEndSpecial()
	{
		isShieldActive = false;
		base.OnEndSpecial();
	}

	protected override void Special()
	{
		base.Special();
		affectedRB = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
		affectedRB.velocity = Vector3.zero;
		affectedRB.isKinematic = true;
		isShieldActive = true;
		StartCoroutine(EndShield());
	}

	public override void StartChargingSpecial()
	{
		base.StartChargingSpecial();
	}

	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	
	}

	private IEnumerator EndShield()
	{
		yield return new WaitForSeconds(shieldTime);
		affectedRB.isKinematic = false;
		isShieldActive = false;
	}
}
