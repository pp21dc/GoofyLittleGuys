using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


//TODO: Make this derive CaptureBase
public class DefenseMinigame : CaptureBase
{
	// Defense minigame specific
	[SerializeField] private TextMeshProUGUI countdownText;     // Countdown text
	[SerializeField] private GameObject throwObjectPrefab;      // The object to throw at the player
	[SerializeField] private Transform throwPoint;              // Center point from where objects will be thrown
	[SerializeField] private float gameDuration = 10f;          // Game duration
	[SerializeField] private float throwInterval = 1f;          // Interval between throws
	[SerializeField] private float throwSpeed = 10f;            // Speed of the thrown object
	[SerializeField] private float projectileLifetime = 3f;     // How long until the projectile despawns.


	private int missCount = 0;                                  // The number of missed projectils
	private int maxMisses = 3;                                  // The maximum number of missed projectiles a player can endure before they lose the minigame.


	private float gameTimer;                                    // The current time in the minigame
	private float gameStartTime;

	// Input action for player movement
	private InputAction moveAction;

	public int MissCount { get { return missCount; } set { missCount = value; } }

	private void OnEnable()
	{
		player.SwitchCurrentActionMap("DefenseMinigame");   // Swap to defense minigame control map
		moveAction = player.actions["Movement"];            // Because movement is so different in Defense minigame than regular, we're handling the inputs in this script directly using events.
		player.DeactivateInput();

		gameActive = true;
		missCount = 0;
		gameTimer = gameDuration;

		// Begin throwing objects at the player.
		StartCoroutine(StartCountdown());                   // Begin countdown
	}

	private void Update()
	{
		if (gameActive)
		{
			HandlePlayerMovement();
			CheckGameEndCondition();
		}
	}

	public override void Initialize(LilGuyBase creature)
	{
		base.Initialize(creature);
		throwPoint = lilGuyBeingCaught.attackPosition;

		// Initialize the player and lil guy positions to opposite ends of the play space.
		player.transform.position = instantiatedBarrier.transform.position - new Vector3(0, 0, spawnRadius);
		lilGuyBeingCaught.transform.position = instantiatedBarrier.transform.position + new Vector3(0, 0, spawnRadius);
		lilGuyBeingCaught.GetComponent<Rigidbody>().isKinematic = true;
	}

	/// <summary>
	/// Method that handles player input and translates it into left/right motion.
	/// </summary>
	private void HandlePlayerMovement()
	{
		Vector2 input = moveAction.ReadValue<Vector2>();
		Vector3 movement = new Vector3(input.x, 0, 0) * player.GetComponent<PlayerBody>().MaxSpeed * Time.deltaTime;
		player.transform.Translate(movement);

		// Clamp the player's x position if needed (adjust values based on desired boundaries)
		float clampedX = Mathf.Clamp(transform.position.x, -spawnRadius, spawnRadius);
		transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
	}

	/// <summary>
	/// Begins a countdown before the game actually begins.
	/// </summary>
	/// <returns></returns>
	private IEnumerator StartCountdown()
	{
		countdownText.text = "3";
		yield return new WaitForSeconds(1f);
		countdownText.text = "2";
		yield return new WaitForSeconds(1f);
		countdownText.text = "1";
		yield return new WaitForSeconds(1f);
		countdownText.text = "Go!";

		yield return new WaitForSeconds(1f);
		gameActive = true;
		player.ActivateInput();

		StartCoroutine(CountdownTimer());
		StartCoroutine(ThrowObjectsRoutine());
	}

	/// <summary>
	/// Keeps track of how much time the minigame has left and updates a timer in UI accordingly.
	/// </summary>
	/// <returns></returns>
	private IEnumerator CountdownTimer()
	{
		while (gameTimer > 0 && gameActive)
		{
			gameTimer -= Time.deltaTime;
			countdownText.text = gameTimer.ToString("F1");  // Show timer to one decimal point
			yield return null;
		}

	}
	/// <summary>
	/// Coroutine that throws a projectile at the player every 'throwInterval' seconds.
	/// </summary>
	/// <returns></returns>
	private IEnumerator ThrowObjectsRoutine()
	{
		while (gameActive && gameTimer > 0)
		{
			ThrowObject();
			yield return new WaitForSeconds(throwInterval);
		}
	}

	/// <summary>
	/// Method that handles throwing an object at the player.
	/// </summary>
	private void ThrowObject()
	{
		// Create a new thrown object at the throw point
		GameObject thrownObject = Instantiate(throwObjectPrefab, throwPoint.position + Vector3.back, Quaternion.identity);
		thrownObject.GetComponent<DefenseProjectile>().Init(player.gameObject, this);

		// Determine throw direction: center, left, or right
		Vector3 direction = Vector3.back;
		int throwType = Random.Range(0, 3); // 0 = center, 1 = left, 2 = right

		if (throwType == 1) direction = Quaternion.Euler(0, -20f, 0) * Vector3.back;		// Thrown left
		else if (throwType == 2) direction = Quaternion.Euler(0, 20f, 0) * Vector3.back;	// Thrown right

		// Apply velocity to the thrown object
		Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
		rb.velocity = direction * throwSpeed;

		// Destroy object if it goes past the player (indicating a miss)
		Destroy(thrownObject, projectileLifetime);
	}

	/// <summary>
	/// Method that checks to see if the win or lose condition for this game was met.
	/// </summary>
	private void CheckGameEndCondition()
	{
		// Missed too many projectiles, so the player lost.
		if (missCount > maxMisses) EndMinigame(false);		
		else if (gameTimer <= 0 && missCount <= 3)
		{
			// Player successfully defended for full duration
			EndMinigame(true);
		}
	}

	protected override void EndMinigame(bool playerWon)
	{
		StopAllCoroutines();
		base.EndMinigame(playerWon);
	}

	private void OnDisable()
	{
		countdownText.gameObject.SetActive(true);
		StopAllCoroutines();					// Stop any ongoing object throws
	}
}
