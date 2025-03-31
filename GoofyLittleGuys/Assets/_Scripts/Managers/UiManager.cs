using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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

		[Header("Player UI")]
		[HorizontalRule]
		[SerializeField] public List<Sprite> shapes = new List<Sprite>();
		[SerializeField] List<PlayerUi> playerUis;              //List of PlayerUi prefab canvases 
		[ColoredGroup][SerializeField] private GameObject playerUiPrefab;     // Prefab for ingame player UI
        [SerializeField] private AudioSource uiAudioSource;

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
            GameManager.Instance.WaterChangeContainer.SwapColors( 
                GameManager.Instance.WaterChangeContainer.baseWaterColor,
                GameManager.Instance.WaterChangeContainer.baseLightFoamColor,
                GameManager.Instance.WaterChangeContainer.baseDarkFoamColor);
            
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
				Time.timeScale = 0;
				foreach (PlayerBody body in GameManager.Instance.Players)
                {
                    body.Controller.PlayerEventSystem.gameObject.SetActive(false);
				}

			}
            else
            {
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

        public void OpenSettingsMenu()
        {
            EventSystem.current.SetSelectedGameObject(settingsInitButton);
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
