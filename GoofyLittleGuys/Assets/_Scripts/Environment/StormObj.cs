using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObj : MonoBehaviour
{
    public float dmgPerInterval;
    public float interval;


    private void OnTriggerEnter(Collider hitCollider)
    {
        Hurtbox playerHurtbox = hitCollider.gameObject.GetComponent<Hurtbox>();
        if (playerHurtbox != null && playerHurtbox.gameObject.GetComponent<TamedBehaviour>() != null) 
        {
            PlayerBody playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>().PlayerOwner;
            if (playerHit != null)
            {
                playerHit.InStorm = true;
                StartCoroutine(DelayedDamage(playerHurtbox));
            }
        } 
    }

    private void OnTriggerExit(Collider hitCollider)
    {
        //Hurtbox playerHurtbox = hitCollider.gameObject.GetComponent<Hurtbox>();
        Hurtbox playerHurtbox = hitCollider.gameObject.GetComponentInParent<Hurtbox>();
        PlayerBody playerHit = hitCollider.gameObject.GetComponentInParent<PlayerBody>();
        if (playerHurtbox != null && playerHit != null)
        {
            if (playerHit.InStorm)
            {
                playerHit.InStorm = false;
            }
        } 
    }

    /// <summary>
    /// Coroutine that simply waits for interval to elapse, then deals some damage
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedDamage(Hurtbox h)
    {
        if (h == null)
        {
            StopCoroutine(DelayedDamage(h));
        }
        PlayerBody playerHit = h.gameObject.GetComponentInParent<PlayerBody>();
        if (playerHit == null)
        {
            StopCoroutine(DelayedDamage(h));
        }
        while (playerHit.InStorm)
        {
            yield return new WaitForSeconds(interval);
            h.TakeDamage(dmgPerInterval);
        }
    }
}
