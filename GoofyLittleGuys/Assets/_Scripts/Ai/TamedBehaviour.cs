using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AiController))]
public class TamedBehaviour : MonoBehaviour
{
	[SerializeField] private float teleportRange = 30f; // Range to teleport if too far
	[SerializeField] private float accelerationTime = 0.02f;  // Time to reach target speed
	[SerializeField] private float followRange = 2f;
	private Vector3 currentVelocity = Vector3.zero;
	private Vector3 movementDirection = Vector3.zero;

	private bool flip = false;
	public bool Flip { get { return flip; } set { flip = value; } }


	private AiController controller;
	[SerializeField] private float stateChangeCooldown = 0.1f; // 100ms buffer
	private float timeSinceLastStateChange = 0f;
	private bool previousIsMovingState = false;

	private void UpdateAnimation()
	{
		// Check the magnitude of velocity
		float speed = controller.RB.velocity.magnitude;

		// Define a small threshold to consider the Lil Guy stationary
		float movementThreshold = 0.1f;

		if (speed > movementThreshold)
		{
			// Trigger running animation
			controller.LilGuy.IsMoving = true;
		}
		else
		{
			// Trigger idle animation
			controller.LilGuy.IsMoving = false;
		}
	}


	void Start()
	{
		controller = GetComponent<AiController>();
	}

	void Update()
	{
		// Flip character
		movementDirection = (controller.Player.position - transform.position).normalized;

		if (controller.RB.velocity.x > 0) controller.LilGuy.Flip = true;
		else if (controller.RB.velocity.x < 0) controller.LilGuy.Flip = false;

	}

	private void FixedUpdate()
	{
		if (controller.LilGuy == controller.Player.GetComponentInParent<PlayerBody>().ActiveLilGuy) return;

		if (controller.Player != null) FollowPlayer();
		UpdateAnimation();
	}

	/// <summary>
	/// Method that allows the tamed AI to move towards it's goal position
	/// </summary>
	private void FollowPlayer()
	{
		float distanceToPlayer = controller.DistanceToPlayer();

		if (distanceToPlayer > teleportRange)
		{
			// Teleport lil guy to the player
			transform.position = controller.Player.position;
			currentVelocity = Vector3.zero; // Reset velocity
		}
		else if (distanceToPlayer > followRange)
		{
			// Calculate movement direction
			movementDirection = (controller.Player.position - transform.position).normalized;

			// Calculate the target velocity
			float lilGuySpeed = Mathf.Max(
				controller.LilGuy.PlayerOwner.GetComponent<PlayerBody>().MaxSpeed,
				controller.LilGuy.Speed
			);
			Vector3 targetVelocity = movementDirection * lilGuySpeed;

			// Smoothly adjust current velocity
			currentVelocity = Vector3.Lerp(
				currentVelocity,
				targetVelocity,
				Time.fixedDeltaTime / accelerationTime
			);

			controller.RB.velocity = new Vector3(currentVelocity.x, controller.RB.velocity.y, currentVelocity.z);
		}
		else if (distanceToPlayer > 0.1f) // Add a stopping threshold
		{
			// Smoothly decelerate when close to follow range
			currentVelocity = Vector3.Lerp(
				currentVelocity,
				Vector3.zero,
				Time.fixedDeltaTime / accelerationTime
			);

			controller.RB.velocity = new Vector3(currentVelocity.x, controller.RB.velocity.y, currentVelocity.z);
		}
		else
		{
			// Fully stop if very close to the player
			currentVelocity = Vector3.zero;
			controller.RB.velocity = Vector3.zero;
		}
	}


}
