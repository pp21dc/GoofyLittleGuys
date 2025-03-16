using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Windows;

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
	[SerializeField] private PlayerController controller;               // Reference to the player body of the player in control of this menu instance.
	[SerializeField] private MultiplayerEventSystem playerEventSystem;  // Reference to this player's multiplayer event system.
	[SerializeField] private Button characterSelectUnit;                        // The buttons on this menu.

	[SerializeField] private Animator lilGuyPreview;
	[SerializeField] private TMP_Text lilGuyName;
	[SerializeField] private Slider strengthSlider;
	[SerializeField] private Slider defenseSlider;
	[SerializeField] private Slider speedSlider;
	[SerializeField] private GameObject lockedInPanel;
	[SerializeField] private AnimationClip[] starterIdleAnims;


	public List<LilGuyBase> starters;                                   // List containing the starters the player can choose from.
	private int currStarterIndex = 0;
	private CharacterSelectState currentState;
	private bool lockedIn = false;

	public PlayerInput Player => player;

	public bool LockedIn { get { return lockedIn; } }


	private void Start()
	{
		
	}


	private void OnDestroy()
	{
		if (player == null) return;
		player.actions["Cancel"].performed -= OnCancelled;
		player.actions["Navigate"].performed -= OnNavigated;
		player.actions["Submit"].performed -= OnSubmitted;
	}

	public void SetPlayer(PlayerInput player)
	{
		this.player = player;
		controller = player.GetComponent<PlayerController>();
		playerEventSystem = controller.PlayerEventSystem;
		playerEventSystem.firstSelectedGameObject = characterSelectUnit.gameObject;

		this.player.SwitchCurrentActionMap("UI");
		this.player.actions["Cancel"].performed += OnCancelled;
		this.player.actions["Navigate"].performed += OnNavigated;
		this.player.actions["Submit"].performed += OnSubmitted;

		this.player.GetComponent<PlayerController>().HasJoined = false;
		ResetUI();

	}

	private void OnNavigated(InputAction.CallbackContext ctx)
	{
		if (lockedIn) return;
		Vector2 input = ctx.ReadValue<Vector2>();
		if (input.x < 0)
		{
			// Move left
			if (currStarterIndex - 1 < 0) currStarterIndex = starters.Count - 1;
			else currStarterIndex--;
		}
		else if (input.x > 0)
		{
			// Move right
			if (currStarterIndex + 1 > starters.Count - 1) currStarterIndex = 0;
			else currStarterIndex++;
		}
		ResetUI();
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

	void ResetUI()
	{
		lilGuyName.text = starters[currStarterIndex].name.ToUpper();
		lilGuyPreview.SetInteger("StarterIndex", currStarterIndex);

		float highestStat = Mathf.Max(starters[currStarterIndex].Strength, Mathf.Max(starters[currStarterIndex].Speed, starters[currStarterIndex].Defense));
  
        strengthSlider.maxValue = highestStat;
		defenseSlider.maxValue = highestStat;
		speedSlider.maxValue = highestStat;

		strengthSlider.value = starters[currStarterIndex].Strength;
		defenseSlider.value = starters[currStarterIndex].Defense;
		speedSlider.value = starters[currStarterIndex].Speed;
	}

	/// <summary>
	/// Called when the cancel action is pressed.
	/// </summary>
	/// <param name="ctx"></param>
	private void OnCancelled(InputAction.CallbackContext ctx)
	{
		if (!ctx.performed) return;
		switch (currentState)
		{
			case CharacterSelectState.CharacterSelect:
				DebugManager.Log(player.name + "is leaving the game.", DebugManager.DebugCategory.INPUT, DebugManager.LogLevel.LOG);				
				if (GameManager.Instance.Players.Count == 1)
				{
					foreach (var p in GameManager.Instance.Players)
					{
						Destroy(p.Controller.gameObject);
					}
					GameManager.Instance.Players.Clear();
					LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
				}
				MultiplayerManager.Instance.LeavePlayer(player);
				break;

			case CharacterSelectState.LockedIn:
				// If we're in the locked in state, go back to character select.
				currentState = CharacterSelectState.CharacterSelect;
				lockedIn = false;

				// Grab the lil guy this player had, and delete them.
				GameObject lilGuyToRemove = controller.Body.LilGuyTeam[0].gameObject;
				controller.Body.LilGuyTeam.RemoveAt(0);
				controller.Body.UnsubscribeActiveLilGuy();
				Destroy(lilGuyToRemove);

				// Unlock the player's button options again.
				SetButtonLockState(false);
				playerEventSystem.SetSelectedGameObject(characterSelectUnit.gameObject);
				break;

		}
	}

	/// <summary>
	/// Helper method that checks if all players currently in the game are locked in.
	/// </summary>
	/// <returns>True if all players are locked in, false if there's less than 2 players in the game, or at least one player isn't locked in.</returns>
	private bool CheckIfValidGameStart()
	{
		if (GameManager.Instance.Players.Count < 2) return false;
		if (!MultiplayerManager.Instance.CharacterSelectScreen.AllPlayersLockedIn()) return false;
		return true;
	}

	/// <summary>
	/// Helper method that sets the buttons on this menu to be enabled or disabled.
	/// </summary>
	/// <param name="lockState">Are the buttons to be enabled or disabled.</param>
	private void SetButtonLockState(bool lockState)
	{
		characterSelectUnit.interactable = (lockState == true) ? false : true;
		lockedInPanel.SetActive(lockState);
	}

	/// <summary>
	/// Method called when the player picks a lil guy from the list of lil guys in the menu.
	/// </summary>
	/// <param name="choice"></param>
	public void OnLockedIn()
	{
		lockedIn = true;
		currentState = CharacterSelectState.LockedIn;
		SetButtonLockState(true);

		// Create a new starter lil guy
		GameObject starterGO = Instantiate(starters[currStarterIndex].gameObject);
		LilGuyBase starter = starterGO.GetComponent<LilGuyBase>();

		starter.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		starter.SetFollowGoal(controller.Body.LilGuyTeamSlots[0].transform);
		starter.Init(LayerMask.NameToLayer("PlayerLilGuys"));
		starter.SetMaterial(GameManager.Instance.OutlinedLilGuySpriteMat);
		starterGO.transform.SetParent(controller.Body.transform, false);
		starterGO.GetComponent<Rigidbody>().isKinematic = true;
		starterGO.transform.localPosition = Vector3.zero;
		controller.Body.SetActiveLilGuy(starter);

		// Add the lil guy to the player's party.
		controller.Body.LilGuyTeam.Add(starter);
		controller.Body.LilGuyTeam[0].PlayerOwner = controller.Body;
		controller.Body.LilGuyTeamSlots[0].LilGuyInSlot = starter;

	}
}
