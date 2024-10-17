using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFountain : InteractableBase
{
	List<GameObject> playersInRange;
	private void OnTriggerStay(Collider other)
	{
		if (other.GetComponentInParent<PlayerBody>() == null) return;
		interactableCanvas.SetActive(true);
		if (!playersInRange.Contains(other.gameObject))
		{
			playersInRange.Add(other.gameObject);
		}
		if (other.GetComponentInParent<PlayerBody>().HasInteracted) OnInteracted(other.GetComponentInParent<PlayerBody>());
	}
	private void OnTriggerExit(Collider other)
	{
		if (playersInRange.Contains(other.gameObject))
		{
			playersInRange.Remove(other.gameObject);
		}
		if (playersInRange.Count <= 0)
		{
			interactableCanvas.SetActive(false);
		}
	}
	public override void OnInteracted(PlayerBody body)
	{
		base.OnInteracted(body);
		foreach (LilGuyBase lilGuy in body.LilGuyTeam)
		{
			if (lilGuy.health < lilGuy.maxHealth) lilGuy.health = lilGuy.maxHealth;
		}
		// Play healing effect?
	}
}
