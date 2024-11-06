using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpeedMinigame : CaptureBase
{
	[SerializeField] private TextMeshProUGUI countdownText;    // Countdown text
	[SerializeField] private GameObject catchFeedback;		   // UI to show the player is in range to tag the creature
	[SerializeField] private float gameDuration = 10f;         // Game duration
	[SerializeField] private float creatureSpeed = 5f;         // Creature's movement speed
	[SerializeField] private float catchDistance = 1.5f;       // Distance to catch creature
	[SerializeField] private Vector2 areaBounds;               // Area boundaries (x, z)

	private float gameTimer;
	private float creatureDistance = 0;

	private void OnEnable()
	{
		player.SwitchCurrentActionMap("SpeedMinigame");
		player.actions["Tag"].performed += OnCatchAttempt;
		gameTimer = gameDuration;
		StartCoroutine(StartCountdown());
	}
	private void OnDisable()
	{
		StopAllCoroutines();
		player.actions["Tag"].performed -= OnCatchAttempt;
		player.SwitchCurrentActionMap("World");
	}

	private void Update()
	{
		creatureDistance = Vector3.Distance(player.transform.position, lilGuyBeingCaught.transform.position);
		catchFeedback.SetActive(creatureDistance <= catchDistance);
	}

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
		countdownText.gameObject.SetActive(false);
		gameActive = true;

		StartCoroutine(CountdownTimer());
		StartCoroutine(MoveCreatureRandomly());
	}

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

	private IEnumerator MoveCreatureRandomly()
	{
		while (gameActive)
		{
			Vector3 randomDirection = new Vector3(
				Random.Range(-areaBounds.x, areaBounds.x),
				lilGuyBeingCaught.transform.position.y,
				Random.Range(-areaBounds.y, areaBounds.y)
			);

			Vector3 targetPosition = transform.position + randomDirection;
			targetPosition = ClampPositionToBounds(targetPosition);

			// Move the creature to the new position
			while (Vector3.Distance(lilGuyBeingCaught.transform.position, targetPosition) > 0.1f && gameActive)
			{
				lilGuyBeingCaught.transform.position = Vector3.MoveTowards(lilGuyBeingCaught.transform.position, targetPosition, lilGuyBeingCaught.speed * Time.deltaTime);
				yield return null;
			}

			yield return new WaitForSeconds(0.5f);  // Brief pause before moving again
		}
	}

	private Vector3 ClampPositionToBounds(Vector3 position)
	{
		// Ensure the creature stays within the specified area bounds
		position.x = Mathf.Clamp(position.x, -areaBounds.x, areaBounds.x);
		position.z = Mathf.Clamp(position.z, -areaBounds.y, areaBounds.y);
		return position;
	}

}
