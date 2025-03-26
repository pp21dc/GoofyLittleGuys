using Managers;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HealingFountain : InteractableBase
{
	[Header("References")]
	[HorizontalRule]
	[SerializeField] private GameObject[] radialCanvases;
	[ColoredGroup][SerializeField] private Transform spawnPoint;

	private bool swappedLayers = false;

	private void OnEnable()
	{
		GameManager.Instance.FountainSpawnPoint = spawnPoint;

		// Tint radial canvases by player color
		for (int i = 0; i < radialCanvases.Length; i++)
		{
			Image fillImage = radialCanvases[i].GetComponentInChildren<Image>();
			fillImage.color = GameManager.Instance.PlayerColours[i];
		}
	}

	private void Update()
	{
		if (!swappedLayers && GameManager.Instance.CurrentPhase > 1)
		{
			UpdateLayers();
			swappedLayers = true;
		}
	}

	public override void StartInteraction(PlayerBody body)
	{
		if (GameManager.Instance.CurrentPhase == 2 || body.IsDead) return;
		if (!PlayerNeedsHealing(body)) return;

		base.StartInteraction(body);

		int index = Mathf.Clamp(body.Controller.PlayerNumber - 1, 0, radialCanvases.Length - 1);

		if (canvasController != null && index < canvasController.Canvases.Length)
			canvasController.Canvases[index].gameObject.SetActive(false);

		if (radialCanvases[index] != null)
			radialCanvases[index].SetActive(true);
	}



	public override void CancelInteraction(PlayerBody body)
	{
		if (body.IsDead) return;
		base.CancelInteraction(body);

		int index = Mathf.Clamp(body.Controller.PlayerNumber - 1, 0, radialCanvases.Length - 1);

		// Hide RadialCanvas
		if (radialCanvases[index] != null)
			radialCanvases[index].SetActive(false);

		// Re-show InteractCanvas if still in range and valid
		UpdateInteractCanvases();

	}


	protected override IEnumerator HoldInteraction(PlayerBody body)
	{
		float holdTime = 0f;
		int index = Mathf.Clamp(body.Controller.PlayerNumber - 1, 0, radialCanvases.Length - 1);
		Image fillImage = radialCanvases[index].GetComponentInChildren<Image>();

		while (holdTime < requiredHoldDuration)
		{
			holdTime += Time.deltaTime;
			if (fillImage != null)
				fillImage.fillAmount = Mathf.Clamp01(holdTime / requiredHoldDuration);
			yield return null;
		}

		CompleteInteraction(body);
		activeHolds.Remove(body);
	}

	protected override void CompleteInteraction(PlayerBody body)
	{
		base.CompleteInteraction(body);

		int index = Mathf.Clamp(body.Controller.PlayerNumber - 1, 0, radialCanvases.Length - 1);

		if (radialCanvases[index] != null)
			radialCanvases[index].SetActive(false);

		if (body.IsDead) return;

		// Restore InteractCanvas only if still in range and allowed
		UpdateInteractCanvases();


		// (Rest of the healing logic unchanged...)
		body.GameplayStats.FountainUses++;

		var haptic = GameManager.Instance.GetHapticEvent("Fountain Used");
		if (haptic != null)
		{
			HapticFeedback.PlayHapticFeedback(body.Controller.GetComponent<PlayerInput>(), haptic.lowFrequency, haptic.highFrequency, haptic.duration);
		}

		foreach (var lilGuy in body.LilGuyTeam)
		{
			if (lilGuy.Health < lilGuy.MaxHealth)
			{
				EventManager.Instance.HealLilGuy(lilGuy, (int)lilGuy.MaxHealth);
				lilGuy.IsDying = false;
				lilGuy.gameObject.SetActive(true);
				lilGuy.GetComponentInChildren<SpriteRenderer>().color = Color.white;
			}
		}

		EventManager.Instance.UpdatePlayerHealthUI(body);
	}

	private bool PlayerNeedsHealing(PlayerBody body)
	{
		foreach (var lilGuy in body.LilGuyTeam)
		{
			if (lilGuy.Health < lilGuy.MaxHealth)
				return true;
		}
		return false;
	}


	/// <summary>
	/// Only show interact canvases if not in Phase 2.
	/// </summary>
	protected override void UpdateInteractCanvases()
	{
		if (GameManager.Instance.CurrentPhase == 2 || canvasController == null)
		{
			canvasController.SetCanvasStates(new bool[canvasController.Canvases.Length]);
			return;
		}

		bool[] activeArray = new bool[canvasController.Canvases.Length];

		foreach (var player in playersInRange)
		{
			if (activeHolds.ContainsKey(player)) continue;
			if (!PlayerNeedsHealing(player)) continue;

			int index = Mathf.Clamp(player.Controller.PlayerNumber - 1, 0, canvasController.Canvases.Length - 1);
			activeArray[index] = true;
		}

		canvasController.SetCanvasStates(activeArray);
	}

}
