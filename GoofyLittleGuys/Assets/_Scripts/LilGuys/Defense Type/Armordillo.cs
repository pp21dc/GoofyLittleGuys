using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Armordillo : DefenseType
{
	[Header("Armordillo Specific")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject knockbackPrefab;
	[ColoredGroup][SerializeField] private GameObject shieldPrefab; // The shield prefab to instantiate
	[ColoredGroup][SerializeField] private Color startColour = new Color(0, 0.9647058823529412f, 1);
	[ColoredGroup][SerializeField] private Color endColour = new Color(0.9450980392156862f, 0.615686274509804f, 0.615686274509804f);
	[ColoredGroup][SerializeField] private float knockbackForceAmount = 60f;
	[ColoredGroup][SerializeField] private float knockbackDuration = 1f;
	[ColoredGroup][SerializeField] private float shieldUptime = 5;
	[ColoredGroup][SerializeField] private float speedBoost = 30f;

	private GameObject instantiatedKnockback;
	private bool speedBoostActive = false;
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

	protected override void OnDisable()
	{
		base.OnDisable();
		if (instantiatedKnockback != null)
		{
			Destroy(instantiatedKnockback);
			instantiatedKnockback = null;
		}
		if (spawnedShieldObj != null)
		{
			Destroy(spawnedShieldObj);
			spawnedShieldObj = null;
		}
	}
	protected override void Special()
	{
		base.Special();
		if (speedBoostActive) return;
		speed += speedBoost;
		CalculateMoveSpeed();
		speedBoostActive = true;

		spawnedShieldObj ??= Instantiate(shieldPrefab, transform.position + Vector3.up, Quaternion.identity, transform); // If spawnShieldObj is null, assign it this instantiated GO
		spawnedShieldObj.GetComponent<Shield>().Initialize(shieldUptime, this, startColour, endColour);
		isShieldActive = true;

		instantiatedKnockback = Instantiate(knockbackPrefab, transform.position, Quaternion.identity, transform);
		instantiatedKnockback.transform.localScale = Vector3.one * 8;
		KnockbackHitbox h = instantiatedKnockback.GetComponent<KnockbackHitbox>();
		h.KnockbackForce = knockbackForceAmount;
		h.KnockbackDuration = knockbackDuration;
	}

	protected override IEnumerator EndSpecial(bool stopImmediate = false)
	{

		if (!stopImmediate)
		{
			if (specialDuration >= 0) yield return new WaitForSeconds(specialDuration);
			else if (specialDuration == -1)
			{
				AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
				if (clip != null) yield return new WaitForSeconds(clip.length);
			}
		}

		// Stop the speed boost immediately if canceling early
		if (speedBoostActive)
		{
			StartCoroutine(StopSpeedBoost());
		}

		// Remove the shield when special ends early
		if (spawnedShieldObj != null)
		{
			spawnedShieldObj.GetComponent<Shield>().BeginShieldFade();
		}

		// Remove the knockback effect if it exists
		if (instantiatedKnockback != null)
		{
			Destroy(instantiatedKnockback);
			instantiatedKnockback = null;
		}

		// Skip waiting if forced to end immediately


		if (anim != null)
		{
			anim.SetTrigger("SpecialAttackEnded");
		}

		LockAttackRotation = false;
		LockMovement = false;

	}

	private IEnumerator StopSpeedBoost()
	{
		speed -= speedBoost;
		CalculateMoveSpeed();
		speedBoostActive = false;
		yield break;
	}
}
