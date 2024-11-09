using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthType : LilGuyBase
{
	private List<Collider> hitColliders = new List<Collider>();
	[SerializeField] private GameObject aoeShape;  // Only visible in editor and only used when aoeType is set to "Custom". 
	[SerializeField] private float aoeMaxSize = 1;
	[SerializeField] private float aoeExpansionSpeed = 1;
	[SerializeField] public int aoeDamage = 1;
	public List<Collider> HitColliders { get { return hitColliders; } set { hitColliders = value; } }

	public StrengthType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}

	public override void Special()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;

		GameObject aoe = Instantiate(aoeShape, attackPosition);
		aoe.GetComponent<AoeHitbox>().InitializeExpansion(aoeMaxSize, aoeExpansionSpeed, this);

		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
	}
}
