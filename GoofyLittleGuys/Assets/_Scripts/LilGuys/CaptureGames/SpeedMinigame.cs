using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpeedMinigame : CaptureBase
{
	[SerializeField] private TextMeshProUGUI countdownText;    // Countdown text
	[SerializeField] private float gameDuration = 10f;         // Game duration
	[SerializeField] private float creatureSpeed = 5f;         // Creature's movement speed
	[SerializeField] private Vector2 areaBounds;               // Area boundaries (x, z)

	private float gameTimer;

	void OnEnable()
	{
		gameTimer = gameDuration;
		StartCoroutine(StartCountdown());
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
				lilGuyBeingCaught.transform.position = Vector3.MoveTowards(lilGuyBeingCaught.transform.position, targetPosition, creatureSpeed * Time.deltaTime);
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

	private void OnTriggerEnter(Collider other)
	{
		if (gameActive && other.gameObject == lilGuyBeingCaught.gameObject)
		{
			gameActive = false;
			EndMinigame(true);  // Player caught the creature
		}
	}
}