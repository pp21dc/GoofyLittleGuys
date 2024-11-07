using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class LastHitMenu : MonoBehaviour
{
	[SerializeField] private PlayerBody player;
	[SerializeField] private Button firstButton;
	[SerializeField] private MultiplayerEventSystem playerEventSystem;
	[SerializeField] private List<GameObject> minigames;
	private LilGuyBase lilGuyToCapture;

	// Called when the UI is enabled
	private void OnEnable()
	{
		// Set the first selected button to "Yes"
		playerEventSystem.firstSelectedGameObject = firstButton.gameObject;
		EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
	}

	public void Initialize(LilGuyBase lilGuy)
	{
		// Store the LilGuy being captured for reference when starting minigame
		lilGuyToCapture = lilGuy;
	}

	public void OnYesSelected()
	{
		player.DisableUIControl();
		// Start the capture minigame for this specific LilGuy's type
		if (lilGuyToCapture != null)
		{
			StartCaptureMinigame();
		}

		ClosePrompt();
	}

	public void OnNoSelected()
	{
		player.DisableUIControl();
		ClosePrompt();
	}

	private void ClosePrompt()
	{
		// Close the UI prompt and clean up button listeners
		gameObject.SetActive(false);

		// Reset player input to gameplay (assuming this is managed by PlayerBody)
	}

	private void StartCaptureMinigame()
	{
		// Pseudocode: Start the capture minigame based on LilGuy type
		if (lilGuyToCapture.type == LilGuyBase.PrimaryType.Strength)
		{
			minigames[0].GetComponent<CaptureBase>().Initialize(lilGuyToCapture);
			minigames[0].SetActive(true);
		}
		else if (lilGuyToCapture.type == LilGuyBase.PrimaryType.Defense)
		{
			minigames[1].GetComponent<CaptureBase>().Initialize(lilGuyToCapture);
			minigames[1].SetActive(true);
		}
		else if (lilGuyToCapture.type == LilGuyBase.PrimaryType.Speed)
		{
			minigames[2].GetComponent<CaptureBase>().Initialize(lilGuyToCapture);
			minigames[2].SetActive(true);
		}
		else
		{
			Debug.LogWarning("Unknown capture type!");
		}
	}
}
