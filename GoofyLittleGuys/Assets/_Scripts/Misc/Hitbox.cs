using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auth: Thomas Berner
// - Hitbox class for any combat based trigger
// -> can be derrived from for other attacks which provide knockback or other effects onHit
public class Hitbox : MonoBehaviour
{
	protected int Damage;
	public GameObject hitboxOwner;
	public LayerMask layerMask;

	public void Init(GameObject hitboxOwner)
	{
		this.hitboxOwner = hitboxOwner;
		gameObject.layer = hitboxOwner.layer;
		Damage = hitboxOwner.GetComponent<LilGuyBase>().strength;
	}

	private void OnTriggerEnter(Collider other)
	{
		Hurtbox h = other.GetComponent<Hurtbox>();
		if (h != null)
			OnHit(h);

	}

	private void OnHit(Hurtbox h)
	{
		Debug.Log("HIT");
		DefenseType defenseLilGuy = h.gameObject.GetComponent<DefenseType>();
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			h.TakeDamage(Mathf.FloorToInt(Damage * defenseLilGuy.DamageReduction));
		}
		else
		{
			h.TakeDamage(Damage);
		}
		if (h.Health <= 0 && h.gameObject.layer == LayerMask.NameToLayer("WildLilGuys")) { h.lastHit = hitboxOwner.GetComponent<LilGuyBase>().playerOwner; }
		Destroy(this.gameObject);
	}
}
