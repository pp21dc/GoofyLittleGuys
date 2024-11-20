using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AiController))]
public class TamedBehaviour : MonoBehaviour
{
	[SerializeField] private float teleportRange = 30f; // Range to teleport if too far
	private float followRange = 2f;
	[SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed
	private float decelerationTime = 0.2f;  // Time to stop
	private Vector3 currentVelocity = Vector3.zero;
	private Vector3 movementDirection = Vector3.zero;

	private bool flip = false;
	public bool Flip { get { return flip; } set { flip = value; } }


	private AiController controller;
	void Start()
	{
		controller = GetComponent<AiController>();
	}

	void Update()
	{
		// Flip character
		movementDirection = (controller.Player.position - controller.transform.position).normalized;

		if (controller.RB.velocity.x > 0) controller.LilGuy.Flip = true;
		else if (controller.RB.velocity.x < 0) controller.LilGuy.Flip = false;

	}

	private void FixedUpdate()
	{
		if (controller.LilGuy == controller.Player.GetComponentInParent<PlayerBody>().ActiveLilGuy)
		{
			return;
		}
		if (controller.Player != null) FollowPlayer();
		if (controller.RB.velocity.magnitude <= 0.1f)
			controller.LilGuy.IsMoving = false;
	}

	private void FollowPlayer()
	{
		if (controller.DistanceToPlayer() > teleportRange)
		{
			// Teleport lil guy to the player
			transform.position = controller.Player.position;
		}
		else if (controller.DistanceToPlayer() > followRange)
		{

			// Calculate the target velocity based on input direction
			Vector3 targetVelocity = movementDirection * controller.LilGuy.playerOwner.GetComponent<PlayerBody>().MaxSpeed;

			// Predict stopping distance
			float stoppingDistance = (currentVelocity.magnitude * currentVelocity.magnitude) / (2f * (1f / decelerationTime));
			float distanceToPlayer = controller.DistanceToPlayer();

			if (distanceToPlayer <= stoppingDistance + followRange)
			{
				// Start decelerating earlier
				currentVelocity = Vector3.Lerp(
					currentVelocity,
					Vector3.zero,
					Time.fixedDeltaTime / decelerationTime
				);
			}
			else
			{
				controller.LilGuy.IsMoving = true;
				// Smoothly accelerate towards the target velocity
				currentVelocity = Vector3.Lerp(
					currentVelocity,
					targetVelocity,
					Time.fixedDeltaTime / accelerationTime
				);
			}

			controller.RB.velocity = new Vector3(currentVelocity.x, controller.RB.velocity.y, currentVelocity.z);
		}
	}
}
