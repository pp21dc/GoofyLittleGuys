using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeSpecialMove : SpecialMoveBase
{
	/// <summary>
	/// Defines a type of AoE. Used to differentiate between available Physics.Overlap[shape] and custom shapes if needed (ex. Cone Shaped).
	/// </summary>
	public enum AoEType
	{
		Box,
		Sphere,
		Capsule,
		Custom
	}

	[SerializeField]
	private AoEType aoeType;

	[SerializeField]
	private Collider aoeShape;	// Only visible in editor and only used when aoeType is set to "Custom". 
	[SerializeField]
	private float aoeMaxSize = 1;
	[SerializeField]
	private float aoeExpansionSpeed = 1;
	[SerializeField]
	private int aoeDamage = 1;


	private Collider[] hitColliders = null;
	public Collider[] HitColliders { get { return hitColliders; } set { hitColliders = value; } }
	protected override void OnSpecialUsed()
	{
		if (cooldownTimer > 0) return;
		switch (aoeType)
		{
			case AoEType.Box:
				hitColliders = Physics.OverlapBox(transform.position,
					new Vector3(aoeMaxSize * 0.5f, aoeMaxSize * 0.5f, aoeMaxSize * 0.5f));
				StartCoroutine("AoeExpansion");
				break;
			case AoEType.Sphere:
				hitColliders = Physics.OverlapSphere(transform.position, aoeMaxSize * 0.5f);
				StartCoroutine("AoeExpansion");
				break;
			case AoEType.Capsule:
				hitColliders = Physics.OverlapCapsule(transform.position + Vector3.down,
					transform.position + Vector3.up, aoeMaxSize * 0.5f);
				StartCoroutine("AoeExpansion");
				break;
			case AoEType.Custom:
				aoeShape.GetComponent<AoeHitbox>().InitializeExpansion(aoeMaxSize, aoeExpansionSpeed, GetComponent<LilGuyBase>());
				DealDamage(hitColliders);
				break;
		}
	}
	/// <summary>
	/// Adds a delay to the damage application of the AoE for the Physics.Overlap[shape].
	/// This is to simulate the "expansion" feeling of the AoE without having to necessarily define an expanding collider.
	/// </summary>
	/// <returns></returns>
	private IEnumerator AoeExpansion()
	{
		yield return new WaitForSeconds(aoeExpansionSpeed);
		DealDamage(hitColliders);
		yield break;
	}
	private void DealDamage(Collider[] hitColliders)
	{
		if (hitColliders == null) return;
		foreach (Collider collider in hitColliders)
		{
			LilGuyBase lilGuy = collider.GetComponent<LilGuyBase>();
			if (lilGuy != null && lilGuy != GetComponent<LilGuyBase>())
			{
				lilGuy.health -= aoeDamage;
			}
		}
	}
}
