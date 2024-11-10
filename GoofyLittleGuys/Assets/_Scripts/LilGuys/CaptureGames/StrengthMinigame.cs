using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class StrengthMinigame : CaptureBase
{
	[SerializeField] private TextMeshProUGUI countdownText;     // Countdown text
	[SerializeField] private Slider strengthMeter;				// The slider seen at the top of the screen to show who's winning or losing
	[SerializeField] private float aiPushStrength = 0.1f;		// How strong the opposing lil guy is at pushing back against the player's mashes.
	[SerializeField] private float playerMashStrength = 0.02f;	// How much strength is behind the player's button mash per button press.
	[SerializeField] private float aiPushIncreaseRate = 0.01f;	// How much stronger the opposing lil guy gets over time as the minigame continues.

	private float currentMeterValue = 0.5f;						// The current score of the minigame.
	private InputAction mashAction;


	private void OnEnable()
	{
		player.SwitchCurrentActionMap("StrengthMinigame");
		mashAction = player.actions["MashButton"];

		// Initialize player and lil guy positions to opposite ends of the play space. Flip the player the other direction since they stand on the left facing right.
		player.transform.position = instantiatedBarrier.transform.position - new Vector3(areaBounds.x, 0, 0);
		player.GetComponent<PlayerBody>().Flip = true;
		lilGuyBeingCaught.transform.position = instantiatedBarrier.transform.position + new Vector3(areaBounds.x, 0, 0);

		player.DeactivateInput();
		mashAction.performed += OnMashButtonPressed;
		StartCoroutine(StartCountdown());	// Begin countdown
	}

	private void OnDisable()
	{
		countdownText.transform.parent.gameObject.SetActive(true);

		mashAction.performed -= OnMashButtonPressed;
	}

	// Start is called before the first frame update
	void Start()
	{
		currentMeterValue = 0.5f;
		strengthMeter.value = currentMeterValue;
	}

	// Update is called once per frame
	void Update()
	{
		if (gameActive)
		{
			HandleAIPush();
			UpdateMeter();
			CheckWinCondition();
		}
	}

	/// <summary>
	/// Begins a countdown before the game actually begins.
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

		countdownText.transform.parent.gameObject.SetActive(false);
		player.ActivateInput();
	}

	/// <summary>
	/// Method that is called when the player presses the button to mash in this game.
	/// </summary>
	/// <param name="ctx"></param>
	private void OnMashButtonPressed(InputAction.CallbackContext ctx)
	{
		currentMeterValue += playerMashStrength;
	}

	/// <summary>
	/// Method that handles the opposing AI's push force in the minigame.
	/// Increases difficulty slowly over the course of the minigame.
	/// </summary>
	private void HandleAIPush()
	{
		currentMeterValue -= aiPushStrength * Time.deltaTime;
		aiPushStrength += aiPushIncreaseRate * Time.deltaTime;
	}

	/// <summary>
	/// Updates the slider at the top of the screen according to the current score of the mashing minigame.
	/// </summary>
	private void UpdateMeter()
	{
		currentMeterValue = Mathf.Clamp01(currentMeterValue);
		strengthMeter.value = currentMeterValue;
	}

	/// <summary>
	/// Method that checks if the win or lose conditions of the minigame were met.
	/// </summary>
	private void CheckWinCondition()
	{
		if (currentMeterValue >= 1f)
		{
			// The player beat the lil guy in the mash competition! So they won!
			gameActive = false;
			EndMinigame(true);
		}
		else if (currentMeterValue <= 0f)
		{
			// The player lost against the lil guy.
			gameActive = false;
			EndMinigame(false);
		}

	}
}
