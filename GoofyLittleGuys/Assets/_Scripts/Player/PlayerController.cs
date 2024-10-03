using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{	public List<LilGuyBase> LilGuyTeam { get { return lilGuyTeam; } }

	[SerializeField]
	private PlayerBody playerBody;
	[SerializeField]
	private List<LilGuyBase> lilGuyTeam;


	/// <summary>
	/// Swaps the Lil guy based on a queue. If input is right, then the next one in list moves to position 1 and if left, the previous lil guy moves to position 1
	/// and the rest cascade accordingly.
	/// </summary>
	/// <param name="shiftDirection">The input provided by the D-Pad. Negative means they pressed left, and positive means they pressed right.</param>
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

	public void OnMove(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		playerBody.UpdateMovementVector(ctx.ReadValue<Vector2>());
	}
	public void OnSwap(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		SwapLilGuy(ctx.ReadValue<float>());
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
		if (lilGuyTeam[0].health <= 0) return;
		lilGuyTeam[0].Attack();
	}
	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{
		if (Managers.GameManager.Instance.IsPaused) return;
		if (lilGuyTeam[0].health <= 0) return;
		lilGuyTeam[0].Special();
	}
}
