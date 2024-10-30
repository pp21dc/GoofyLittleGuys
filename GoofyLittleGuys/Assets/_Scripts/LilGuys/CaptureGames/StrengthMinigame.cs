using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StrengthMinigame : MonoBehaviour
{
	// Put in base class
	[SerializeField] private PlayerInput player;
	[SerializeField] private LilGuyBase lilGuyBeingCaught;
	private bool gameActive = false;

	// Strength minigame specific
	[SerializeField] private Slider strengthMeter;
	[SerializeField] private float aiPushStrength = 0.1f;
	[SerializeField] private float playerMashStrength = 0.02f;
	[SerializeField] private float aiPushIncreaseRate = 0.01f;
	private float currentMeterValue = 0.5f;
	private InputAction mashAction;


	private void OnEnable()
	{

		player.SwitchCurrentActionMap("StrengthMinigame");
		mashAction = player.actions["MashButton"];
		mashAction.performed += OnMashButtonPressed;
	}

	private void OnMashButtonPressed(InputAction.CallbackContext ctx)
	{
		currentMeterValue += playerMashStrength;
	}

	private void HandleAIPush()
	{
		currentMeterValue -= aiPushStrength * Time.deltaTime;
		aiPushStrength += aiPushIncreaseRate * Time.deltaTime;
	}

	private void UpdateMeter()
	{
		currentMeterValue = Mathf.Clamp01(currentMeterValue);
		strengthMeter.value = currentMeterValue;
	}

	private void CheckWinCondition()
	{
		if (currentMeterValue >= 1f)
		{
			gameActive = false;
			EndMinigame(true);
		}
		else if (currentMeterValue <= 0f)
		{
			gameActive = false;
			EndMinigame(false);
		}

	}

	private void EndMinigame(bool playerWon)
	{
		if (playerWon)
		{
			Debug.Log("Player Won!");
			// Add this lil guy to their team (if there's space)
			// Probably call some method on the player just to handle team management
			// In case they need to choose to remove a lil guy or something.
		}
		else
		{
			Debug.Log("Player lost!");
			// Lost... idk what happens :3
		}

		gameObject.SetActive(false);
	}
	private void OnDisable()
	{
		mashAction.performed -= OnMashButtonPressed;
		player.SwitchCurrentActionMap("World");
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
		HandleAIPush();
		UpdateMeter();
		CheckWinCondition();

	}
}
