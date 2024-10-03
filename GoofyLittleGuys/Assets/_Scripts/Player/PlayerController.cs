using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField]
	private PlayerBody playerBody;
	
	public void OnMove(InputAction.CallbackContext ctx)
	{
		playerBody.UpdateMovementVector(ctx.ReadValue<Vector2>());
	}
	public void OnSwap(InputAction.CallbackContext ctx)
	{
		playerBody.SwapLilGuy(ctx.ReadValue<float>());
	}
	public void OnPause(InputAction.CallbackContext ctx)
	{

	}
	public void OnJump(InputAction.CallbackContext ctx)
	{
		Debug.Log(ctx.duration);
		playerBody.JumpPerformed(ctx.duration);

	}
	public void OnPrimarySkill(InputAction.CallbackContext ctx)
	{

	}
	public void OnSecondarySkill(InputAction.CallbackContext ctx)
	{

	}
}
