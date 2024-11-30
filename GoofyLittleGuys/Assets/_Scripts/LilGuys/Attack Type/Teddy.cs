using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teddy : StrengthType
{
	[SerializeField] private float aoeLifetime = 1;

	public void SpawnConeAoe()
	{
		GameObject aoe = Instantiate(aoeShape, attackPosition);
		aoe.GetComponent<AoeHitbox>().Init(gameObject);
		Destroy(aoe, aoeLifetime);
	}
}
