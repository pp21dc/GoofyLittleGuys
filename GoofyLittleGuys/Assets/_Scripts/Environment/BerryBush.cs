using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBush : InteractableBase
{
	[SerializeField] private int minBerryTime = 3;
	[SerializeField] private int maxBerryTime = 5;
	[SerializeField] private int numOfBerries = 3;
	private int berryAmountOnBush = 3;
	[SerializeField] private List<GameObject> berryMeshes;

	List<GameObject> playersInRange = new List<GameObject>();
	private bool hasBerries = true;
	private bool isRegrowing = false;

	private void OnTriggerStay(Collider other)
	{
		PlayerBody playerInRange = other.GetComponent<PlayerBody>();
		if (playerInRange == null) return;   // Ignore non-player colliders
		interactableCanvas.SetActive(hasBerries);
		if (!hasBerries) return;                                // If there's no berries on this bush, don't go to the interact behaviour.

		if (!playersInRange.Contains(other.gameObject))
		{
			// Add any players in range of this berry bush to the in range list.
			playersInRange.Add(other.gameObject);
		}

		playerInRange.ClosestInteractable = this;
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBody playerInRange = other.GetComponent<PlayerBody>();
		if (playerInRange == null) return;
		if (playersInRange.Contains(other.gameObject))
		{
			playersInRange.Remove(other.gameObject);
		}
		if (playersInRange.Count <= 0)
		{
			interactableCanvas.SetActive(false);
		}

		playerInRange.ClosestInteractable = null;
	}

	/// <summary>
	/// Called when a player interacts with this interactable object.
	/// </summary>
	/// <param name="body">PlayerBody: The player that interacted with this object.</param>
	public override void OnInteracted(PlayerBody body)
	{
		base.OnInteracted(body);
		if (berryAmountOnBush > 0 && body.BerryCount < body.MaxBerryCount)
		{
			body.BerryCount++;
			body.PlayerUI.SetBerryCount(body.BerryCount);
			berryAmountOnBush--;
		}

		// Remove the berries frm the bush as they are consumed.
		// Start Berry Regrowth timer.
		if (berryAmountOnBush <= 0) hasBerries = false;
		else hasBerries = true;


		UpdateVisuals(berryAmountOnBush); 		
		if (berryAmountOnBush <= 0 && !isRegrowing)
		{
			StartCoroutine(BerryRegrowth(Random.Range(minBerryTime, maxBerryTime + 1)));
		}


		// Play healing effect?
	}

	/// <summary>
	/// Helper method that hides the berries mesh.
	/// </summary>
	private void UpdateVisuals(int berryCount)
	{
		if (berryCount <= 0)
		{
			foreach (GameObject berryMesh in berryMeshes)
			{
				berryMesh.SetActive(false);
			}
			return;
		}
		else
		{
			for (int i = 0; i < berryCount; i++)
			{
				berryMeshes[i].SetActive(true);
			}
			for (int i = berryMeshes.Count - 1; i >= berryCount; i--)
			{
				berryMeshes[i].SetActive(false);
			}
		}
	}

	/// <summary>
	/// Coroutine that handles the regeneration of berries on a berry bush.
	/// </summary>
	/// <param name="timeUntilBerries">float: The time in seconds until the berries grow back.</param>
	/// <returns></returns>
	private IEnumerator BerryRegrowth(float timeUntilBerries)
	{
		if (isRegrowing) yield break; // Prevent multiple coroutines
		isRegrowing = true;

		while (berryAmountOnBush < numOfBerries)
		{
			yield return new WaitForSeconds(timeUntilBerries);
			berryAmountOnBush++;
			hasBerries = true;
			UpdateVisuals(berryAmountOnBush);
		}

		isRegrowing = false;
	}

}
