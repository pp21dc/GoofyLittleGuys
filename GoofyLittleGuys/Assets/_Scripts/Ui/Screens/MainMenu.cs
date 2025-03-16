using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private List<Button> buttons;


	private void OnEnable()
	{
		ToggleButtons(true);
	}
	/// <summary>
	/// Called when play is pressed.
	/// </summary>
	public void OnPlayButtonPressed()
	{
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
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}
}
