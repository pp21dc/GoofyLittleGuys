using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObj : MonoBehaviour
{
	public float dmgPerInterval;
	public float interval;

	private List<PlayerBody> playersInStorm = new List<PlayerBody>();
	private Coroutine stormDmgCoroutine = null;

	private void Start()
	{
		StartCoroutine(DmgCycle());
	}

	//On trigger enter, a coroutine should start for the player that entered, that (while the given player is InStorm),
	//simply waits a few seconds, then turns on StormDmg for that player if it isn't already on.
	private void OnTriggerEnter(Collider collision)
	{
		Hurtbox playerHurtbox = collision.gameObject.GetComponent<Hurtbox>();
		if (playerHurtbox != null && playerHurtbox.gameObject.GetComponent<TamedBehaviour>() != null)
		{
			PlayerBody playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>().PlayerOwner;
			if (playerHit == null) return;
			if (!playersInStorm.Contains(playerHit))
			{
				playersInStorm.Add(playerHit);
			}
		}
	}


	//Coroutine that, notifies thisPlayer it has started this coroutine,
	//then, while thisPlayer is InStorm, wait interval, then set thisPlayer.StormDmg = true if it's not already.
	private IEnumerator DmgCycle()
	{
		while (true)
		{
			yield return new WaitForSeconds(interval);
			if (playersInStorm.Count > 0) foreach (PlayerBody body in playersInStorm) body.StormDamage(dmgPerInterval);
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
			if (playerHit == null) return;
			if (playersInStorm.Contains(playerHit))
			{
				playerHit.InStorm = false;
				playersInStorm.Remove(playerHit);
			}

		}
	}
}
