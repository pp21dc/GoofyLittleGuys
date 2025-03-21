using Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum CharacterSelectState
{
	Disconnected,
	CharacterSelect,
	LockedIn,
	Tutorial
}

public class UISelector : MonoBehaviour
{
	/// <summary>
	/// Enumeration type that defines what the current state of a player's Character Select is in.
	/// </summary>
	

	[SerializeField] private PlayerInput player;                        // The input of the player in control of this menu.
	[SerializeField] private PlayerController controller;               // Reference to the player body of the player in control of this menu instance.
	[SerializeField] private TMP_Text selectorText;


	[SerializeField] private GameObject lockedInPanel;
	[SerializeField] private Image[] playerColourIndicators;
	[SerializeField] private Image[] playerShapeIndicators;
	[SerializeField] private Sprite checkmarkSprite;

	private int currStarterIndex = 0;
	private CharacterSelectState currentState = CharacterSelectState.CharacterSelect;
	private CharacterSelectHandler charSelectMenu;
	private bool lockedIn = false;

	private PlayerCard card;
	public PlayerInput Player => player;
	public PlayerController Controller => controller;

	public int CurrentStarterIndex => currStarterIndex;
	public bool LockedIn { get { return lockedIn; } }

	private float bufferTime = 0.5f;

	public void UpdateColours()
	{
		int playerNum = player.GetComponent<PlayerController>().PlayerNumber - 1;
		foreach (Image image in playerColourIndicators)
		{
			image.color = GameManager.Instance.PlayerColours[playerNum];
		}
		foreach (Image image in playerShapeIndicators)
		{
			image.color = GameManager.Instance.PlayerColours[playerNum];
			image.sprite = UiManager.Instance.shapes[playerNum];
		}
		if (currentState == CharacterSelectState.LockedIn) playerShapeIndicators[0].sprite = checkmarkSprite;
		selectorText.text = $"P{playerNum + 1}";
		UpdateCard();
	}

	public void UpdateCard()
	{
		int playerNum = player.GetComponent<PlayerController>().PlayerNumber - 1;
		card = charSelectMenu.PlayerCards[playerNum];
		card.UpdateState(currentState, playerNum);
	}
	private void OnDestroy()
	{
		if (player == null) return;
		player.actions["Cancel"].performed -= OnCancelled;
		player.actions["Navigate"].performed -= OnNavigated;
		player.actions["Navigate"].performed -= OnNavigateTutorialPrompt;
		player.actions["Submit"].performed -= OnSubmitted;
		controller.PlayerEventSystem.enabled = true;
	}

	public void SetPlayer(PlayerInput player, CharacterSelectHandler menu)
	{
		this.player = player;
		charSelectMenu = menu;
		controller = player.GetComponent<PlayerController>();
		controller.PlayerEventSystem.enabled = false;


		this.player.actions["Cancel"].performed += OnCancelled;
		this.player.actions["Navigate"].performed += OnNavigated;
		this.player.actions["Submit"].performed += OnSubmitted;

		this.player.SwitchCurrentActionMap("UI");
		CoroutineRunner.Instance.StartCoroutine(Buffer());

		this.player.GetComponent<PlayerController>().HasJoined = true;
		UpdateColours();
	}

	private IEnumerator Buffer()
	{
		while (bufferTime > 0)
		{
			bufferTime -= Time.unscaledDeltaTime;
			yield return null;
		}
	}
	private void OnNavigated(InputAction.CallbackContext ctx)
	{
		if (lockedIn) return;
		if (!ctx.performed || bufferTime > 0) return;
		Vector2 input = ctx.ReadValue<Vector2>();
		if (input.x < 0)
		{
			// Move left
			if (currStarterIndex - 1 < 0) currStarterIndex = charSelectMenu.starters.Count - 1;
			else currStarterIndex--;
		}
		else if (input.x > 0)
		{
			// Move right
			if (currStarterIndex + 1 > charSelectMenu.starters.Count - 1) currStarterIndex = 0;
			else currStarterIndex++;
		}

		MoveSelector();
	}

