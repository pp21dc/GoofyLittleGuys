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
                StopDamage(playerHurtbox,playerHit);
                //playerHit.InStorm = false;
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
            yield break;
        }
        PlayerBody playerHit = h.gameObject.GetComponentInParent<PlayerBody>();
        if (playerHit == null)
        {
            StopCoroutine(DelayedDamage(h));
            yield break;
        }
        while (playerHit.InStorm)
        {
            if (!playerHit.IsDead)
            {
                yield return new WaitForSeconds(interval);
                h.TakeDamage(dmgPerInterval);
            }
            else
            {
                yield break;
            }
            
        }
    }

    /// <summary>
    /// Forces the DelayedDamage coroutine to stop for a given hurtbox and player, then makes sure that player is no longer
    /// considered to be in the storm.
    /// </summary>
    private void StopDamage(Hurtbox h, PlayerBody playerHit)
    {
        StopCoroutine(DelayedDamage(h));
        playerHit.InStorm = false;
    }

}
