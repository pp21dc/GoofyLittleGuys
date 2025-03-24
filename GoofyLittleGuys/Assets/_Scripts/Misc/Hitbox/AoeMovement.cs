using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeMovement : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private LayerMask groundLayer; // LayerMask to identify terrain or ground

	[Header("Movement Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float speed = 5f;
	[ColoredGroup][SerializeField] private float raycastHeightOffset = 1f; // How high above to start the raycast
	[ColoredGroup][SerializeField] private float smoothTime = 0.1f; // For smoothing position adjustments


	private Rigidbody rb;
	private Vector3 velocity = Vector3.zero;
	private Vector3 forwardDir;

	public float Speed { set {  speed = value; } }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		forwardDir = transform.right;
	}
	void Update()
	{
		// Move the wind wall forward
		Vector3 forwardMovement = forwardDir * -speed * Time.deltaTime;
		Vector3 targetPosition = transform.position + forwardMovement;

		// Adjust height to follow the terrain
		Ray ray = new Ray(targetPosition + Vector3.up * raycastHeightOffset, Vector3.down);
		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
		{
			targetPosition.y = hit.point.y; // Align with terrain height
		}

		// Smooth movement to avoid jitter
		rb.MovePosition(Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime));
	}
}
