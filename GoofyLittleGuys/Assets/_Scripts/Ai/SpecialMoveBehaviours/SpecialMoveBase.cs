using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LilGuyBase))]
public class SpecialMoveBase : MonoBehaviour
{
	protected float cooldownTimer = 0;
	protected float cooldownDuration = 1;
	[SerializeField]
	protected int currentCharges = 1;
	[SerializeField]
	protected int maxCharges = 1;

	protected LilGuyBase lilGuy;

	protected float chargeRefreshRate = 1;
	protected float chargeTimer = 0;
	public virtual void OnSpecialUsed() { }

	private void Update()
	{
		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}
		if (chargeTimer > 0)
		{
			chargeTimer -= Time.deltaTime;
		}
		if (currentCharges < maxCharges && chargeTimer <= 0)
		{
			currentCharges++;
			chargeTimer = chargeRefreshRate;
		}
	}
}
