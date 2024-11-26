using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObj : MonoBehaviour
{
    public float dmgPerInterval = 3;
    public float interval = 3;

    private bool intervalRunning = false; // whether or not 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider hitCollider)
    {

        if(hitCollider.attachedRigidbody)
        {
            Hurtbox playerHurtbox = hitCollider.gameObject.GetComponent<Hurtbox>();
            PlayerBody playerHit = hitCollider.gameObject.GetComponent<PlayerBody>();
            if (playerHurtbox != null && playerHit != null )
            {
                if (!playerHit.InStorm)
                {
                    StartCoroutine(DelayedDamage(playerHurtbox));
                    playerHit.InStorm = true;
                }
                
            }

        }
    }

    private void OnTriggerExit(Collider hitCollider)
    {
        Hurtbox playerHurtbox = hitCollider.gameObject.GetComponent<Hurtbox>();
        PlayerBody playerHit = hitCollider.gameObject.GetComponent<PlayerBody>();
        if (playerHurtbox != null && playerHit != null)
        {
            if (playerHit.InStorm)
            {
                StopCoroutine(DelayedDamage(playerHurtbox));
                playerHit.InStorm = false;
            }
        }
    }

    /// <summary>
    /// Coroutine that simply waits for interval to elapse, then
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedDamage(Hurtbox hurtbox)
    {
        if(hurtbox == null)
        {
            StopCoroutine(DelayedDamage(hurtbox));
        }
        // wait for interval (seconds, ensure it is relative to deltaTime)
        yield return new WaitForSeconds(interval);
        // Deal damage == dmgPerInterval
        hurtbox.TakeDamage(dmgPerInterval);

        StartCoroutine(DelayedDamage(hurtbox));
    }

}
