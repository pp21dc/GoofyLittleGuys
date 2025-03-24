using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private PlayerBody playerBody;
	[ColoredGroup][SerializeField] private MultiplayerEventSystem playerEventSystem;
	[ColoredGroup][SerializeField] private Camera playerCam;
	[ColoredGroup][SerializeField] private Camera spectatorCam;

	[Header("Special Cancel Buffers")]
	[HorizontalRule]
	[ColoredGroup][SerializeField][Tooltip("Buffer time for special attack cancellation.")] private float turteriamSpecialBufferTime = 0.75f; // Time before the special can be canceled
	[ColoredGroup][SerializeField][Tooltip("Buffer time for special attack cancellation.")] private float toadstoolSpecialBufferTime = 1.5f; // Time before the special can be canceled


	private float lastSpecialTime = -1f; // Tracks when the last special started
	private int playerNumber = 0;
	private bool showTeamUI = true;
	private bool showMinimap = true;
	private bool hasJoined = false;
	private bool inTeamFullMenu = false;

	public int PlayerNumber { get { return playerNumber; } set => playerNumber = value; }
	public bool InTeamFullMenu { get { return inTeamFullMenu; } set { inTeamFullMenu = value; } }

	public bool HasJoined { get { return hasJoined; } set { hasJoined = value; } }

	public Camera PlayerCam => playerCam;
	public Camera SpectatorCam => spectatorCam;
	public PlayerBody Body => playerBody;
	public MultiplayerEventSystem PlayerEventSystem => playerEventSystem;

	private void Start()
	{
		UpdateCullLayer();
	}
	public void UpdateCullLayer()
	{
		LayerMask uiCullLayer;
		switch (GetComponent<PlayerInput>().currentControlScheme)
		{
			case "Gamepad":
				uiCullLayer = LayerMask.NameToLayer("UI_Gamepad");
				playerCam.cullingMask |= 1 << uiCullLayer; // Add layer to culling mask
				break;
			case "Keyboard":
				uiCullLayer = LayerMask.NameToLayer("UI_LeftKeyboard");
				playerCam.cullingMask |= 1 << uiCullLayer; // Add layer to culling mask
				break;
		}
	}

	public void OnBerryUsed(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;

		// If the player is not near a downed wild lil guy
		if (ctx.performed) playerBody.UseBerry();
	}
	public void OnMove(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		playerBody.UpdateMovementVector(ctx.ReadValue<Vector2>());
	}
	
	public void OnSpectatorUpDown(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		if (playerBody.IsDead && GameManager.Instance.CurrentPhase == 2)
			playerBody.UpdateUpDown(ctx.performed ? ctx.ReadValue<float>() : 0);
	}

	public void OnSwap(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		if (ctx.performed) playerBody.SwapLilGuy(ctx.ReadValue<float>());
	}

	public void OnPause(InputAction.CallbackContext ctx)
	{
		GameManager.Instance.IsPaused = true;
		EventManager.Instance.CallGamePaused(GetComponent<PlayerInput>());
	}

	public void OnShowHideTeamUI(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		showTeamUI = !showTeamUI;
		// Idk, invoke some event perhaps, passing showTeamUI
		// Event ties to the UI on this player's camera, and either shows or hides the UI based
		// on what showTeamUI evaluates to
	}

	public void OnShowHideMinimap(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		showMinimap = !showMinimap;
		// Idk, invoke some event perhaps, passing showMinimap
		// Event ties to the UI on this player's camera, and either shows or hides the UI based
		// on what showMinimap evaluates to
	}

	public void OnInteract(InputAction.CallbackContext ctx)
	{
		if (ctx.performed) playerBody.Interact();
		if (ctx.canceled) playerBody.StopInteract(); // Call stop when player releases input
	}



	public void OnPrimarySkill(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].Health <= 0) return;
		if (GetComponent<PlayerInput>().currentControlScheme == "Keyboard" && Keyboard.current.shiftKey.isPressed) return;
		
		
		if (ctx.performed) // Button Pressed (Start Holding)
		{
			playerBody.LilGuyTeam[0].StartAttacking();
		}
		else if (ctx.canceled) // Button Released (Stop Holding)
		{
			playerBody.LilGuyTeam[0].StopAttacking();
		}
	}

	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].Health <= 0) return;

		var character = playerBody.LilGuyTeam[0];

		if (ctx.started)
		{
			// Prevent canceling the special too quickly
			

			if (character is Turteriam turt && turt.InstantiatedDome != null)
			{
                if (Time.time - lastSpecialTime < turteriamSpecialBufferTime) return;
                turt.OnEndSpecial(true); // Remove the dome when canceling special
			}
			else if (character is Toadstool defenseCharacter && defenseCharacter.IsInSpecialAttack)
            {
                if (Time.time - lastSpecialTime < toadstoolSpecialBufferTime) return;
                defenseCharacter.OnEndSpecial(true); // End the special
			}
			else
			{
				lastSpecialTime = Time.time;
				character.StartChargingSpecial();
			}
		}
		else if (ctx.canceled)
		{
			character.StopChargingSpecial();
		}
	}


}
