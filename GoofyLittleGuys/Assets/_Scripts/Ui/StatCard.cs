using Managers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class StatCard : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	public Image[] playerShapes;
	public Image[] checkmarkShapes;
	[ColoredGroup] public TMP_Text playerNum;
	[ColoredGroup] public TMP_Text[] titleTextBoxes;
	[ColoredGroup] public TMP_Text stats;
	[ColoredGroup] public GameObject crownImage;
	[ColoredGroup] public TMP_Text ranking;
	[ColoredGroup] public Animator mostUsedIcon;
	[ColoredGroup] public AnimatorOverrideController aocTemplate;
	[ColoredGroup] public Image background;
	[ColoredGroup] public Image submitIcon;
	[ColoredGroup] public TMP_Text submitText;
	[ColoredGroup] public GameObject statViewPanel;
	[ColoredGroup] public GameObject lockedInPanel;
	[ColoredGroup] public Sprite readyCheckmark;


	private PlayerInput playerInput;
	private bool isReady = false;

	public PlayerInput PlayerInput => playerInput;
	public void Initialize(PlayerInput input)
	{
		playerInput = input;

		playerInput.actions["Submit"].performed += OnSubmit;
		playerInput.actions["Cancel"].performed += OnCancel;

		playerInput.SwitchCurrentActionMap("UI"); // Ensure they’re on the correct action map

		PlayerController controller = playerInput.GetComponent<PlayerController>();
		InputDisplayInfo info = controller.UIMappings.GetInfo("Submit");
		if (controller.ControlSchemeType == PlayerController.ControllerType.Gamepad)
		{
			submitIcon.sprite = info.gamepadIcon;
			submitText.text = "";
		}
		else
		{
			submitIcon.sprite = info.keyboardIcon;
			RectTransform rect = submitIcon.GetComponent<RectTransform>();
			Vector2 newSize = rect.sizeDelta;
			newSize.x = info.keyboardKeyName == "Space" ? 160 : 80;
			rect.sizeDelta = newSize;
			submitText.text = info.keyboardKeyName;
		}
	}

	private void OnDestroy()
	{
		if (playerInput != null)
		{
			playerInput.actions["Submit"].performed -= OnSubmit;
			playerInput.actions["Cancel"].performed -= OnCancel;
		}
	}

	private void OnSubmit(InputAction.CallbackContext ctx)
	{
		if (isReady) return;

		isReady = true;
		foreach (Image i in checkmarkShapes)
		{
			i.sprite = readyCheckmark;
		}
		statViewPanel.SetActive(false);
		lockedInPanel.SetActive(true);
		VictoryScreenManager.Instance.MarkPlayerReady(playerInput);
	}

	private void OnCancel(InputAction.CallbackContext ctx)
	{
		if (!isReady) return;

		isReady = false;
		foreach (Image i in checkmarkShapes)
		{
			i.sprite = UiManager.Instance.shapes[playerInput.GetComponent<PlayerController>().PlayerNumber - 1];
		}
		statViewPanel.SetActive(true);
		lockedInPanel.SetActive(false);
		VictoryScreenManager.Instance.UnmarkPlayerReady(playerInput);
	}
}
