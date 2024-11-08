using Managers;
using System.Collections;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public class SpeedType : LilGuyBase
{
	private Transform attackPos;
	[SerializeField] private float distance;

	private bool isDashing = false;
	private Coroutine dashCoroutine;
	private float dashLerpTime = 2f; // Time to smoothly transition back to normal speed
	private Vector3 dashDirection;
	private Vector3 dashStartPosition;

	public SpeedType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}

	public override void Special()
	{
		//TODO: ADD SPEED SPECIAL ATTACK
		if (currentCharges <= 0 && cooldownTimer > 0) return;

		Rigidbody rb = GameManager.Instance.CurrentPhase < 2 ? playerOwner.GetComponent<Rigidbody>() : GetComponent<Rigidbody>();
		dashDirection = playerOwner.GetComponent<PlayerBody>().MovementDirection.normalized;

		if (dashDirection == Vector3.zero) return; // No movement input

		// Track starting position and set dash state
		dashStartPosition = rb.position;
		isDashing = true;
		playerOwner.GetComponent<PlayerBody>().IsDashing = isDashing;

		// Apply initial force to start the dash
		float dashSpeed = speed * rb.velocity.magnitude; // Dash speed (tunable)
		rb.velocity = dashDirection * dashSpeed;

		// Decrease charges

		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;

	}

	private void FixedUpdate()
	{
		if (playerOwner != null)
			playerOwner.GetComponent<PlayerBody>().IsDashing = isDashing;
		if (!isDashing) return;
		// Calculate the distance traveled from the starting position
		Rigidbody rb = GameManager.Instance.CurrentPhase < 2 ? playerOwner.GetComponent<Rigidbody>() : GetComponent<Rigidbody>();
		float traveledDistance = Vector3.Distance(dashStartPosition, rb.position);

		if (traveledDistance >= distance)
		{
			// Stop the dash
			dashCoroutine ??= StartCoroutine(SmoothLerpVelocity(rb));
		}

	}
	private IEnumerator SmoothLerpVelocity(Rigidbody rb)
	{
		Vector3 initialVelocity = rb.velocity;
		Vector3 targetVelocity = playerOwner.GetComponent<PlayerBody>().MovementDirection.normalized * playerOwner.GetComponent<PlayerBody>().MaxSpeed;
		float elapsedTime = 0f;

		while (elapsedTime < dashLerpTime)
		{
			// Linearly interpolate between the current velocity and the target velocity
			rb.velocity = Vector3.Lerp(initialVelocity, targetVelocity, elapsedTime / dashLerpTime);
			elapsedTime += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		// Ensure the velocity ends exactly at the target
		rb.velocity = targetVelocity;
		isDashing = false;
	}
}
