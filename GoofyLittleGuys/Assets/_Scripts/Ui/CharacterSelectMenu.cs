using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CharacterSelectMenu : MonoBehaviour
{
	/// <summary>
	/// Enumeration type that defines what the current state of a player's Character Select is in.
	/// </summary>
	public enum CharacterSelectState
	{
		CharacterSelect,
		LockedIn
	}

	[SerializeField] private PlayerInput player;                        // The input of the player in control of this menu.
	[SerializeField] private PlayerBody body;							// Reference to the player body of the player in control of this menu instance.
	[SerializeField] private MultiplayerEventSystem playerEventSystem;	// Reference to this player's multiplayer event system.
	[SerializeField] private List<Button> buttons;						// The buttons on this menu.

	public List<LilGuyBase> starters;                                   // List containing the starters the player can choose from.
	private CharacterSelectState currentState;
	private bool lockedIn = false;

	public bool LockedIn { get { return lockedIn; } }


	private void Start()
	{
		player.actions["Cancel"].performed += OnCancelled;
		player.actions["Submit"].performed += OnSubmitted;
		EventManager.Instance.GameStarted += GameInit;
	}

	private void OnDisable()
	{
		player.actions["Cancel"].performed -= OnCancelled;
		player.actions["Submit"].performed -= OnSubmitted;
		EventManager.Instance.GameStarted -= GameInit;
	}

	/// <summary>
	/// Called when we leave characterSelect and go to the main game.
	/// </summary>
	void GameInit()
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Called when the submit button is pressed.
	/// </summary>
	/// <param name="ctx"></param>
	private void OnSubmitted(InputAction.CallbackContext ctx)
	{
		if (currentState == CharacterSelectState.LockedIn && CheckIfValidGameStart())
		{
			// If we are in the locked in state and every other player is locked in, we can start the game!
			EventManager.Instance.CallLilGuyLockedInEvent();
		}
	}
	
	/// <summary>
	/// Called when the cancel action is pressed.
	/// </summary>
	/// <param name="ctx"></param>
	private void OnCancelled(InputAction.CallbackContext ctx)
	{
		switch (currentState)
		{
			case CharacterSelectState.CharacterSelect:
				// If we're in character select, we're going back to the main menu
				foreach (PlayerInput input in PlayerInput.all)
				{
					// Destroy all player instances.
					Destroy(input.gameObject);
				}
				LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
				break;

			case CharacterSelectState.LockedIn:
				// If we're in the locked in state, go back to character select.
				currentState = CharacterSelectState.CharacterSelect;
				lockedIn = false;

				// Grab the lil guy this player had, and delete them.
				GameObject lilGuyToRemove = body.LilGuyTeam[0].gameObject;
				body.LilGuyTeam.RemoveAt(0);
				Destroy(lilGuyToRemove);

				// Unlock the player's button options again.
				SetButtonLockState(false);
				playerEventSystem.SetSelectedGameObject(buttons[0].gameObject);
				break;

		}
	}

	/// <summary>
	/// Helper method that checks if all players currently in the game are locked in.
	/// </summary>
	/// <returns>True if all players are locked in, false if there's less than 2 players in the game, or at least one player isn't locked in.</returns>
	private bool CheckIfValidGameStart()
	{
		if (PlayerInput.all.Count < 2) return false;
		foreach (PlayerInput input in PlayerInput.all)
		{
			if (!input.GetComponentInChildren<CharacterSelectMenu>().LockedIn) return false;
		}
		return true;
	}

	/// <summary>
	/// Helper method that sets the buttons on this menu to be enabled or disabled.
	/// </summary>
	/// <param name="lockState">Are the buttons to be enabled or disabled.</param>
	private void SetButtonLockState(bool lockState)
	{
		foreach (Button button in buttons)
		{
			button.interactable = !lockState;
		}
	}

	/// <summary>
	/// Method called when the player picks a lil guy from the list of lil guys in the menu.
	/// </summary>
	/// <param name="choice"></param>
	public void OnLockedIn(int choice)
	{
		lockedIn = true;
		currentState = CharacterSelectState.LockedIn;
		SetButtonLockState(true);

		// Create a new starter lil guy
		GameObject starter = Instantiate(starters[choice].gameObject);
		starter.GetComponent<LilGuyBase>().Init(LayerMask.NameToLayer("PlayerLilGuys"));
		starter.transform.SetParent(body.LilGuyTeamSlots[0].transform, false);  // false keeps local position/rotation
		starter.transform.localPosition = Vector3.zero;							// Ensure it’s positioned at the parent
		starter.GetComponent<Rigidbody>().isKinematic = true;

		// Add the lil guy to the player's party.
		body.LilGuyTeam.Add(starter.GetComponent<LilGuyBase>());
		body.LilGuyTeam[0].playerOwner = body.gameObject;

	}
}
