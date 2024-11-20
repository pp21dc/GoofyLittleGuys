using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBush : InteractableBase
{
	[SerializeField] private int minBerryTime = 3;
	[SerializeField] private int maxBerryTime = 5;
	[SerializeField] private int numOfBerries = 1;
	[SerializeField] private GameObject berriesMesh;

	List<GameObject> playersInRange = new List<GameObject>();
	private bool hasBerries = true;

	private void OnTriggerStay(Collider other)
	{
		if (other.GetComponent<PlayerBody>() == null) return;	// Ignore non-player colliders
		interactableCanvas.SetActive(hasBerries);
		if (!hasBerries) return;								// If there's no berries on this bush, don't go to the interact behaviour.

		if (!playersInRange.Contains(other.gameObject))
		{
			// Add any players in range of this berry bush to the in range list.
			playersInRange.Add(other.gameObject);
		}

		if (other.GetComponent<PlayerBody>().HasInteracted) OnInteracted(other.GetComponent<PlayerBody>());	// Player interacted
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

	/// <summary>
	/// Called when a player interacts with this interactable object.
	/// </summary>
	/// <param name="body">PlayerBody: The player that interacted with this object.</param>
	public override void OnInteracted(PlayerBody body)
	{
		base.OnInteracted(body);
		body.BerryCount += numOfBerries;

		// Remove the berries frm the bush as they are consumed.
		// Start Berry Regrowth timer.
		hasBerries = false;
		UpdateVisuals();
		StartCoroutine(BerryRegrowth(Random.Range(minBerryTime, maxBerryTime + 1)));

		// Play healing effect?
	}

	/// <summary>
	/// Helper method that hides the berries mesh.
	/// </summary>
	private void UpdateVisuals()
	{
		berriesMesh.SetActive(hasBerries);
	}

	/// <summary>
	/// Coroutine that handles the regeneration of berries on a berry bush.
	/// </summary>
	/// <param name="timeUntilBerries">float: The time in seconds until the berries grow back.</param>
	/// <returns></returns>
	private IEnumerator BerryRegrowth(float timeUntilBerries)
	{
		yield return new WaitForSeconds(timeUntilBerries);
		hasBerries = true;
		UpdateVisuals();
	}
}
