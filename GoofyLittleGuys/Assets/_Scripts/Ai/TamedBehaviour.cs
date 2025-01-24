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

	private bool flip = false;
	public bool Flip { get { return flip; } set { flip = value; } }


	private AiController controller;

	private void UpdateAnimation()
	{
		// Check the magnitude of velocity
		float speed = controller.LilGuy.RB.velocity.magnitude;

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
		if (controller.FollowPosition != null && !controller.LilGuy.RB.isKinematic) controller.LilGuy.MovementDirection = (controller.FollowPosition.position - transform.position).normalized;

	}

	private void FixedUpdate()
	{
		if (controller.FollowPosition == null) return;
		if (controller.LilGuy == controller.LilGuy.PlayerOwner.ActiveLilGuy) return;

		FollowPlayer();
		UpdateAnimation();
	}

	/// <summary>
	/// Method that allows the tamed AI to move towards it's goal position
	/// </summary>
	private void FollowPlayer()
	{
		float distanceToPlayer = controller.DistanceToPlayer();

		Vector3 velocity = controller.LilGuy.RB.velocity;
		velocity.y = 0;
		if (distanceToPlayer > teleportRange)
		{
			// Teleport lil guy to the player
			transform.position = controller.FollowPosition.position;
			currentVelocity = Vector3.zero; // Reset velocity
		}
		else if (distanceToPlayer > followRange)
		{
			// Calculate movement direction
			if (controller.FollowPosition != null && !controller.LilGuy.RB.isKinematic) controller.LilGuy.MovementDirection = (controller.FollowPosition.position - transform.position).normalized;

			// Calculate the target velocity
			float lilGuySpeed = controller.LilGuy.PlayerOwner.MaxSpeed;
			Vector3 targetVelocity = controller.LilGuy.MovementDirection * lilGuySpeed;

			// Smoothly adjust current velocity
			currentVelocity = Vector3.Lerp(
				currentVelocity,
				targetVelocity,
				Time.fixedDeltaTime / accelerationTime
			);
			controller.LilGuy.RB.velocity = new Vector3(currentVelocity.x, controller.LilGuy.RB.velocity.y, currentVelocity.z);
		}
		else if (distanceToPlayer > 0.1f) // Add a stopping threshold
		{
			// Smoothly decelerate when close to follow range
			currentVelocity = Vector3.Lerp(
				currentVelocity,
				Vector3.zero,
				Time.fixedDeltaTime / accelerationTime
			);
			controller.LilGuy.RB.velocity = new Vector3(currentVelocity.x, controller.LilGuy.RB.velocity.y, currentVelocity.z);
		}
		else
		{
			// Fully stop if very close to the player
			currentVelocity = Vector3.zero;
			controller.LilGuy.RB.velocity = Vector3.zero;
		}
	}


}
