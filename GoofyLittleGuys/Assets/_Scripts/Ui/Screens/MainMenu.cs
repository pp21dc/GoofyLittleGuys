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

	void ToggleButtons(bool active)
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
