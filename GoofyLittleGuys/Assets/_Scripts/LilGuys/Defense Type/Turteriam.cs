using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turteriam : DefenseType
{
	[Header("Turteriam Specific")]
	[SerializeField] private GameObject domePrefab;
	[SerializeField] private float teamDamageReductionDuration = 6f;
	[SerializeField] private float domeLifetime = 6f;
	[SerializeField] private float domeMaxSize = 6f;
	[SerializeField] private float domeExpansionSpeed = 6f;


	private bool damageReductionActive = false;
	private GameObject instantiatedDome = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (damageReductionActive) return;
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

	public override void Special()
	{
		if (playerOwner != null) playerOwner.TeamDamageReduction += DamageReduction;
		else isShieldActive = true;
		damageReductionActive = true;
		instantiatedDome = Instantiate(domePrefab, transform.position, Quaternion.identity);
		instantiatedDome.GetComponent<TurteriamWall>().Init(domeMaxSize, domeExpansionSpeed, domeLifetime);
		StartCoroutine(StopDamageReduction(playerOwner != null));
		base.Special();
	}
	private IEnumerator StopDamageReduction(bool playerOwned)
	{
		yield return new WaitForSeconds(teamDamageReductionDuration);
		if (!playerOwned) isShieldActive = false;
		else playerOwner.TeamDamageReduction -= DamageReduction;

		instantiatedDome = null;
		damageReductionActive = false;
	}

}
