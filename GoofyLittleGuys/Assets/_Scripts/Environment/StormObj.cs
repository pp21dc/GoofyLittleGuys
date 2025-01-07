using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObj : MonoBehaviour
{
    public float dmgPerInterval;
    public float interval;

    //On trigger enter, a coroutine should start for the player that entered, that (while the given player is InStorm),
    //simply waits a few seconds, then turns on StormDmg for that player if it isn't already on.
    private void OnTriggerEnter(Collider collision)
    {
        Hurtbox playerHurtbox = collision.gameObject.GetComponent<Hurtbox>();
        if (playerHurtbox != null && playerHurtbox.gameObject.GetComponent<TamedBehaviour>() != null)
        {
            PlayerBody playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>().PlayerOwner;
            if (playerHit != null && !playerHit.StormCoroutine)
            {
                playerHit.InStorm = true;
                StartCoroutine(DmgCycle(playerHit));
            }
        }
    }

    //On trigger stay (each frame a player is in the storm) they should check if they are meant to take damage this frame, if so, we should call 
    //the players' StormDamage method.
    private void OnTriggerStay(Collider collision)
    {
        Hurtbox playerHurtbox = collision.gameObject.GetComponent<Hurtbox>();
        if (playerHurtbox != null && playerHurtbox.gameObject.GetComponent<TamedBehaviour>() != null)
        {
            PlayerBody playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>().PlayerOwner;
            if (playerHit != null && !playerHit.InStorm)
            {
                playerHit.InStorm = true;
            }
            if (playerHit != null && playerHit.StormDmg)
            {
                playerHit.StormDamage(dmgPerInterval);
            }
        }
    }

    //Coroutine that, notifies thisPlayer it has started this coroutine,
    //then, while thisPlayer is InStorm, wait interval, then set thisPlayer.StormDmg = true if it's not already.
    private IEnumerator DmgCycle(PlayerBody thisPlayer)
    {
        thisPlayer.StormCoroutine = true;
        while (thisPlayer.InStorm)
        {
            yield return new WaitForSeconds(interval);
            if (!thisPlayer.StormDmg)
            {
                thisPlayer.StormDmg = true;
            }
        }
    }

    //On trigger exit, thisPlayer that left should have its coroutine stopped, and 
    //then they should be notified that StormCoroutine = false and StormDmg = false.
    private void OnTriggerExit(Collider collision)
    {
        Hurtbox playerHurtbox = collision.gameObject.GetComponent<Hurtbox>();
        if (playerHurtbox != null && playerHurtbox.gameObject.GetComponent<TamedBehaviour>() != null)
        {
            PlayerBody playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>().PlayerOwner;
            if (playerHit != null && playerHit.StormCoroutine)
            {
                StopCoroutine(DmgCycle(playerHit));
                playerHit.InStorm = false;
                playerHit.StormDmg = false;
                playerHit.StormCoroutine = false;
            }

        }
    }
}
