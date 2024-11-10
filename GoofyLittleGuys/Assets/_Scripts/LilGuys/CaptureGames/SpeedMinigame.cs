using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpeedMinigame : CaptureBase
{
	[SerializeField] private TextMeshProUGUI countdownText;			// Countdown text
	[SerializeField] private GameObject catchFeedback;				// UI to show the player is in range to tag the creature
	[SerializeField] private float gameDuration = 10f;				// Game duration
	[SerializeField] private float creatureSpeed = 5f;				// Creature's movement speed
	[SerializeField] private float catchDistance = 1.5f;			// Distance to catch creature
	[SerializeField] private float timeBetweenNewLocations = 0.5f;	// Distance to catch creature

	private float gameTimer;										// The current time in the minigame
	private float creatureDistance = 0;								// The distance the lil guy is from the player

	private void OnEnable()
	{
		player.SwitchCurrentActionMap("SpeedMinigame");		// Switch control map to speed minigame mapping
		player.actions["Tag"].performed += OnCatchAttempt;  // Event to handle tagging input
		player.DeactivateInput();

		// Initialize the player and lil guy positions to opposite ends of the play space.
		player.transform.position = instantiatedBarrier.transform.position - new Vector3(0, 0, areaBounds.y);
		lilGuyBeingCaught.transform.position = instantiatedBarrier.transform.position + new Vector3(0, 0, areaBounds.y);
		gameTimer = gameDuration;

		// TODO: Port this countdown to the other minigames.
		// I think having a countdown before the game actually starts is nice
		// Helps the player prepare for what's about to happen.
		StartCoroutine(StartCountdown());					// Begin countdown
	}

	private void OnDisable()
	{
		countdownText.gameObject.SetActive(true);
		StopAllCoroutines();

		player.actions["Tag"].performed -= OnCatchAttempt;
	}

	private void Update()
	{
		creatureDistance = Vector3.Distance(player.transform.position, lilGuyBeingCaught.transform.position);
		catchFeedback.SetActive(creatureDistance <= catchDistance);
	}

	/// <summary>
	/// Method that is called when the player presses the tag action.
	/// </summary>
	/// <param name="ctx"></param>
	private void OnCatchAttempt(InputAction.CallbackContext ctx)
	{
		if (!gameActive) return;

		// Check the distance between the player and the creature
		if (creatureDistance <= catchDistance)
		{
			// Player successfully caught the creature
			gameActive = false;
			EndMinigame(true);
		}
	}

	/// <summary>
	/// Starts a countdown before the game actually begins.
	/// </summary>
	/// <returns></returns>
	private IEnumerator StartCountdown()
	{
		countdownText.text = "3";
		yield return new WaitForSeconds(1f);
		countdownText.text = "2";
		yield return new WaitForSeconds(1f);
		countdownText.text = "1";
		yield return new WaitForSeconds(1f);
		countdownText.text = "Go!";

		yield return new WaitForSeconds(1f);
		gameActive = true;
		player.ActivateInput();

		StartCoroutine(CountdownTimer());
		StartCoroutine(MoveCreatureRandomly());
	}

	/// <summary>
	/// Keeps track of and shows how much time is left in the minigame in the UI.
	/// </summary>
	/// <returns></returns>
	private IEnumerator CountdownTimer()
	{
		while (gameTimer > 0 && gameActive)
		{
			gameTimer -= Time.deltaTime;
			countdownText.text = gameTimer.ToString("F1");  // Show timer to one decimal point
			yield return null;
		}

		EndMinigame(false);  // Time ran out
	}

	/// <summary>
	/// Coroutine that moves the lil guy to random spots within the play space every now and then.
	/// </summary>
	/// <returns></returns>
	private IEnumerator MoveCreatureRandomly()
	{
		while (gameActive)
		{

			// Pick a new random location to reach.
			Vector3 randomLocation = new Vector3(
				Random.Range(instantiatedBarrier.transform.position.x - areaBounds.x, instantiatedBarrier.transform.position.x + areaBounds.x),
				lilGuyBeingCaught.transform.position.y,
				Random.Range(instantiatedBarrier.transform.position.z - areaBounds.y, instantiatedBarrier.transform.position.z + areaBounds.y)
			);

			// Move the creature to the new position
			while (Vector3.Distance(lilGuyBeingCaught.transform.position, randomLocation) > 0.1f && gameActive)
			{
				lilGuyBeingCaught.transform.position = Vector3.MoveTowards(lilGuyBeingCaught.transform.position, randomLocation, lilGuyBeingCaught.speed * Time.deltaTime);
				yield return null;
			}

			yield return new WaitForSeconds(0.5f);  // Brief pause before moving again
		}
	}
}
