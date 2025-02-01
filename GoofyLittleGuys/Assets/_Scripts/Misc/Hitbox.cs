using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auth: Thomas Berner
// - Hitbox class for any combat based trigger
// -> can be derrived from for other attacks which provide knockback or other effects onHit
public class Hitbox : MonoBehaviour
{
	protected float Damage;
	public GameObject hitboxOwner;
	public LayerMask layerMask;

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
		Damage = Mathf.CeilToInt(0.56f * hitboxOwner.GetComponent<LilGuyBase>().Strength);
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
		Debug.Log("HIT");
		Toadstool toadstool = h.GetComponent<Toadstool>();
		if (toadstool != null && toadstool.IsShieldActive && AreEnemies(hitboxOwner, h.gameObject))
		{
			h.TakeDamage(Mathf.CeilToInt(Damage * Mathf.Max((1 - toadstool.DamageReduction), 0.01f)));    // Ceil because we don't want them to be completely immune to damage.
			h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;

			EventManager.Instance.ApplyDebuff(hitboxOwner, toadstool.PoisonDamage, toadstool.PoisonDuration, DebuffType.Poison, toadstool.PoisonDamageApplicationInterval);
		}
		DefenseType defenseLilGuy = h.gameObject.GetComponent<DefenseType>();
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			// Specifically if we hit a lil guy whose shield is up, we have to apply damage reduction
			h.TakeDamage(Mathf.CeilToInt(Damage * Mathf.Max((1 - defenseLilGuy.DamageReduction), 0.01f)));    // Ceil because we don't want them to be completely immune to damage.
			h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		}
		else
		{
			// Regular damage taken.
			if (gameObject.layer != LayerMask.NameToLayer("Player") && hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner != h.Owner.GetComponent<LilGuyBase>().PlayerOwner) h.TakeDamage(Damage);
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
