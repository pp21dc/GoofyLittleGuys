using Managers;
using System.Collections;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public class SpeedType : LilGuyBase
{
	[SerializeField] private float distance;

	private Coroutine dashCoroutine;
	private float dashLerpTime = 2f; // Time to smoothly transition back to normal speed
	private Vector3 dashDirection;
	private Vector3 dashStartPosition;

	public SpeedType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}

	public override void Special()
	{
		// Ensure we have charges available or the cooldown is active
		if (currentCharges <= 0 && cooldownTimer > 0) return;

		// Determine the Rigidbody based on the current game phase
		Rigidbody rb = GameManager.Instance.CurrentPhase < 2 ? playerOwner.GetComponent<Rigidbody>() : GetComponent<Rigidbody>();
		dashDirection = playerOwner.GetComponent<PlayerBody>().MovementDirection.normalized;

		// Ensure there’s movement input
		if (dashDirection == Vector3.zero) return;

		// Start dash in PlayerBody
		dashStartPosition = rb.position;
		playerOwner.GetComponent<PlayerBody>().StartDash();

		// Apply initial force to start the dash
		float dashSpeed = speed * rb.velocity.magnitude; // Tunable dash speed
		rb.velocity = dashDirection * dashSpeed;

		// Decrement charges and reset cooldowns
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
	}

	private void FixedUpdate()
	{
		if (playerOwner == null) return;

		// Get Rigidbody based on current game phase
		Rigidbody rb = GameManager.Instance.CurrentPhase < 2 ? playerOwner.GetComponent<Rigidbody>() : GetComponent<Rigidbody>();
		if (!playerOwner.GetComponent<PlayerBody>().IsDashing) return;

		// Check if the dash has reached its max distance
		float traveledDistance = Vector3.Distance(dashStartPosition, rb.position);
		if (traveledDistance >= distance)
		{
			// Stop dash smoothly
			dashCoroutine ??= StartCoroutine(SmoothLerpVelocity(rb));
		}
	}

	/// <summary>
	/// Smoothly slows the dash action down to regular speed.
	/// </summary>
	/// <param name="rb">The rigidbody the dash was acting on.</param>
	/// <returns></returns>
	private IEnumerator SmoothLerpVelocity(Rigidbody rb)
	{
		Vector3 initialVelocity = rb.velocity;
		Vector3 targetVelocity = playerOwner.GetComponent<PlayerBody>().MovementDirection.normalized * playerOwner.GetComponent<PlayerBody>().MaxSpeed;
		float elapsedTime = 0f;

		while (elapsedTime < dashLerpTime)
		{
			rb.velocity = Vector3.Lerp(initialVelocity, targetVelocity, elapsedTime / dashLerpTime);
			elapsedTime += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		// Finalize dash stop in PlayerBody
		rb.velocity = targetVelocity;
		playerOwner.GetComponent<PlayerBody>().StopDash();
	}
}
