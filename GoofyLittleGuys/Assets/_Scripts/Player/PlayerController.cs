using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private PlayerBody playerBody;

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
		Managers.GameManager.Instance.IsPaused = !Managers.GameManager.Instance.IsPaused;
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
		playerBody.IsJumping = ctx.canceled ? false : true;
		playerBody.JumpPerformed();
	}

	public void OnPrimarySkill(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].health <= 0) return;
		playerBody.LilGuyTeam[0].Attack();
	}

	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (playerBody.LilGuyTeam[0].health <= 0) return;
		playerBody.LilGuyTeam[0].Special();
	}
}
