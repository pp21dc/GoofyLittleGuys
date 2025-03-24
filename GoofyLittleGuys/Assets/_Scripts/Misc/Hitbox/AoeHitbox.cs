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
		damage = Mathf.CeilToInt(0.56f * hitboxOwner.GetComponent<LilGuyBase>().Strength) * aoeDamageMultiplier;
		gameObject.SetActive(true);
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
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			// Specifically if we hit a lil guy whose shield is up, we have to apply damage reduction
			h.TakeDamage(Mathf.CeilToInt(damage * (1 - defenseLilGuy.DamageReduction)));    // Ceil because we don't want them to be completely immune to damage.
		}
		else
		{
			// Regular damage taken.
			if (gameObject.layer != LayerMask.NameToLayer("Player")) h.TakeDamage(damage);
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
