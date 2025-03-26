using System.Collections;
using System.Linq;
using UnityEngine;

public class Toadstool : DefenseType
{
	[Header("Toadstool Specific")]
	[HorizontalRule]
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private GameObject gasPrefab;
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float poisonDamage = 3;
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float poisonDuration = 2;
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float poisonDamageApplicationInterval = 0.5f;

	private Rigidbody affectedRB;

	public float PoisonDamage => poisonDamage;
	public float PoisonDuration => poisonDuration;
	public float PoisonDamageApplicationInterval => poisonDamageApplicationInterval;

	public override void OnEndSpecial(bool stopImmediate = false)
	{
		isShieldActive = false;
		base.OnEndSpecial(stopImmediate);
	}

	protected override void Special()
	{
		base.Special();
		CanStun = false;
		affectedRB = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
		affectedRB.velocity = Vector3.zero;
		affectedRB.isKinematic = true;
		isShieldActive = true;
	}

	public override void StartChargingSpecial()
	{
		base.StartChargingSpecial();
	}

	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	
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
		}

		if (!ReferenceEquals(affectedRB, null)) affectedRB.isKinematic = false;
		isShieldActive = false;
		LockAttackRotation = false;
		LockMovement = false;
		CanStun = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		//if (!ReferenceEquals(affectedRB, null)) affectedRB.isKinematic = false;
		//isShieldActive = false;
	}
}
