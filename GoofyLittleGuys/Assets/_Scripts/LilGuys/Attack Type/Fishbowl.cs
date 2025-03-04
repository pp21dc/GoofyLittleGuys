using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fishbowl : StrengthType
{
	[Header("Fishbowl Specific")]
	[SerializeField] private float minChargeTime = 0.5f;
	[SerializeField] private float maxChargeTime = 1.5f;
	[SerializeField] private float aoeMaxSize = 5f;
	[SerializeField] private float waveDestroyTime = 2f;
	[SerializeField] private float waveMoveSpeed = 10f;
	[SerializeField] private float minKnockback = 10f;
	[SerializeField] private float maxKnockback = 25f;
	[SerializeField] private float knockbackDuration = 1f;
	[SerializeField] private Color chargeEffectColour;

	bool isCharging = false;
	private float chargeTime = 0f;
	private GameObject instantiatedAoe = null;
	private GameObject chargeEffect;

	protected override void Update()
	{
		base.Update();
		if (isCharging)
		{
			chargeTime += Time.deltaTime;
			chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);
			if (chargeEffect != null) chargeEffect.transform.localScale = Vector3.one * (0.5f + 2 * chargeTime);
			if (chargeTime >= maxChargeTime)
			{
				StopChargingSpecial();
			}
		}
	}
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			if (!isCharging)
			{
				isCharging = true;
				chargeTime = 0;
				chargeEffect = Instantiate(FXManager.Instance.GetEffect("ChargeUp"), transform.position, Quaternion.identity, transform);
				chargeEffect.GetComponent<ParticleSystem>().startColor = chargeEffectColour;
			}

			LockMovement = true;
			// Decrement charges and reset cooldowns
			cooldownTimer = cooldownDuration;
			chargeTimer = chargeRefreshRate;
			currentCharges--;

            anim.ResetTrigger("SpecialAttackEnded");
            anim.ResetTrigger("EndCharge");
            anim.ResetTrigger("SpecialAttack");
            anim.SetTrigger("SpecialAttack");
            if (playerOwner == null) StopChargingSpecial();
		}
			
	}

	public override void StopChargingSpecial()
	{
		if (!isCharging) return;
		PlaySound("Fishbowl_PopOff");
		if (chargeTime >= minChargeTime)
		{
			LockMovement = false;
			isCharging = false;
			Special();
		}
		else
		{
			StartCoroutine(WaitForChargeCompletion());
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Destroy(chargeEffect);
		LockMovement = false;
		isCharging = false;
	}

	protected override void Special()
	{
		Destroy(chargeEffect);
		anim.SetTrigger("EndCharge");
        anim.ResetTrigger("SpecialAttack");
        LockMovement = true;	
		base.Special();
		isCharging = false;
	}

	private IEnumerator WaitForChargeCompletion()
	{
		while (chargeTime < minChargeTime)
		{
			yield return null;
		}
		isCharging = false;
		Special();
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
        if (anim != null)
        {
            anim.SetTrigger("SpecialAttackEnded");
            anim.ResetTrigger("EndCharge");
			StopLoopingSound();
			PlaySound("Fishbowl_PopOn");
		}
        LockAttackRotation = false;
        LockMovement = false;
    }
    public void SpawnWaveAoe()
	{
		instantiatedAoe = Instantiate(aoeShape, transform.position, Quaternion.identity);
		instantiatedAoe.GetComponent<FishbowlWaves>().Init(specialDuration, chargeTime, maxChargeTime, minChargeTime);
		foreach(Transform child in  instantiatedAoe.transform)
		{
			AoeHitbox hitbox = child.GetComponent<AoeHitbox>();
			KnockbackHitbox kHitbox = child.GetComponent<KnockbackHitbox>();
			hitbox.AoeDamageMultiplier = aoeDamageMultiplier;
			child.GetComponent<AoeMovement>().Speed = waveMoveSpeed;
			hitbox.Init(gameObject);
			kHitbox.KnockbackForce = Mathf.Lerp(minKnockback, maxKnockback, (chargeTime - minChargeTime)/(maxChargeTime - minChargeTime));
			kHitbox.KnockbackDuration = knockbackDuration;

		}
		PlayLoopingSound("Special_Fishbowl");
	}
}
