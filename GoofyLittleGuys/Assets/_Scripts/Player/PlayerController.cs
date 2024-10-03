using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField]
	private PlayerBody playerBody;
	[SerializeField]
	private List<LilGuyBase> lilGuyTeam;

	public void OnMove(InputAction.CallbackContext ctx)
	{
		playerBody.UpdateMovementVector(ctx.ReadValue<Vector2>());
	}
	public void OnSwap(InputAction.CallbackContext ctx)
	{
		SwapLilGuy(ctx.ReadValue<float>());
	}
	public void SwapLilGuy(float shiftDirection)
	{
		if (lilGuyTeam.Count <= 1) return;
		if (shiftDirection > 0)
		{
			LilGuyBase currentSelected = lilGuyTeam[0];
			for (int i = 1; i < lilGuyTeam.Count - 1; i++)
			{
				lilGuyTeam[i] = lilGuyTeam[i + 1];
			}
			lilGuyTeam[lilGuyTeam.Count - 1] = currentSelected;
		}
		else
		{
			LilGuyBase lastInTeam = lilGuyTeam[lilGuyTeam.Count - 1];
			for (int i = lilGuyTeam.Count - 1; i > 0; i++)
			{
				lilGuyTeam[i] = lilGuyTeam[i - 1];
			}
			lilGuyTeam[0] = lastInTeam;
		}
	}
	public void OnPause(InputAction.CallbackContext ctx)
	{

	}
	public void OnJump(InputAction.CallbackContext ctx)
	{
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

	}
	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{

	}
}
