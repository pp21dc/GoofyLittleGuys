using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


//TODO: Make this derive CaptureBase
public class DefenseMinigame : CaptureBase
{
	// Defense minigame specific
	[SerializeField] private GameObject throwObjectPrefab; // The object to throw at the player
	[SerializeField] private Transform throwPoint; // Center point from where objects will be thrown
	[SerializeField] private float throwInterval = 1f; // Interval between throws
	[SerializeField] private float throwSpeed = 10f; // Speed of the thrown object
	private int missCount = 0;
	private int maxMisses = 3;
	private float gameDuration = 10f; // Duration of the minigame in seconds
	private float gameStartTime;

	// Input action for player movement
	private InputAction moveAction;


	public override void Initialize(LilGuyBase creature)
	{
		base.Initialize(creature);
		throwPoint = lilGuyBeingCaught.transform; 
	}
	private void OnEnable()
	{
		player.SwitchCurrentActionMap("DefenseMinigame");
		moveAction = player.actions["Move"];
		gameActive = true;
		missCount = 0;
		gameStartTime = Time.time;

		StartCoroutine(ThrowObjectsRoutine());
	}

	private void Update()
	{
		if (gameActive)
		{
			HandlePlayerMovement();
			CheckGameEndCondition();
		}
	}

	private void HandlePlayerMovement()
	{
		Vector2 input = moveAction.ReadValue<Vector2>();
		Vector3 movement = new Vector3(input.x, 0, 0) * player.GetComponent<PlayerBody>().MaxSpeed * Time.deltaTime;
		transform.Translate(movement);

		// Clamp the player's x position if needed (adjust values based on desired boundaries)
		float clampedX = Mathf.Clamp(transform.position.x, -5f, 5f);
		transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
	}

	private IEnumerator ThrowObjectsRoutine()
	{
		while (gameActive && Time.time - gameStartTime < gameDuration)
		{
			ThrowObject();
			yield return new WaitForSeconds(throwInterval);
		}
		if (gameActive) EndMinigame(true); // If still active, player has successfully defended
	}

	private void ThrowObject()
	{
		// Create a new thrown object at the throw point
		GameObject thrownObject = Instantiate(throwObjectPrefab, throwPoint.position, Quaternion.identity);

		// Determine throw direction: center, left, or right
		Vector3 direction = Vector3.forward;
		int throwType = Random.Range(0, 3); // 0 = center, 1 = left, 2 = right

		if (throwType == 1) // Left
			direction = Quaternion.Euler(0, -20f, 0) * Vector3.forward;
		else if (throwType == 2) // Right
			direction = Quaternion.Euler(0, 20f, 0) * Vector3.forward;

		// Apply velocity to the thrown object
		Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
		rb.velocity = direction * throwSpeed;

		// Destroy object if it goes past the player (indicating a miss)
		Destroy(thrownObject, 3f); // Adjust lifetime as necessary
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("ThrownObject"))
		{
			// Player successfully intercepts the object
			Destroy(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("ThrownObject"))
		{
			// Object missed the player
			missCount++;
			if (missCount >= maxMisses)
			{
				EndMinigame(false); // Player failed the minigame
			}
		}
	}

	private void CheckGameEndCondition()
	{
		if (Time.time - gameStartTime >= gameDuration)
		{
			EndMinigame(true); // Player successfully defended for full duration
		}
	}

	protected override void EndMinigame(bool playerWon)
	{
		StopAllCoroutines();
		base.EndMinigame(playerWon);
	}

	private void OnDisable()
	{
		player.SwitchCurrentActionMap("World");
		StopAllCoroutines(); // Stop any ongoing object throws
	}
}
