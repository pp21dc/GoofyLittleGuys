using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Managers
{
	public class UiManager : SingletonBase<UiManager>
	{
		public enum Shapes
		{
			Circle,
			Hexagon,
			Diamond,
			Square
		};
		[Header("Pause Menu")]
		[HorizontalRule]
		[ColoredGroup][SerializeField] private GameObject pauseScreen;        // The pause menu.
		[ColoredGroup][SerializeField] private EventSystem pauseEventSystem;  // The event system tied specifically to the pause menu.
		[ColoredGroup][SerializeField] private GameObject firstSelected;      // The first button in the menu to be selected on default
		[ColoredGroup][SerializeField] private GameObject settingsInitButton;
		[ColoredGroup][SerializeField] private TMP_Text playerNumText;
		[ColoredGroup][SerializeField] private Image[] playerShapes;
		[ColoredGroup][SerializeField] private TMP_Text pauseTitle;

		[Header("Settings Menu")]
		[ColoredGroup][SerializeField] private GameObject settingsMenu;        // The pause menu.

		[Header("Player UI")]
		[HorizontalRule]
		[SerializeField] public List<Sprite> shapes = new List<Sprite>();
		[SerializeField] List<PlayerUi> playerUis;              //List of PlayerUi prefab canvases 
		[ColoredGroup][SerializeField] private GameObject playerUiPrefab;     // Prefab for ingame player UI
		[SerializeField] private AudioSource uiAudioSource;

		private PlayerInput playerWhoPaused;

		public GameObject SettingsMenu => settingsMenu;
		private void Start()
		{
			EventManager.Instance.NotifyGamePaused += GamePaused;
		}

		/// <summary>
		/// Method called when the Resume button on the pause menu is pressed.
		/// </summary>
		public void OnResumePressed()
		{
			PlayButtonPressSfx();
			GameManager.Instance.IsPaused = false;
			Time.timeScale = 1;
			foreach (PlayerBody body in GameManager.Instance.Players)
			{
				body.Controller.PlayerEventSystem.gameObject.SetActive(true);
			}
			pauseScreen.SetActive(GameManager.Instance.IsPaused);
			GameManager.Instance.TimerCanvas.SetActive(true);
			EnableAllPlayerInputs();
		}

		/// <summary>
		/// Method called when the Quit button in the pause menu is pressed.
		/// </summary>
		public void OnQuitToMainMenuPressed()
		{
			PlayButtonPressSfx();
			GameManager.Instance.IsPaused = false;
			pauseScreen.SetActive(GameManager.Instance.IsPaused);
			EventManager.Instance.CallGameOverEvent();
			// Reset water color for next game :3
			GameManager.Instance.WaterChangeContainer.ResetColors();

			for (int i = GameManager.Instance.Players.Count - 1; i >= 0; i--)
			{
				// Delete all player instances.
				Destroy(GameManager.Instance.Players[i].Controller.gameObject);
			}
			LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
			GameManager.Instance.PlayMainMenuMusic();
			GameManager.Instance.Players.Clear();
		}

		/// <summary>
		/// Method that disables every player's inputs.
		/// </summary>
		private void DisableAllPlayerInputs()
		{
			foreach (var playerInput in FindObjectsOfType<PlayerInput>())
			{
				playerInput.DeactivateInput(); // Deactivate input for all players
			}
		}

		/// <summary>
		/// Enables all player inputs and sets their action maps to world.
		/// </summary>
		private void EnableAllPlayerInputs()
		{
			foreach (var playerInput in FindObjectsOfType<PlayerInput>())
			{
				playerInput.ActivateInput(); // Reactivate input for all players
				if (playerInput.GetComponent<PlayerController>().InTeamFullMenu) playerInput.SwitchCurrentActionMap("UI");
				else playerInput.SwitchCurrentActionMap("World"); // Switch back to gameplay action map
			}
		}

		/// <summary>
		/// Method called when the game is paused.
		/// </summary>
		/// <param name="player">The player who paused the game</param>
		private void GamePaused(PlayerInput player)
		{			
			pauseScreen.SetActive(GameManager.Instance.IsPaused);
			if (GameManager.Instance.IsPaused)
			{
				GameManager.Instance.TimerCanvas.SetActive(false);
				playerWhoPaused = player;
				PlayerController pausedPlayerController = player.GetComponent<PlayerController>();
				Time.timeScale = 0;
				foreach(Image s in playerShapes)
				{
					s.sprite = shapes[pausedPlayerController.PlayerNumber - 1];
					s.color = pausedPlayerController.Body.PlayerColour;
				}
				playerNumText.text = $"Player {pausedPlayerController.PlayerNumber}";
				foreach (PlayerBody body in GameManager.Instance.Players)
				{
					body.Controller.PlayerEventSystem.gameObject.SetActive(false);
				}

			}
			else
			{
				GameManager.Instance.TimerCanvas.SetActive(true);
				playerWhoPaused = null;
				Time.timeScale = 1;
				foreach (PlayerBody body in GameManager.Instance.Players)
				{
					body.Controller.PlayerEventSystem.gameObject.SetActive(true);
				}
			}
			pauseEventSystem.GetComponent<InputSystemUIInputModule>().actionsAsset = player.actions;    // Set the UI input module's input to be the player who paused.
			pauseEventSystem.SetSelectedGameObject(null);
			pauseEventSystem.SetSelectedGameObject(firstSelected);

			DisableAllPlayerInputs();

			if (GameManager.Instance.IsPaused) player.SwitchCurrentActionMap("UI");
			else player.SwitchCurrentActionMap("World");
		}

		/// <summary>
		/// Method called when damage is taken to update the UI
		/// </summary>
		/// <param name="playerNumber">the player who paused the game</param>
		/// <param name="newHealthValue">the new value of the lil guy's health</param>
		private void ModifyHealth(int playerNumber, float newHealthValue)
		{

		}

		/// <summary>
		/// Adds player UI to list of player UIs
		/// </summary>
		private void AddPlayerUi()
		{
			GameObject ui = Instantiate(playerUiPrefab);
			playerUis.Add(ui.GetComponent<PlayerUi>());
		}

		public void OnSettingsButtonPressed()
		{
			SettingsMenu.SetActive(true);

			var settingsController = SettingsMenu.GetComponentInChildren<SettingsController>();
			settingsController.PreviousMenu = gameObject;

			var inputModule = SettingsMenu.GetComponentInChildren<InputSystemUIInputModule>();
			var pauseInputModule = pauseEventSystem.GetComponent<InputSystemUIInputModule>();

			inputModule.actionsAsset = pauseInputModule.actionsAsset;

			var tabGroup = SettingsMenu.GetComponentInChildren<TabGroupController>();
			var navigateRibbonAction = pauseInputModule.actionsAsset.FindAction("NavigateRibbon", true);
			if (tabGroup != null && navigateRibbonAction != null)
			{
				tabGroup.SetNavigateRibbonAction(navigateRibbonAction);
			}

			var playerVolume = playerWhoPaused.GetComponentInChildren<Volume>(); // however you get it
			settingsController.InitializeAsPlayer(playerVolume);


			pauseScreen.SetActive(false);
			pauseEventSystem.gameObject.SetActive(false);
		}

		public void ReinitializeMenu()
		{
			pauseScreen.SetActive(true);
			pauseEventSystem.gameObject.SetActive(true);

			pauseEventSystem.SetSelectedGameObject(null); // Clear selection first
			pauseEventSystem.SetSelectedGameObject(firstSelected); // Actually reselect it

		}

		public void PlayButtonHighlightSfx()
		{
			Managers.AudioManager.Instance.PlaySfx("Button_Highlight", uiAudioSource);
		}

		public void PlayButtonPressSfx()
		{
			Managers.AudioManager.Instance.PlaySfx("Button_Press", uiAudioSource);
		}
	}
}
