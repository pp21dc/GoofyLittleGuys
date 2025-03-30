using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class AoeHitbox : MonoBehaviour
{
	public GameObject hitboxOwner;
	public LayerMask layerMask;

	protected float damage;
	protected float aoeDamageMultiplier = 1;
	protected float knockbackForce = 0f;
	protected float slowAmount = 0f;
	protected float slowDuration = 0f;
	public float AoeDamageMultiplier { get { return aoeDamageMultiplier; } set { aoeDamageMultiplier = value; } }
	public float KnockbackForce { get { return knockbackForce; } set { knockbackForce = value; } }
	public float SlowDuration { set { slowDuration = value; } }
	public float SlowAmount { set { slowAmount = value; } }

	private void Update()
	{
		if (hitboxOwner != null) gameObject.layer = hitboxOwner.layer;
	}
	private void OnTriggerEnter(Collider other)
	{
		if (hitboxOwner == null) return;
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
		gameObject.layer = hitboxOwner.layer;
		damage = CalculateDamage();
		gameObject.SetActive(true);
	}
	protected float CalculateDamage()
	{
		LilGuyBase lilGuy = hitboxOwner.GetComponent<LilGuyBase>();
		float baseDamage = Mathf.CeilToInt(0.56f * lilGuy.Strength * aoeDamageMultiplier);

		// Add subtle damage variation (random bonus from 0 to 5)
		float variableDamage = Random.Range(0, 6); // max is exclusive, so 6 gives up to +5
		return Mathf.CeilToInt(baseDamage + variableDamage);
	}

	/// <summary>
	/// Method called when this hitbox hits a hurtbox.
	/// </summary>
	/// <param name="h"></param>
	private void OnHit(Hurtbox h)
	{
		if (h.Owner == hitboxOwner) return;
		Vector3 knockbackDir = (h.transform.position - transform.position).normalized;
		DebugManager.Log("HIT", DebugManager.DebugCategory.COMBAT);
		DefenseType defenseLilGuy = h.gameObject.GetComponent<DefenseType>();
		PlayerBody attacker = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			float damageDealt = Mathf.CeilToInt(damage * Mathf.Max((1 - defenseLilGuy.DamageReduction), 0.01f));
			// Specifically if we hit a lil guy whose shield is up, we have to apply damage reduction
			h.TakeDamage(damageDealt);    // Ceil because we don't want them to be completely immune to damage.

			if (attacker != null)
				attacker.GameplayStats.DamageDealt += damageDealt;
		}
		else
		{
			// Regular damage taken.
			if (gameObject.layer != LayerMask.NameToLayer("Player")) h.TakeDamage(damage);
			if (attacker != null)
				attacker.GameplayStats.DamageDealt += damage;
		}

		if (h.Health <= 0 && h.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			// If this was a wild lil guy that was hit and they were defeated, log the player who last hit them.
			h.LastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner;
		}

		if (slowAmount != 0)
		{
			// Apply slow
			GameObject slowedEntity; 
			if (h.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
			{
				slowedEntity = h.Owner.GetComponent<LilGuyBase>().PlayerOwner.gameObject;
			}
			else if (h.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
			{
				slowedEntity = h.Owner;
			}
			else return;
			EventManager.Instance.ApplyDebuff(slowedEntity, slowAmount, slowDuration, BuffType.Slow, hitboxOwner);
		}
	}
}
