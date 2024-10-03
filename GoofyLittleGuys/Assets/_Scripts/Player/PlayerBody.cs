
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	public bool IsJumping { get { return isJumping; } set { isJumping = value; } }

	[SerializeField]
	private LayerMask groundLayer;

	[SerializeField]
	private float fallMultiplier = 4f;		// The default fall speed of the player
	[SerializeField]
	private float lowJumpMultiplier = 6f;	// The fall speed for when the player ends their jump early (allows for shorter jumps)
	[SerializeField, Range(1, 10)]
	private float jumpSpeed = 4f;
	private bool isJumping = false;


	[SerializeField, Range(1, 10)]
	private float maxSpeed = 10f;			// To be replaced with lilGuys[0].speed
	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;

	public void UpdateMovementVector(Vector2 dir)
	{
		movementDirection = new Vector3(dir.x, 0, dir.y);
	}
	public void JumpPerformed()
	{
		if (!IsGrounded()) return;
		rb.velocity += Vector3.up * jumpSpeed;
	}
	private bool IsGrounded()
	{
		return Physics.Raycast(transform.position - Vector3.down * 0.05f, Vector3.down, 0.1f, groundLayer);
	}
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}
	private void FixedUpdate()
	{
		Vector3 targetVelocity = movementDirection.normalized * maxSpeed;
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
