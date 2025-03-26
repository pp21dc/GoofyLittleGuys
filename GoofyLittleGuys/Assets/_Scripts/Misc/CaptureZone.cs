using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureZone : InteractableBase
{
	private LilGuyBase lilGuyToCapture;
	private AiController controller;

	public void Init(LilGuyBase lilGuy)
	{
		lilGuyToCapture = lilGuy;
		canvasController = lilGuyToCapture.TameCanvas;
		controller = lilGuyToCapture.GetComponent<AiController>();
	}
	private void Update()
	{
		lilGuyToCapture.TameCanvas.gameObject.SetActive(true);
	}
	protected override void OnTriggerStay(Collider other)
	{
		PlayerBody player = other.GetComponent<PlayerBody>();
		if (player == null) return;

		if (!playersInRange.Contains(player))
			playersInRange.Add(player);

		player.ClosestWildLilGuy = lilGuyToCapture;

		UpdateInteractCanvases();
	}

	protected override void OnTriggerExit(Collider other)
	{
		PlayerBody player = other.GetComponent<PlayerBody>();
		if (player == null) return;

		if (playersInRange.Contains(player))
			playersInRange.Remove(player);

		if (player.ClosestWildLilGuy == lilGuyToCapture)
			player.ClosestWildLilGuy = null;

		UpdateInteractCanvases();
	}

	private void OnDestroy()
	{
		lilGuyToCapture.TameCanvas.gameObject.SetActive(false);
		foreach (var body in playersInRange)
		{
			if (body.ClosestWildLilGuy == lilGuyToCapture)
				body.ClosestWildLilGuy = null;
		}
	}
}
