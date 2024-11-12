using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class LastHitMenu : MonoBehaviour
{
	[SerializeField] private PlayerBody player;							// Reference to the player owner of this menu.
	[SerializeField] private Button firstButton;						// The first button to be selected on default.
	[SerializeField] private MultiplayerEventSystem playerEventSystem;	// Reference to the player's event system.
	[SerializeField] private List<GameObject> minigames;				// A list of minigames that could occur for the player.
	private LilGuyBase lilGuyToCapture;									// Thi lil guy we are trying to capture... or not.


	private void OnEnable()
	{
		// Set the first selected button to "Yes"
		playerEventSystem.gameObject.SetActive(true);
		playerEventSystem.firstSelectedGameObject = firstButton.gameObject;
		playerEventSystem.SetSelectedGameObject(firstButton.gameObject);
	}

	public void Initialize(LilGuyBase lilGuy)
	{
		// Store the LilGuy being captured for reference when starting minigame
		lilGuyToCapture = lilGuy;
	}

	/// <summary>
	/// Called when we are going to try and catch the lil guy.
	/// </summary>
	public void OnYesSelected()
	{
		playerEventSystem.gameObject.SetActive(false);
		player.DisableUIControl();
		// Start the capture minigame for this specific LilGuy's type
		if (lilGuyToCapture != null)
		{
			StartCaptureMinigame();
		}

		ClosePrompt();
	}

	/// <summary>
	/// Called when we chose to collect stats instead.
	/// </summary>
	public void OnNoSelected()
	{
		playerEventSystem.gameObject.SetActive(false);
		player.DisableUIControl();
		player.LilGuyTeam[0].AddCaptureStats(lilGuyToCapture);
		//Destroy(lilGuyToCapture.gameObject);
		ClosePrompt();
	}

	private void ClosePrompt()
	{
		// Close the UI prompt and clean up button listeners
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Method that starts one of the capture minigames, depending on the type of lil guy that the player is trying to capture.
	/// </summary>
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
