using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auth: Thomas Berner
// - Hitbox class for any combat based trigger
// -> can be derrived from for other attacks which provide knockback or other effects onHit
public class Hitbox : MonoBehaviour
{
	public GameObject hitboxOwner;
	public LayerMask layerMask;

	protected float Damage;

	private void Update()
	{
		if (hitboxOwner != null) gameObject.layer = hitboxOwner.layer;
	}
	private void OnTriggerEnter(Collider other)
	{
		if (hitboxOwner.layer == LayerMask.NameToLayer("Player")) return;
		Hurtbox h = other.GetComponent<Hurtbox>();
		if (h != null)
			OnHit(h);
	}

	/// <summary>
	/// Method called when this hitbox is instantiated.
	/// </summary>
	/// <param name="hitboxOwner">The owner of the hitbox.</param>
	public virtual void Init(GameObject hitboxOwner)
	{
		this.hitboxOwner = hitboxOwner;
		Damage = CalculateDamage();
	}
	protected float CalculateDamage()
	{
		LilGuyBase lilGuy = hitboxOwner.GetComponent<LilGuyBase>();
		float baseDamage = Mathf.CeilToInt(0.56f * lilGuy.Strength);

		// Add subtle damage variation (random bonus from 0 to 5)
		float variableDamage = Random.Range(0, 6); // max is exclusive, so 6 gives up to +5
		return Mathf.CeilToInt(baseDamage + variableDamage);
	}

	private bool AreEnemies(GameObject attacker, GameObject target)
	{
		LilGuyBase attackerLilGuy = attacker.GetComponent<LilGuyBase>();
		LilGuyBase targetLilGuy = target.GetComponent<LilGuyBase>();

		if (attackerLilGuy == null || targetLilGuy == null) return false; // Safety check

		return attackerLilGuy.PlayerOwner != targetLilGuy.PlayerOwner; // Only true if they have different owners
	}

	/// <summary>
	/// Method called when this hitbox hits a hurtbox.
	/// </summary>
	/// <param name="h"></param>
	private void OnHit(Hurtbox h)
	{
		if (hitboxOwner.layer == LayerMask.NameToLayer("Player")) return;
		Managers.DebugManager.Log("HIT", DebugManager.DebugCategory.COMBAT);
		Toadstool toadstool = h.GetComponent<Toadstool>();
		PlayerBody attacker = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		if (toadstool != null && toadstool.IsShieldActive && AreEnemies(hitboxOwner, h.gameObject))
		{
			float damageDealt = Mathf.CeilToInt(Damage * Mathf.Max((1 - toadstool.DamageReduction), 0.01f));
			h.TakeDamage(damageDealt);    // Ceil because we don't want them to be completely immune to damage.
			h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;

			EventManager.Instance.ApplyDebuff(hitboxOwner, toadstool.PoisonDamage, toadstool.PoisonDuration, BuffType.Poison, toadstool, toadstool.PoisonDamageApplicationInterval);
			toadstool.PlaySound("Poison_Spray");
		}
		DefenseType defenseLilGuy = h.gameObject.GetComponent<DefenseType>();
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			float damageDealt = Mathf.CeilToInt(Damage * Mathf.Max((1 - defenseLilGuy.DamageReduction), 0.01f));
			float damageReduced = Damage - (Mathf.CeilToInt(Damage * Mathf.Max((1 - defenseLilGuy.DamageReduction), 0.01f)));

			if (attacker != null)
				attacker.GameplayStats.DamageDealt += damageDealt;
			if (!ReferenceEquals(h.GetComponent<LilGuyBase>().PlayerOwner, null))
			{
				h.GetComponent<LilGuyBase>().PlayerOwner.GameplayStats.DamageReduced += damageReduced;
			}
			// Specifically if we hit a lil guy whose shield is up, we have to apply damage reduction
			h.TakeDamage(damageDealt);    // Ceil because we don't want them to be completely immune to damage.
			h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		}
		else
		{
			// Regular damage taken.
			if (gameObject.layer != LayerMask.NameToLayer("Player") && hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner != h.Owner.GetComponent<LilGuyBase>().PlayerOwner)
			{
				if (attacker != null) 
					attacker.GameplayStats.DamageDealt += Damage;
				h.TakeDamage(Damage);
			}
			h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		}

		if (h.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			WildBehaviour hitLilGuyWild = h.GetComponent<WildBehaviour>();
			// If this was a wild lil guy that was hit and they were defeated, log the player who last hit them.
			hitLilGuyWild.IncreaseHostility(Random.Range(1f, 3f));
			if (hitLilGuyWild.Charisma > 5) hitLilGuyWild.AggroWildLilGuys();
			if (h.Health <= 0) h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		}
		Destroy(gameObject);
	}
}
