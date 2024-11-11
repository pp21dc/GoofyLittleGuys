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
        [SerializeField] private GameObject pauseScreen;        // The pause menu.
        [SerializeField] private EventSystem pauseEventSystem;  // The event system tied specifically to the pause menu.
        [SerializeField] private GameObject firstSelected;      // The first button in the menu to be selected on default
        [SerializeField] private GameObject playerUiPrefab;     // Prefab for ingame player UI

        [SerializeField] List<PlayerUi> playerUis;              //List of PlayerUi prefab canvases 
        private void Start()
        {
            EventManager.Instance.NotifyGamePaused += GamePaused;
        }

        /// <summary>
        /// Method called when the Resume button on the pause menu is pressed.
        /// </summary>
        public void OnResumePressed()
        {
            GameManager.Instance.IsPaused = false;
            pauseScreen.SetActive(GameManager.Instance.IsPaused);
            EnableAllPlayerInputs();
        }

        /// <summary>
        /// Method called when the Quit button in the pause menu is pressed.
        /// </summary>
        public void OnQuitToMainMenuPressed()
        {
            GameManager.Instance.IsPaused = false;
            pauseScreen.SetActive(GameManager.Instance.IsPaused);
            StartCoroutine(Quit());

            foreach (PlayerInput input in PlayerInput.all)
            {
                // Delete all player instances.
                Destroy(input.gameObject);
            }
            LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");

        }

        /// <summary>
        /// Coroutine that handles quit behaviour
        /// </summary>
        /// <returns></returns>
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
                playerInput.SwitchCurrentActionMap("World"); // Switch back to gameplay action map
            }
        }

        /// <summary>
        /// Method called when the game is paused.
        /// </summary>
        /// <param name="player">The player who paused the game</param>
        private void GamePaused(PlayerInput player)
        {
            pauseScreen.SetActive(GameManager.Instance.IsPaused);
            pauseEventSystem.GetComponent<InputSystemUIInputModule>().actionsAsset = player.actions;    // Set the UI input module's input to be the player who paused.

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
            playerUis[playerNumber].SetPersistentHealthBarValue(newHealthValue);
        }

        /// <summary>
        /// Adds player UI to list of player UIs
        /// </summary>
        private void AddPlayerUi()
        {
            GameObject ui = Instantiate(playerUiPrefab);
            playerUis.Add(ui.GetComponent<PlayerUi>());
        }
    }
}
