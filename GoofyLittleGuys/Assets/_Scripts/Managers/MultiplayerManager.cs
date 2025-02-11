using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace Managers
{
	public class MultiplayerManager : SingletonBase<MultiplayerManager>
	{
		[SerializeField] private GameObject blackoutCam;
		[SerializeField] private int characterSelectSceneIndex = 1; // Change to the build index of the char select screen.
		private CharacterSelectHandler characterSelectScreen;
		public CharacterSelectHandler CharacterSelectScreen { get { return characterSelectScreen; } set { characterSelectScreen = value; } }

		private GameObject instantiatedBlackoutCam = null;
		private bool canJoinLeave = false;
		public bool CanJoinLeave => canJoinLeave;

		// Start is called before the first frame update
		void Start()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			EventManager.Instance.NotifyGameOver += OnGameOver;
			canJoinLeave = CheckSceneIndex(SceneManager.GetActiveScene().buildIndex);

			if (canJoinLeave) PlayerInputManager.instance.EnableJoining();
			else PlayerInputManager.instance.DisableJoining();
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			EventManager.Instance.NotifyGameOver -= OnGameOver;
		}

		public void OnGameOver()
		{
			if (!ReferenceEquals(instantiatedBlackoutCam, null))
			{
				Destroy(instantiatedBlackoutCam );
			}
		}

		public void OnPlayerJoined(PlayerInput input)
		{
			if (!canJoinLeave) return;

			// Add the player to the game and manually assign the control scheme later
			GameManager.Instance.Players.Add(input.GetComponentInChildren<PlayerBody>());
			characterSelectScreen.OnPlayerJoin(input);

			input.GetComponent<PlayerController>().UpdateCullLayer();
			// Switch control scheme after setup to avoid conflicts

		}

		/// <summary>
		/// Method called from PlayerController.cs when the player presses the leave game input.
		/// </summary>
		/// <param name="player">The player that pressed leave</param>
		public void LeavePlayer(PlayerInput player)
		{
			if (!canJoinLeave) return;
			if (PlayerInput.all.Contains(player))
			{
				characterSelectScreen.OnPlayerLeft(player);
				// If this player exists in the list of players, remove them and adjust the screens.
			}

		}

		/// <summary>
		/// Helper method to adjust split screen positions and sizes when a player leaves.
		/// Scuffed because Unity doesn't have an innate way to 'remove players' from the list and update cams accordingly.
		/// </summary>
		public void AdjustCameraRects()
		{
			int playerCount = GameManager.Instance.Players.Count;

			if (playerCount > 2)
			{
				for (int i = 0; i < GameManager.Instance.Players.Count; i++)
				{
					GameManager.Instance.Players[i].Controller.PlayerCam.fieldOfView = 70;
					GameManager.Instance.Players[i].Controller.PlayerNumber = i + 1;
				}
			}
			else if (playerCount <= 2)
			{
				for (int i = 0; i < GameManager.Instance.Players.Count; i++)
				{
					GameManager.Instance.Players[i].Controller.PlayerCam.fieldOfView = 90;
					GameManager.Instance.Players[i].Controller.PlayerNumber = i + 1;
				}
			}
			if (playerCount == 3)
			{
				// Centering the 3rd player to the bottom of the screen, which is why it's not in a for loop
				GameManager.Instance.Players[2].Controller.PlayerCam.rect = new Rect(0f, 0f, 0.5f, 0.5f);
				GameManager.Instance.Players[1].Controller.PlayerCam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				GameManager.Instance.Players[0].Controller.PlayerCam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
				instantiatedBlackoutCam = Instantiate(blackoutCam);
				Camera cam = instantiatedBlackoutCam.GetComponent<Camera>();
				cam.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
			}
			else if (playerCount == 2)
			{
				// Half and half
				for (int i = 0; i < GameManager.Instance.Players.Count; i++)
				{
					GameManager.Instance.Players[i].Controller.PlayerCam.rect = new Rect(i * 0.5f, 0, 0.5f, 1f);
				}
			}
			else if (playerCount == 1)
			{
				// Full screen view
				GameManager.Instance.Players[0].Controller.PlayerCam.rect = new Rect(0, 0, 1f, 1f);
			}
		}

		/// <summary>
		/// Method called when the scene loaded event is triggered.
		/// </summary>
		/// <param name="scene">The loaded scene</param>
		/// <param name="mode">The way this scene was loaded (normal or additive)</param>
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			canJoinLeave = CheckSceneIndex(scene.buildIndex);

			// Disable joining if we are now in the game world.
			if (canJoinLeave) PlayerInputManager.instance.EnableJoining();
			else PlayerInputManager.instance.DisableJoining();
		}

		/// <summary>
		/// Compares the current scene index to the character select scene index
		/// </summary>
		/// <param name="currentSceneIndex">The current scene index</param>
		/// <returns>True if the current scene is or comes before the character select screen, otherwise false.</returns>
		private bool CheckSceneIndex(int currentSceneIndex)
		{
			return currentSceneIndex == characterSelectSceneIndex;
		}
	}
}
