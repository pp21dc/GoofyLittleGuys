using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthType : LilGuyBase
{
	private Transform attackPos;
	private List<Collider> hitColliders = new List<Collider>();
	[SerializeField] private GameObject aoeShape;  // Only visible in editor and only used when aoeType is set to "Custom". 
	[SerializeField] private float aoeMaxSize = 1;
	[SerializeField] private float aoeExpansionSpeed = 1;
	[SerializeField] private int aoeDamage = 1;
	public List<Collider> HitColliders { get { return hitColliders; } set { hitColliders = value; } }

	public StrengthType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}

	public override void Special()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;

		GameObject aoe = Instantiate(aoeShape, transform);
		aoe.GetComponent<AoeHitbox>().InitializeExpansion(aoeMaxSize, aoeExpansionSpeed, this);
		DealDamage(hitColliders);

		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
	}

	private void DealDamage(List<Collider> hitColliders)
	{
		if (hitColliders == null) return;
		foreach (Collider collider in hitColliders)
		{
			LilGuyBase enemyLilGuy = collider.GetComponent<LilGuyBase>();
			if (enemyLilGuy != null && enemyLilGuy != this)
			{
				enemyLilGuy.health -= aoeDamage * strength;
			}
		}
	}
}
