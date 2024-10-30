using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private PlayerBody playerBody;

	private bool showTeamUI = true;
	private bool showMinimap = true;

	public void OnMove(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		playerBody.UpdateMovementVector(ctx.ReadValue<Vector2>());
	}

	public void OnSwap(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		playerBody.SwapLilGuy(ctx.ReadValue<float>());
	}

	public void OnPause(InputAction.CallbackContext ctx)
	{
		GameManager.Instance.IsPaused = true;
		EventManager.Instance.CallGamePaused(GetComponent<PlayerInput>());

	}

	public void OnShowHideTeamUI(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		showTeamUI = !showTeamUI;
		// Idk, invoke some event perhaps, passing showTeamUI
		// Event ties to the UI on this player's camera, and either shows or hides the UI based
		// on what showTeamUI evaluates to


	}
	public void OnShowHideMinimap(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		showMinimap = !showMinimap;
		// Idk, invoke some event perhaps, passing showMinimap
		// Event ties to the UI on this player's camera, and either shows or hides the UI based
		// on what showMinimap evaluates to


	}

	public void OnInteract(InputAction.CallbackContext ctx)
	{
		playerBody.HasInteracted = ctx.performed ? true : false; // If action performed, then true, otherwise false.
	}
	public void OnLeave(InputAction.CallbackContext ctx)
	{
		MultiplayerManager.Instance.LeavePlayer(GetComponent<PlayerInput>());
	}
	public void OnJump(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (ctx.started) playerBody.StartJumpBuffer();
		if (ctx.canceled) playerBody.IsJumping = false;
	}

	public void OnPrimarySkill(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].health <= 0) return;
		
		// Hold to keep attacking as opposed to mashing.
		playerBody.LilGuyTeam[0].Attack();
	}

	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].health <= 0) return;
		playerBody.LilGuyTeam[0].Special();
	}
}
