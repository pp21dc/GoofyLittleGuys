using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	/// <summary>
	/// Called when play is pressed.
	/// </summary>
	public void OnPlayButtonPressed()
	{
		LevelLoadManager.Instance.LoadNewLevel("01_CharacterSelectMenu");

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
