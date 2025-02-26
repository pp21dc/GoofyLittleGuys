using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Armordillo : DefenseType
{
	[Header("Armordillo Specific")]
	[SerializeField] private GameObject knockbackPrefab;
	[SerializeField] private float knockbackForceAmount = 60f;
	[SerializeField] private float knockbackDuration = 1f;
	[SerializeField] private GameObject shieldPrefab; // The shield prefab to instantiate
	[SerializeField] private float duration = 1;
	[SerializeField] private float speedBoost = 30f;
	[SerializeField] private Color startColour = new Color(0, 0.9647058823529412f, 1);
	[SerializeField] private Color endColour = new Color(0.9450980392156862f, 0.615686274509804f, 0.615686274509804f);

	private float speedBoostTime;
	private GameObject instantiatedKnockback;
	bool speedBoostActive = false;
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		
	}
	public override void StopChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;   // If currently on cooldown and there are no more charges to use.
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			base.StopChargingSpecial();
		}
	}
	protected override void Special()
	{		
		base.Special();
		if (speedBoostActive) return;
		speed += speedBoost;
		CalculateMoveSpeed();
		StartCoroutine(StopSpeedBoost());
		speedBoostActive = true;

		spawnedShieldObj ??= Instantiate(shieldPrefab, transform.position + Vector3.up, Quaternion.identity, transform); // If spawnShieldObj is null, assign it this instantiated GO
		spawnedShieldObj.GetComponent<Shield>().Initialize(duration, this, startColour, endColour);
		isShieldActive = true;

		instantiatedKnockback = Instantiate(knockbackPrefab, transform.position, Quaternion.identity, transform);
		instantiatedKnockback.transform.localScale = Vector3.one * 8;
		KnockbackHitbox h = instantiatedKnockback.GetComponent<KnockbackHitbox>();
		h.KnockbackForce = knockbackForceAmount;
		h.KnockbackDuration = knockbackDuration;
	}

	protected override IEnumerator EndSpecial(bool stopImmediate = false)
	{
		if (specialDuration >= 0) yield return new WaitForSeconds(specialDuration);
		else if (specialDuration == -1)
		{
			AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
			if (clip != null) yield return new WaitForSeconds(clip.length);
		}
		if (anim != null)
		{
			anim.SetTrigger("SpecialAttackEnded");
		}

		LockAttackRotation = false;
		LockMovement = false;

		// Destroy the knockback prefab if it exists
		if (instantiatedKnockback != null)
		{
			Destroy(instantiatedKnockback);
		}
	}
	private IEnumerator StopSpeedBoost()
	{
		yield return new WaitForSeconds(specialDuration);
		speed -= speedBoost;
		CalculateMoveSpeed();
		speedBoostActive = false;
	}
}
