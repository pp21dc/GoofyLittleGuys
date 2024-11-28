using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class AoeHitbox : MonoBehaviour
{
	protected float damage;
	protected float aoeDamage;
	public float AoeDamage { get { return aoeDamage; } set { aoeDamage = value; } }
	public GameObject hitboxOwner;
	public LayerMask layerMask;

	private void OnTriggerEnter(Collider other)
	{
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
		damage = aoeDamage + hitboxOwner.GetComponent<LilGuyBase>().Strength;
	}

	/// <summary>
	/// Method called when this hitbox hits a hurtbox.
	/// </summary>
	/// <param name="h"></param>
	private void OnHit(Hurtbox h)
	{
		Debug.Log("HIT");
		DefenseType defenseLilGuy = h.gameObject.GetComponent<DefenseType>();
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			// Specifically if we hit a lil guy whose shield is up, we have to apply damage reduction
			h.TakeDamage(Mathf.CeilToInt(damage * (1 - defenseLilGuy.DamageReduction)));    // Ceil because we don't want them to be completely immune to damage.
		}
		else
		{
			// Regular damage taken.
			h.TakeDamage(damage);
		}

		if (h.Health <= 0 && h.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			// If this was a wild lil guy that was hit and they were defeated, log the player who last hit them.
			h.lastHit = hitboxOwner.GetComponent<LilGuyBase>().PlayerOwner.gameObject;
		}
	}
}
