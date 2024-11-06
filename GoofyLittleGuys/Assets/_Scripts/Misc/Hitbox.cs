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
	public LayerMask layerMask;

	private void OnTriggerEnter(Collider other)
	{
		layerMask = GameManager.Instance.CurrentLayerMask;

		if (layerMask == (layerMask | (1 << other.transform.gameObject.layer)))
		{
			Hurtbox h = other.GetComponent<Hurtbox>();

			if (h != null)
				OnHit(h);
		}
	}

	private void OnHit(Hurtbox h)
	{
		DefenseType defenseLilGuy = h.gameObject.GetComponent<DefenseType>();
		if (defenseLilGuy != null && defenseLilGuy.IsShieldActive)
		{
			h.TakeDamage(Mathf.FloorToInt(Damage * defenseLilGuy.DamageReduction));
		}
		else
		{
			h.TakeDamage(Damage);
		}
		if (h.Health <= 0) { h.lastHit = gameObject; }
		Destroy(this.gameObject);
	}
}
