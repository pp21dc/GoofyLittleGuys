using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFountain : InteractableBase
{
	[SerializeField] private Transform spawnPoint;
	List<GameObject> playersInRange = new List<GameObject>();
	private void OnTriggerStay(Collider other)
	{
		if (other.GetComponent<PlayerBody>() == null) return;
		interactableCanvas.SetActive(true);
		if (!playersInRange.Contains(other.gameObject))
		{
			playersInRange.Add(other.gameObject);
		}
		if (other.GetComponent<PlayerBody>().HasInteracted) OnInteracted(other.GetComponent<PlayerBody>());
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
				lilGuy.health = lilGuy.maxHealth;
				lilGuy.gameObject.SetActive(true);
				lilGuy.GetComponent<SpriteRenderer>().color = Color.white;
			}
		}
		// Play healing effect?
	}
}
