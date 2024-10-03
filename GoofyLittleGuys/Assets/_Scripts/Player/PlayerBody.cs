
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	[SerializeField]
	private List<LilGuyBase> lilGuyTeam;
	[SerializeField]
	private float fallMultiplier = 2.5f;
	[SerializeField]
	private float lowJumpMultiplier = 2f;
	[SerializeField, Range(1, 10)]
	private float jumpSpeed = 2f;
	[SerializeField]
	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;
	[SerializeField, ReadOnly]
	private Vector3 rbVelocity;
	public bool isJumping = false;

	public void UpdateMovementVector(Vector2 dir)
	{
		movementDirection = new Vector3(dir.x, 0, dir.y);
	}
	public void JumpPerformed(bool endJumpEarly)
	{
		isJumping = !endJumpEarly;
		rb.velocity += Vector3.up * jumpSpeed;
	}
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}
	private void FixedUpdate()
	{
		rbVelocity = rb.velocity;
		Vector3 newForceDirection = movementDirection * 100 * Time.fixedDeltaTime;
		rb.velocity += newForceDirection;

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
