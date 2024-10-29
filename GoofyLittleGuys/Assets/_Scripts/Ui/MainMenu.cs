using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public void OnPlayButtonPressed()
	{
		LevelLoadManager.Instance.LoadNewLevel("01_CharacterSelectMenu");

	}
	public void OnQuitButtonPressed()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}
}
