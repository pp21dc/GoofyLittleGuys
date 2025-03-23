using Managers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StatCard : MonoBehaviour
{
	public TMP_Text playerNum;
	public TMP_Text titles;
	public TMP_Text stats;
	public TMP_Text ranking;
	public Image mostUsedIcon;
	public Image[] playerShapes;
	public Image background;
	public Sprite readyCheckmark;

	private PlayerInput playerInput;
	private bool isReady = false;

	public void Initialize(PlayerInput input)
	{
		playerInput = input;

		playerInput.actions["Submit"].performed += OnSubmit;
		playerInput.actions["Cancel"].performed += OnCancel;

		playerInput.SwitchCurrentActionMap("UI"); // Ensure they’re on the correct action map

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
		foreach (Image i in playerShapes)
		{
			i.sprite = readyCheckmark;
		}

		VictoryScreenManager.Instance.MarkPlayerReady(playerInput);
	}

	private void OnCancel(InputAction.CallbackContext ctx)
	{
		if (!isReady) return;

		isReady = false;
		foreach (Image i in playerShapes)
		{
			i.sprite = UiManager.Instance.shapes[playerInput.GetComponent<PlayerController>().PlayerNumber - 1];
		}

		VictoryScreenManager.Instance.UnmarkPlayerReady(playerInput);
	}
}
