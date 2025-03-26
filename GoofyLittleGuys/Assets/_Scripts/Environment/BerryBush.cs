using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBush : InteractableBase
{
	[Header("References")]
	[HorizontalRule]
	[SerializeField] private List<GameObject> berryMeshes;

	[Header("Growth Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private int minBerryTime = 3;
	[ColoredGroup][SerializeField] private int maxBerryTime = 5;
	[ColoredGroup][SerializeField] private int numOfBerries = 3;

	private int berryAmountOnBush = 3;
	private bool hasBerries = true;
	private bool isRegrowing = false;
	private bool swappedLayers = false;

	private void Update()
	{
		if (!swappedLayers && !hasBerries)
		{
			UpdateLayers();
			swappedLayers = true;
		}
		else if (swappedLayers && hasBerries)
		{
			UpdateLayers(true);
			swappedLayers = false;
		}
	}

	public override void StartInteraction(PlayerBody body)
	{
		if (body.IsDead || !hasBerries) return;
		base.StartInteraction(body);
	}

	public override void CancelInteraction(PlayerBody body)
	{
		if (body.IsDead) return;
		base.CancelInteraction(body);
	}

	protected override void CompleteInteraction(PlayerBody body)
	{
		base.CompleteInteraction(body);

		if (berryAmountOnBush > 0 && body.BerryCount < body.MaxBerryCount)
		{
			body.BerryCount++;
			body.PlayerUI.SetBerryCount(body.BerryCount);
			body.ActiveLilGuy.PlaySound("Pick_Berry");
			berryAmountOnBush--;
		}

		hasBerries = berryAmountOnBush > 0;
		UpdateVisuals(berryAmountOnBush);

		if (!hasBerries && !isRegrowing)
			StartCoroutine(BerryRegrowth(Random.Range(minBerryTime, maxBerryTime + 1)));
	}

	private void UpdateVisuals(int berryCount)
	{
		for (int i = 0; i < berryMeshes.Count; i++)
		{
			berryMeshes[i].SetActive(i < berryCount);
		}
	}

	private IEnumerator BerryRegrowth(float timeUntilBerries)
	{
		if (isRegrowing) yield break;
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

	/// <summary>
	/// Only show canvases if berries are available.
	/// </summary>
	protected override void UpdateInteractCanvases()
	{
		if (!hasBerries || canvasController == null)
		{
			canvasController.SetCanvasStates(new bool[canvasController.Canvases.Length]);
			return;
		}

		base.UpdateInteractCanvases();
	}
}
