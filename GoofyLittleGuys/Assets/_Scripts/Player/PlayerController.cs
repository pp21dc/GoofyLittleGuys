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

	public void OnJump(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (ctx.canceled)
			playerBody.IsJumping = false;
		else
		{
			playerBody.IsJumping = true;
		}
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
