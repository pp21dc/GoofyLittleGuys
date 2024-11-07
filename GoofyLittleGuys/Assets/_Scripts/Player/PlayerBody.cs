
using Managers;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerBody : MonoBehaviour
{
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private List<LilGuyBase> lilGuyTeam;
	[SerializeField] private List<GameObject> lilGuyTeamSlots;
	[SerializeField] private GameObject playerMesh;
	[SerializeField] private PlayerInput playerInput;
	[SerializeField] private GameObject lastHitPromptUI;

	[SerializeField, Range(1, 25f)] private float maxSpeed = 25f;           // To be replaced with lilGuys[0].speed
	[SerializeField] private float movementDeceleration = 0.9f;           // To be replaced with lilGuys[0].speed
	[Header("Jump Parameters")]

	[SerializeField, Range(1, 25)] private float jumpSpeed = 4f;

	[Tooltip("EDIT THIS IF HIGH JUMP FEELS FLOATY!\nUpward velocity threshold before increased gravity multiplier gets applied.")]
	[SerializeField] private float fallThresholdSpeed = 0f;

	[Tooltip("Gravity modifier that makes falling faster, and allows for short jumps.")]
	[SerializeField] private float fallMultiplier = 4f;

	[Tooltip("How long (in seconds) after leaving the ground can a jump input be accepted?")]
	[SerializeField] private float coyoteTime = 0.2f;   // How long after leaving the ground can a jump input be accepted

	[Tooltip("How long (in seconds) a jump input is 'remembered' for.")]
	[SerializeField] private float jumpBufferTime = 0.2f;   // How long the jump input is 'remembered' for

	private float coyoteTimeCounter;
	private float jumpbufferCounter;
	private bool isJumping = false;
	private bool flip = false;

	private bool hasInteracted = false;
	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;

	public float MaxSpeed { get { return maxSpeed; } }
	public bool HasInteracted { get { return hasInteracted; } set { hasInteracted = value; } }
	public bool IsJumping { get { return isJumping; } set { isJumping = value; } }
	public Vector3 MovementDirection { get { return movementDirection; } }
	public List<LilGuyBase> LilGuyTeam { get { return lilGuyTeam; } }
	public List<GameObject> LilGuyTeamSlots { get { return lilGuyTeamSlots; } }

	public void UpdateMovementVector(Vector2 dir)
	{
		movementDirection = new Vector3(dir.x, 0, dir.y);
		if (dir.x > 0) flip = true;
		if (dir.x < 0) flip = false;
	}

	/// <summary>
	/// Swaps the Lil guy based on a queue. If input is right, then the next one in list moves to position 1 and if left, the previous lil guy moves to position 1
	/// and the rest cascade accordingly.
	/// </summary>
	/// <param name="shiftDirection">The input provided by the D-Pad. Negative means they pressed left, and positive means they pressed right.</param>
	public void SwapLilGuy(float shiftDirection)
	{
		if (lilGuyTeam.Count <= 1) return;

		if (shiftDirection < 0)
		{
			// Store the first element to rotate it to the end
			LilGuyBase currentSelected = lilGuyTeam[0];
			Transform initialParent;

			// Shift elements left
			for (int i = 0; i < lilGuyTeam.Count - 1; i++)
			{
				initialParent = lilGuyTeam[i].transform.parent;			//1th guy parent
				lilGuyTeam[i + 1].transform.SetParent(initialParent);	// 2st guy parent is now 1th guy parent
				lilGuyTeam[i] = lilGuyTeam[i + 1];						//1th team pos holds 2st guy
				lilGuyTeam[i].transform.localPosition = Vector3.zero;	//Reset pos

			}
			currentSelected.transform.SetParent(lilGuyTeamSlots[lilGuyTeam.Count - 1].transform);
			lilGuyTeam[lilGuyTeam.Count - 1] = currentSelected;
			lilGuyTeam[lilGuyTeam.Count - 1].transform.localPosition = Vector3.zero;
		}
		else if (shiftDirection > 0)
		{
			// Store the last element to rotate it to the beginning
			LilGuyBase lastInTeam = lilGuyTeam[lilGuyTeam.Count - 1];	// last team lil guy
			Transform lastParent;

			// Shift elements right
			for (int i = lilGuyTeam.Count - 1; i > 0; i--)
			{
				lastParent = lilGuyTeam[i].transform.parent;        //1nd guy parent
				lilGuyTeam[i - 1].transform.SetParent(lastParent);	//0st guy parent is now 1nd guy parent
				lilGuyTeam[i] = lilGuyTeam[i - 1];					//1nd team pos now holds 0st guy
				lilGuyTeam[i].transform.localPosition = Vector3.zero; //Reset local pos

			}

			lastInTeam.transform.SetParent(lilGuyTeamSlots[0].transform);
			lilGuyTeam[0] = lastInTeam;
			lilGuyTeam[0].transform.localPosition = Vector3.zero;
		}
	}

	/// <summary>
	/// Called to do the actual jump force application.
	/// </summary>
	public void PerformJump()
	{
		if (IsGrounded())
		{
			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
			Managers.AudioManager.Instance.PlaySfx("Jump",gameObject.GetComponent<AudioSource>());
		}
	}

	/// <summary>
	/// Called when the jump button is pressed to start the jump buffer.
	/// </summary>
	public void StartJumpBuffer()
	{
		isJumping = true;
		jumpbufferCounter = jumpBufferTime;
	}

	private bool IsGrounded()
	{
		return Physics.Raycast(transform.position - Vector3.down * 0.05f, Vector3.down, 0.1f, groundLayer);
	}

	/// <summary>
	/// Called when the GameStarted event is invoked (occurs after character select, and phase one is loaded).
	/// </summary>
	private void Init()
	{
		playerInput.camera.clearFlags = CameraClearFlags.Skybox;
		GetComponentInChildren<MultiplayerEventSystem>().firstSelectedGameObject = null;
		GetComponentInChildren<MultiplayerEventSystem>().gameObject.SetActive(false);
		playerMesh.SetActive(true);
	}

	public void ShowLastHitPrompt(LilGuyBase lilGuy)
	{
		lastHitPromptUI.GetComponent<LastHitMenu>().Initialize(lilGuy);
		lastHitPromptUI?.SetActive(true); // Activate the UI element if not null
		EnableUIControl();
	}

	// Enable UI control for the player
	public void EnableUIControl()
	{
		playerInput.SwitchCurrentActionMap("UI"); // Switch to UI input map for menu control
	}

	// Call this when the player exits the UI
	public void DisableUIControl()
	{
		playerInput.SwitchCurrentActionMap("World"); // Switch back to gameplay controls
		lastHitPromptUI?.SetActive(false); // Deactivate the UI element

	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		EventManager.Instance.GameStarted += Init;
	}

	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= Init;
	}

	private bool CheckTeamHealth()
	{
		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			if (lilGuyTeam[i].health > 0) return true;
		}
		return false;
	}
	private void Respawn()
	{
		transform.position = GameManager.Instance.FountainSpawnPoint.position;
		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			lilGuyTeam[i].health = lilGuyTeam[i].maxHealth;
		}
	}
	private void FixedUpdate()
	{
		if (flip) playerMesh.transform.rotation = Quaternion.Euler(0, 180, 0);
		else playerMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
		if (lilGuyTeam[0].health <= 0)
		{
			if (CheckTeamHealth())
			{
				LilGuyBase currentSelected = lilGuyTeam[0];
				Transform initialParent;

				// Shift elements left
				for (int i = 0; i < lilGuyTeam.Count - 1; i++)
				{
					initialParent = lilGuyTeam[i].transform.parent;         //1th guy parent
					lilGuyTeam[i + 1].transform.SetParent(initialParent);   // 2st guy parent is now 1th guy parent
					lilGuyTeam[i] = lilGuyTeam[i + 1];                      //1th team pos holds 2st guy
					lilGuyTeam[i].transform.localPosition = Vector3.zero;   //Reset pos

				}
				currentSelected.transform.SetParent(lilGuyTeamSlots[lilGuyTeam.Count - 1].transform);
				lilGuyTeam[lilGuyTeam.Count - 1] = currentSelected;
				lilGuyTeam[lilGuyTeam.Count - 1].transform.localPosition = Vector3.zero;
			}
			else
			{
				Respawn();
			}
		}

		// Movement behaviours


		// Reduce gliding when there’s no movement input
		if (movementDirection.magnitude < 0.1f)
		{
			// Slow down quicker if no movement direction input
			rb.velocity = new Vector3(rb.velocity.x * movementDeceleration, rb.velocity.y, rb.velocity.z * movementDeceleration);
		}
		else
		{
			// Apply motion velocity as the player is moving
			Vector3 targetVelocity = movementDirection.normalized * maxSpeed;
			Vector3 newForceDirection = (targetVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z));
			rb.velocity += newForceDirection * Time.fixedDeltaTime;
		}

		// Jump behaviours


		// Update coyote time counter
		if (IsGrounded()) coyoteTimeCounter = coyoteTime;
		else coyoteTimeCounter -= Time.fixedDeltaTime;

		// Update jump buffer counter if jump button was pressed recently
		if (isJumping) jumpbufferCounter = jumpBufferTime;
		else jumpbufferCounter -= Time.fixedDeltaTime;

		// Apply jump if coyote time and jump buffer is valid
		if (coyoteTimeCounter > 0 && jumpbufferCounter > 0)
		{
			PerformJump();
			jumpbufferCounter = 0;
		}

		// Applies increased gravity while falling for faster descent and snappier jump feel
		if (rb.velocity.y < fallThresholdSpeed)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
		}

		// Reduces upward velocity when jump button is released early, creating a shorter jump
		else if (rb.velocity.y > 0 && !IsJumping)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
		}

		// Applies normal gravity when jump button is held, allowing for a higher jump
		else if (rb.velocity.y > 0 && IsJumping)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
		}
	}
}
