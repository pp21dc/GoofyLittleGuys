using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Managers
{
	public class UiManager : SingletonBase<UiManager>
	{
		[SerializeField] private GameObject pauseScreen;
		[SerializeField] private EventSystem pauseEventSystem;
		[SerializeField] private GameObject firstSelected;

		public void OnResumePressed()
		{
			GameManager.Instance.IsPaused = false;
			pauseScreen.SetActive(GameManager.Instance.IsPaused);
			EnableAllPlayerInputs();
		}
		public void OnQuitToMainMenuPressed()
		{
			GameManager.Instance.IsPaused = false;
			pauseScreen.SetActive(GameManager.Instance.IsPaused);
			StartCoroutine(Quit());
			foreach (PlayerInput input in PlayerInput.all)
			{
				Destroy(input.gameObject);
			}
			LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
			
		}

		private IEnumerator Quit()
		{
			for (int i = PlayerInput.all.Count - 1; i >= 0; i--)
			{
				Destroy(PlayerInput.all[i].gameObject);
				yield return null;
			}
			LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
			yield break;
		}
		private void Start()
		{
			EventManager.Instance.NotifyGamePaused += GamePaused;
		}
		private void DisableAllPlayerInputs()
		{
			foreach (var playerInput in FindObjectsOfType<PlayerInput>())
			{
				playerInput.DeactivateInput(); // Deactivate input for all players
			}
		}

		private void EnableAllPlayerInputs()
		{
			foreach (var playerInput in FindObjectsOfType<PlayerInput>())
			{
				playerInput.ActivateInput(); // Reactivate input for all players
				playerInput.SwitchCurrentActionMap("World"); // Switch back to gameplay action map
			}
		}

		private void GamePaused(PlayerInput player)
		{
			pauseScreen.SetActive(GameManager.Instance.IsPaused);
			pauseEventSystem.GetComponent<InputSystemUIInputModule>().actionsAsset = player.actions;
			DisableAllPlayerInputs();

			if (GameManager.Instance.IsPaused) player.SwitchCurrentActionMap("UI");
			else player.SwitchCurrentActionMap("World");
		}

	}
}
