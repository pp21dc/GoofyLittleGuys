using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

//TODO: Make this derive CaptureBase
public class StrengthMinigame : CaptureBase
{

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
