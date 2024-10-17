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
        if (layerMask == (layerMask | (1 << other.transform.gameObject.layer)))
        {
            Hurtbox h = other.GetComponent<Hurtbox>();

            if (h != null)
                OnHit(h);
        }
    }

    private void OnHit(Hurtbox h)
    {
        h.TakeDamage(Damage);
        Destroy(this.gameObject);
    }
}
