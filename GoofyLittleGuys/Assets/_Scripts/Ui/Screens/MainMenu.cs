using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private List<Button> buttons;
	[SerializeField] private GameObject menuEventSystem;
	[SerializeField] private GameObject mainMenu;

	public GameObject MenuEventSystem => menuEventSystem;

	private void OnEnable()
	{
		ToggleButtons(true);
	}
	/// <summary>
	/// Called when play is pressed.
	/// </summary>
	public void OnPlayButtonPressed()
	{
		Managers.UiManager.Instance.PlayButtonPressSfx();
		ToggleButtons(false);
		LevelLoadManager.Instance.LoadNewLevel("01_CharacterSelectMenu");
	}

	public void OnSettingsButtonPressed()
	{
		ToggleButtons(false);
		mainMenu.SetActive(false);
		UiManager.Instance.SettingsMenu.SetActive(true);
		UiManager.Instance.SettingsMenu.GetComponentInChildren<SettingsController>().PreviousMenu = gameObject;
		menuEventSystem.SetActive(false);
	}

	public void ReinitializeMenu()
	{
		ToggleButtons(true);
		mainMenu.SetActive(true);
		menuEventSystem.SetActive(true);

		EventSystem eventSystem = menuEventSystem.GetComponent<EventSystem>();
		if (eventSystem != null && buttons.Count > 0)
		{
			eventSystem.SetSelectedGameObject(null); // Clear selection first
			eventSystem.SetSelectedGameObject(buttons[0].gameObject); // Actually reselect it
		}
	}


	public void ToggleButtons(bool active)
	{
		foreach (var button in buttons)
		{
			button.interactable = active;
		}
	}
	/// <summary>
	/// Called when Quit button is pressed. If we're in the editor, we just stop playmode.
	/// </summary>
	public void OnQuitButtonPressed()
	{
		Managers.UiManager.Instance.PlayButtonPressSfx();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}
}
