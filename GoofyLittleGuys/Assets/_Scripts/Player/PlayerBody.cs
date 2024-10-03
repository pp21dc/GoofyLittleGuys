
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	[SerializeField]
	private LayerMask groundLayer;
	[SerializeField]
	private List<LilGuyBase> lilGuyTeam;
	[SerializeField]
	private float fallMultiplier = 2.5f;
	[SerializeField]
	private float lowJumpMultiplier = 2f;
	[SerializeField, Range(1, 10)]
	private float jumpSpeed = 2f;
	[SerializeField, Range(1, 10)]
	private float maxSpeed = 10f; // To be replaced with lilGuys[0].speed
	[SerializeField]
	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;
	public bool isJumping = false;

	public void UpdateMovementVector(Vector2 dir)
	{
		movementDirection = new Vector3(dir.x, 0, dir.y);
	}
	public void JumpPerformed(double durationHeld)
	{
		if (!IsGrounded()) return;
		rb.velocity += Vector3.up * jumpSpeed;
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
	private bool IsGrounded()
	{
		return Physics.Raycast(transform.position - Vector3.down * 0.1f, Vector3.down, 0.1f, groundLayer);
	}
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}
	private void FixedUpdate()
	{
		Vector3 targetVelocity = movementDirection * maxSpeed;
		Vector3 newForceDirection = (targetVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z));
		rb.velocity += newForceDirection * Time.fixedDeltaTime;
		if (rb.velocity.y < 0)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
		}
		else if (rb.velocity.y > 0 && !isJumping)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
		}
	}
}
