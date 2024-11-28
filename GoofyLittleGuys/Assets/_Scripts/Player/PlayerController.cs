using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private PlayerBody playerBody;
	[SerializeField] private MultiplayerEventSystem playerEventSystem;
	[SerializeField] private Camera playerCam;

	private bool showTeamUI = true;
	private bool showMinimap = true;

	public Camera PlayerCam => playerCam;
	public PlayerBody Body => playerBody;
	public MultiplayerEventSystem PlayerEventSystem => playerEventSystem;

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
	}

	public void OnLeave(InputAction.CallbackContext ctx)
	{
		MultiplayerManager.Instance.LeavePlayer(GetComponent<PlayerInput>());
	}

	public void OnPrimarySkill(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].Health <= 0) return;

		// Hold to keep attacking as opposed to mashing.
		playerBody.LilGuyTeam[0].IsAttacking = ctx.performed;
	}

	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{
		if (GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].Health <= 0) return;
		if (ctx.performed) playerBody.LilGuyTeam[0].Special();
	}
}
