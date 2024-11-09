using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFountain : InteractableBase
{
	[SerializeField] private Transform spawnPoint;				// The position players should respawn at, should all their lil guys be defeated.
	List<GameObject> playersInRange = new List<GameObject>();
	private void OnTriggerStay(Collider other)
	{
		if (other.GetComponent<PlayerBody>() == null) return;
		interactableCanvas.SetActive(true);

		if (!playersInRange.Contains(other.gameObject))
		{
			// Add any players in range to the in range list.
			playersInRange.Add(other.gameObject);
		}
		if (other.GetComponent<PlayerBody>().HasInteracted) OnInteracted(other.GetComponent<PlayerBody>());
	}

	private void OnTriggerExit(Collider other)
	{
		// Removing players from in range list if they go outside of the fountain's range/
		if (playersInRange.Contains(other.gameObject))
		{
			playersInRange.Remove(other.gameObject);
		}
		if (playersInRange.Count <= 0)
		{
			interactableCanvas.SetActive(false);
		}
	}
	private void Awake()
	{
		GameManager.Instance.FountainSpawnPoint = spawnPoint;
	}

	public override void OnInteracted(PlayerBody body)
	{
		base.OnInteracted(body);
		foreach (LilGuyBase lilGuy in body.LilGuyTeam)
		{
			if (lilGuy.health < lilGuy.maxHealth)
			{
				// Heal up every lil guy in the player's team to full.
				// Reset their visuals.
				lilGuy.health = lilGuy.maxHealth;
				lilGuy.gameObject.SetActive(true);
				lilGuy.GetComponent<SpriteRenderer>().color = Color.white;
			}
		}
		// Play healing effect?
	}
}
