using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public class SpeedType : LilGuyBase
{
	private Transform attackPos;
	[SerializeField] private float distance;

	public SpeedType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
	{
	}

	public override void Special()
	{
		//TODO: ADD SPEED SPECIAL ATTACKif (currentCharges <= 0 || cooldownTimer > 0) return;
		Vector3 moveDirection = GetComponent<PlayerBody>().MovementDirection;
		moveDirection = moveDirection.normalized;
		Rigidbody rb = GetComponent<Rigidbody>();

		float acceleration = Mathf.Pow(moveDirection.magnitude * 0.1f * speed, 2) / (distance * 2);
		float forceMagnitude = rb.mass * acceleration;

		rb.AddForce(forceMagnitude * moveDirection, ForceMode.Impulse);
		currentCharges--;
	}
}
