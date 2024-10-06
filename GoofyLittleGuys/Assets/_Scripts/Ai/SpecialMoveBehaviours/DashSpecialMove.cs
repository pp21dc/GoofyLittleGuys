using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashSpecialMove : SpecialMoveBase
{
	[SerializeField]
	private float distance;
	public override void OnSpecialUsed()
	{
		if (currentCharges <= 0 || cooldownTimer > 0) return;
		lilGuy = GetComponent<LilGuyBase>();
		Vector3 moveDirection = GetComponent<PlayerBody>().MovementDirection;
		moveDirection = moveDirection.normalized;

		Rigidbody rb = GetComponent<Rigidbody>();

		float acceleration = Mathf.Pow(moveDirection.magnitude * 0.1f * lilGuy.speed, 2) / (distance * 2);
		float forceMagnitude = rb.mass * acceleration;

		rb.AddForce(forceMagnitude * moveDirection, ForceMode.Impulse);
		currentCharges--;
	}
}
