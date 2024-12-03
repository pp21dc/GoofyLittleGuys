using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObj : MonoBehaviour
{
    public float dmgPerInterval;
    public float interval;
    private float dmgPerTick;

    //private bool intervalRunning = false; // whether or not the storm is currently damaging a player
    // Start is called before the first frame update
    void Start()
    {
        dmgPerTick = dmgPerInterval / interval;
    }

   

    private void OnTriggerEnter(Collider hitCollider)
    {
        

        Hurtbox playerHurtbox = hitCollider.gameObject.GetComponentInParent<Hurtbox>();
        
        StartCoroutine(DelayedDamage(playerHurtbox));
        PlayerBody playerHit = hitCollider.gameObject.GetComponentInParent<PlayerBody>();
        playerHit.InStorm = true;
           
    }

    private void OnTriggerStay(Collider hitCollider)
    {

        if(hitCollider.attachedRigidbody)
        {
            Hurtbox playerHurtbox = hitCollider.gameObject.GetComponent<Hurtbox>();
            //playerHurtbox.TakeDamage(dmgPerInterval);
            PlayerBody playerHit = hitCollider.gameObject.GetComponent<PlayerBody>();
            if (playerHurtbox != null && playerHit != null )
            {
                
                
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
                //StopCoroutine(DelayedDamage(playerHurtbox));
                playerHit.InStorm = false;
            }
        }
        //StopAllCoroutines();
    }

    /// <summary>
    /// Deals damage to h every frame, but only enough to deal 'dmgPerInterval' per 'interval' of time
    /// </summary>
    /// <param name="h"></param>
    private void DamageOverTime(PlayerBody player)
    {
        float dmgTaken = dmgPerTick * Time.deltaTime;
        LilGuyBase hurtLilGuy = player.gameObject.GetComponent<LilGuyBase>();
        hurtLilGuy.Health -= dmgTaken;
        //h.TakeDamage(dmgTaken);
    }


    /// <summary>
    /// Coroutine that simply waits for interval to elapse, then deals some damage
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedDamage(Hurtbox h)
    {
        PlayerBody playerHit = h.gameObject.GetComponentInParent<PlayerBody>();
        if (h == null)
        {
            StopAllCoroutines();
        }
        // wait for interval (seconds, ensure it is relative to deltaTime although I think WaitForSeconds handles that)
        yield return new WaitForSeconds(interval);
        // Deal damage == dmgPerInterval
        if (playerHit.InStorm)
        {
            h.TakeDamage(dmgPerInterval);
            StartCoroutine(DelayedDamage(h));
        }
        else
        {
            StopCoroutine(DelayedDamage(h));
        }
       
       

        //StartCoroutine(DelayedDamage(hurtbox));
        
        
    }

}
