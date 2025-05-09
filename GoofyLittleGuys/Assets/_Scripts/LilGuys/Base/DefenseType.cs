using UnityEngine;

public class DefenseType : LilGuyBase
{
	[Header("Defense Type Specific")]
	[HorizontalRule]
	[ColoredGroup(0.1865f, 0.3124f, 0.373f)][SerializeField] private float damageReduction = 0.5f;     // Change to damageReduction

	protected GameObject spawnedShieldObj = null;    // The actual instantiated shield object on the lil guy
	protected bool isShieldActive = false;

	public GameObject SpawnedShieldObj { get { return spawnedShieldObj; } set { spawnedShieldObj = value; } }
	public bool IsShieldActive { get { return isShieldActive; } set { isShieldActive = value; } }
	public float DamageReduction { get { return damageReduction; } }

	public override void StopChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;   // If currently on cooldown and there are no more charges to use.
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			base.StopChargingSpecial();
		}
	}

	public override void StartChargingSpecial()
	{
		base.StartChargingSpecial();
	}
	protected override void Special()
	{
		base.Special();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (spawnedShieldObj != null) Destroy(spawnedShieldObj);
	}
}
