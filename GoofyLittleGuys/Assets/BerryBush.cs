using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBush : InteractableBase
{
	[SerializeField] private int minBerryTime = 3;
	[SerializeField] private int maxBerryTime = 5;
	[SerializeField] private GameObject berriesMesh;

	List<GameObject> playersInRange = new List<GameObject>();
	private float berryTimer = 0;
	private bool hasBerries = true;

	private void OnTriggerStay(Collider other)
	{
		if (other.GetComponentInParent<PlayerBody>() == null) return;
		interactableCanvas.SetActive(hasBerries);
		if (!hasBerries) return;

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
		if (body.LilGuyTeam[0].health <= 0 || body.LilGuyTeam[0].health >= body.LilGuyTeam[0].maxHealth) return;
		body.LilGuyTeam[0].health = body.LilGuyTeam[0].maxHealth;

		hasBerries = false;
		UpdateVisuals();
		StartCoroutine(BerryRegrowth(Random.Range(minBerryTime, maxBerryTime + 1)));

		// Play healing effect?
	}

	private void UpdateVisuals()
	{
		berriesMesh.SetActive(hasBerries);
	}

	private IEnumerator BerryRegrowth(float timeUntilBerries)
	{
		yield return new WaitForSeconds(timeUntilBerries);
		hasBerries = true;
		UpdateVisuals();
	}
}
