
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBody : MonoBehaviour
{
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private List<LilGuyBase> lilGuyTeam;
	[SerializeField] private float fallMultiplier = 4f;      // The default fall speed of the player
	[SerializeField] private float lowJumpMultiplier = 6f;   // The fall speed for when the player ends their jump early (allows for shorter jumps)
	[SerializeField, Range(1, 10)] private float jumpSpeed = 4f;
	[SerializeField, Range(1, 25f)] private float maxSpeed = 25f;           // To be replaced with lilGuys[0].speed

	private bool isJumping = false;
	private bool hasInteracted = false;
	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;

	public bool HasInteracted {  get { return hasInteracted; } set { hasInteracted =value; } }
	public bool IsJumping { get { return isJumping; } set { isJumping = value; } }
	public Vector3 MovementDirection { get { return movementDirection; } }
	public List<LilGuyBase> LilGuyTeam { get { return lilGuyTeam; } }
	public void UpdateMovementVector(Vector2 dir)
	{
		movementDirection = new Vector3(dir.x, 0, dir.y);
	}



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

	public void JumpPerformed()
	{
		if (!IsGrounded()) return;
		rb.velocity += Vector3.up * jumpSpeed;
	}

	private bool IsGrounded()
	{
		return Physics.Raycast(transform.position - Vector3.down * 0.05f, Vector3.down, 0.1f, groundLayer);
	}
	private void Init()
	{
		GetComponent<PlayerInput>().camera.clearFlags = CameraClearFlags.Skybox;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();

		EventManager.Instance.GameStarted += Init;
	}

	private void FixedUpdate()
	{
		Vector3 targetVelocity = movementDirection.normalized * maxSpeed;
		Vector3 newForceDirection = (targetVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z));
		rb.velocity += newForceDirection * Time.fixedDeltaTime;

		// Jump behaviours
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
