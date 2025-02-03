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

	public GameObject InstantiatedDome { set { instantiatedDome = value; } }
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

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}

	private void OnDestroy()
	{
		DeleteDome();
		OnDeath -= DeleteDome;
	}

	protected override void OnDisable()
	{
		DeleteDome();
		base.OnDisable();
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

	protected override void Special()
	{
		if (playerOwner != null) EventManager.Instance.ApplyTeamDamageReduction(playerOwner, teamDamageReductionDuration, this);
		else isShieldActive = true;
		damageReductionActive = true;
		instantiatedDome = Instantiate(domePrefab, transform.position, Quaternion.identity);
		instantiatedDome.GetComponent<TurteriamWall>().Init(domeMaxSize, domeExpansionSpeed, domeLifetime);
		base.Special();
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
		if (playerOwner != null) return;
		damageReductionActive = false;
		isShieldActive = false;

	}

}
