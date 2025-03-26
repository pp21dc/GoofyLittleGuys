using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractCanvasController : MonoBehaviour
{
    [SerializeField] private InteractCanvas[] canvases;

	public InteractCanvas[] Canvases => canvases;

	public void SetCanvasStates(bool[] activeStates)
	{
		for (int i = 0; i < canvases.Length; i++)
		{
			if (canvases[i] != null)
				canvases[i].gameObject.SetActive(activeStates[i]);
		}
	}

	private void Awake()
	{
		for (int i = 0; i < GameManager.Instance.Players.Count; i++)
		{
			PlayerController controller = GameManager.Instance.Players[i].Controller;
			if (controller != null)
			{
				InputDisplayInfo info = controller.UIMappings.GetInfo("Interact");
				if (controller.ControlSchemeType == PlayerController.ControllerType.Gamepad)
				{
					canvases[i].Icon.sprite = info.gamepadIcon;
					canvases[i].KeyboardSymbol.text = "";
				}
				else
				{
					canvases[i].Icon.sprite = info.keyboardIcon;
					canvases[i].KeyboardSymbol.text = info.keyboardKeyName;
				}
			}
		}
	}
}
