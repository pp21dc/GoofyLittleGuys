using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Turteriam : DefenseType
{
	[Header("Turteriam Specific")]
	[HorizontalRule]
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private GameObject domePrefab;
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float domeLifetime = 6f;
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float domeMaxSize = 6f;
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float domeExpansionSpeed = 6f;


	private GameObject instantiatedDome = null;
	private bool damageReductionActive = false;

	public GameObject InstantiatedDome { set { instantiatedDome = value; } get { return instantiatedDome; } }
	public bool DamageReductionActive { set { damageReductionActive = value; } }

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		OnDeath += DeleteDome;
	}

	public void DeleteDome()
	{
		if (instantiatedDome != null) Destroy(instantiatedDome);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DeleteDome();
		OnDeath -= DeleteDome;
	}

	protected override void OnDisable()
	{
		//if (Managers.GameManager.Instance == null) return;
		//CoroutineRunner.Instance.StartCoroutine(EventManager.Instance.StopDamageReduction(playerOwner, teamDamageReductionDuration, this, true));
		//DeleteDome();
		base.OnDisable();
	}
	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (damageReductionActive) return;
		CanStun = false;
		base.StartChargingSpecial();
	}

	public override void StopChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;   // If currently on cooldown and there are no more charges to use.
		if (damageReductionActive) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			base.StopChargingSpecial();
		}
	}

	protected override void Special()
	{
		if (playerOwner != null) playerOwner?.Buffs.AddBuff(BuffType.TeamDamageReduction, DamageReduction, domeLifetime, this);
		else isShieldActive = true;
		damageReductionActive = true;
		instantiatedDome = Instantiate(domePrefab, transform.position, Quaternion.identity);
		instantiatedDome.GetComponent<TurteriamWall>().Init(domeMaxSize, domeExpansionSpeed, domeLifetime);
		CanStun = true;
		base.Special();
	}

	public override void OnEndSpecial(bool stopImmediate = false)
	{
		base.OnEndSpecial(stopImmediate);

	}
	protected override IEnumerator EndSpecial(bool stopImmediate = false)
	{
		if (stopImmediate)
		{
			DeleteDome();
			playerOwner?.Buffs.RemoveBuffFromSource(BuffType.TeamDamageReduction, this);
		}
		else if (!stopImmediate)
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

		LockAttackRotation = false;
		LockMovement = false;
		isShieldActive = false;
		damageReductionActive = false;

		yield break;
	}
}
