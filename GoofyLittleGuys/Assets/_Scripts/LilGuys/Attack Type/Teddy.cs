using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teddy : StrengthType
{
	[SerializeField] private float aoeLifetime = 1;
	public Teddy(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}

	public void SpawnConeAoe()
	{
		GameObject aoe = Instantiate(aoeShape, attackPosition);
		aoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(aoe, aoeLifetime);
	}
}