    private void OnNavigateTutorialPrompt(InputAction.CallbackContext ctx)
	{
		var input = ctx.ReadValue<Vector2>();
		if (input.x < 0) //left
		{
            controller.PlayerEventSystem.SetSelectedGameObject(null);
			controller.PlayerEventSystem.SetSelectedGameObject(charSelectMenu.TutorialPromptButtons[0]);
        }
		else if (input.x > 0) //right
		{
            controller.PlayerEventSystem.SetSelectedGameObject(null);
            controller.PlayerEventSystem.SetSelectedGameObject(charSelectMenu.TutorialPromptButtons[1]);
        }
			
	}

    /// <summary>
    /// Called when the submit button is pressed.
    /// </summary>
    /// <param name="ctx"></param>
    private void OnSubmitted(InputAction.CallbackContext ctx)
	{
		if (!ctx.performed || bufferTime > 0) return;
		if (currentState == CharacterSelectState.LockedIn && CheckIfValidGameStart())
		{
			// If we are in the locked in state and every other player is locked in, we prompt if they want the tutorial
			InTutorialPrompt();
		}
		else if (currentState == CharacterSelectState.CharacterSelect)
		{
			OnLockedIn();
		}
		else if (currentState == CharacterSelectState.Tutorial)
		{
			controller.PlayerEventSystem.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
		}
	}

	private void InTutorialPrompt()
	{
		currentState = CharacterSelectState.Tutorial;
		
		charSelectMenu.TutorialPrompt.SetActive(true);
		controller.PlayerEventSystem.SetSelectedGameObject(null);
		StartCoroutine(DelayButtonSelect());

		//player.actions["Submit"].performed -= OnSubmitted;
		player.actions["Navigate"].performed -= OnNavigated;
		player.actions["Navigate"].performed += OnNavigateTutorialPrompt;
	}
	
	private IEnumerator DelayButtonSelect()
	{
		yield return null;
		controller.PlayerEventSystem.SetSelectedGameObject(charSelectMenu.TutorialPromptButtons[0]);
	}

	private void MoveSelector()
	{
		charSelectMenu.LilGuySelectorParents[currStarterIndex].SetSelectorContainer(gameObject, controller.PlayerNumber - 1);
	}

	/// <summary>
	/// Called when the cancel action is pressed.
	/// </summary>
	/// <param name="ctx"></param>
	private void OnCancelled(InputAction.CallbackContext ctx)
	{
		if (!ctx.performed || bufferTime > 0) return;
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

					player.actions["Cancel"].performed -= OnCancelled;
					player.actions["Navigate"].performed -= OnNavigated;
					player.actions["Submit"].performed -= OnSubmitted;
					
					if (!LevelLoadManager.Instance.IsLoadingLevel) LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
				}
				card.UpdateState(CharacterSelectState.Disconnected);
				MultiplayerManager.Instance.LeavePlayer(player);
				break;

			case CharacterSelectState.LockedIn:
				// If we're in the locked in state, go back to character select.
				currentState = CharacterSelectState.CharacterSelect;
				lockedIn = false;
				UpdateColours();
				UpdateCard();

				// Grab the lil guy this player had, and delete them.
				GameObject lilGuyToRemove = controller.Body.LilGuyTeam[0].gameObject;
				controller.Body.LilGuyTeam.RemoveAt(0);
				controller.Body.UnsubscribeActiveLilGuy();
				Destroy(lilGuyToRemove);
				break;
			
			case CharacterSelectState.Tutorial:
				charSelectMenu.TutorialPrompt.SetActive(false);

				//player.actions["Submit"].performed += OnSubmitted;
				player.actions["Navigate"].performed += OnNavigated;
				player.actions["Navigate"].performed -= OnNavigateTutorialPrompt;
				currentState = CharacterSelectState.LockedIn;
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
	/// Method called when the player picks a lil guy from the list of lil guys in the menu.
	/// </summary>
	/// <param name="choice"></param>
	public void OnLockedIn()
	{
		lockedIn = true;
		currentState = CharacterSelectState.LockedIn;
		UpdateColours();


		// Create a new starter lil guy
		GameObject starterGO = Instantiate(charSelectMenu.starters[currStarterIndex].gameObject);
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
