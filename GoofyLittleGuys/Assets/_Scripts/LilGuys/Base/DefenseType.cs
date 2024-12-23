using UnityEngine;

public class DefenseType : LilGuyBase
{
	protected GameObject spawnedShieldObj = null;    // The actual instantiated shield object on the lil guy
	protected bool isShieldActive = false;
	[SerializeField] private float damageReduction = 0.5f;     // Change to damageReduction
															   // Insert variable 3 that can vary between lil guys
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
	public override void Special()
	{		
			base.Special();		
	}

	private void OnDisable()
	{
		if (spawnedShieldObj != null) Destroy(spawnedShieldObj);
	}
}
