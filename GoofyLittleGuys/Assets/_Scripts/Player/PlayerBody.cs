
using Managers;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerBody : MonoBehaviour
{
	[SerializeField] private LayerMask groundLayer;                     // What is considered the ground layer?
	[SerializeField] private List<LilGuyBase> lilGuyTeam;               // The lil guys in the player's team.
	[SerializeField] private List<LilGuySlot> lilGuyTeamSlots;          // The physical positions on the player prefab that the lil guys are children of.
	[SerializeField] private GameObject playerMesh;                     // Reference to the player's mesh gameobject
	[SerializeField] private PlayerInput playerInput;                   // This player's input component.
	[SerializeField] private GameObject lastHitPromptUI;                // The last hit prompt UI.
	[SerializeField] private GameObject teamFullMenu;					// The menu shown if the player captured a lil guy but their team is full.

	[SerializeField, Range(1, 25f)] private float maxSpeed = 25f;       // To be replaced with lilGuys[0].speed
	[SerializeField] private float movementDeceleration = 0.9f;

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

	private bool isDashing = false;                         // When the dash action is pressed for speed lil guy. Note this is in here because if the player swaps mid dash, they will get stuck in dash UNLESS this bool is here and is adjusted here.
	private bool flip = false;
	private bool inMinigame = false;
	private bool hasInteracted = false;
	private bool hasSwappedRecently = false; // If the player is in swap cooldown (feel free to delete cmnt)
	private bool hasImmunity = false; // If the player is in swap I-frames (feel free to delete cmnt)


	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;

	public float MaxSpeed { get { return maxSpeed; } }
	public bool HasInteracted { get { return hasInteracted; } set { hasInteracted = value; } }
	public bool HasSwappedRecently { get { return hasSwappedRecently; } set { hasSwappedRecently = value; } }
	public bool HasImmunity { get { return hasImmunity; } set { hasImmunity = value; } }
	public bool IsJumping { get { return isJumping; } set { isJumping = value; } }
	public bool IsDashing { get { return isDashing; } set { isDashing = value; } }
	public bool InMinigame { get { return inMinigame; } set { inMinigame = value; } }
	public Vector3 MovementDirection { get { return movementDirection; } }
	public List<LilGuyBase> LilGuyTeam { get { return lilGuyTeam; } }
	public List<LilGuySlot> LilGuyTeamSlots { get { return lilGuyTeamSlots; } }
	public GameObject TeamFullMenu { get { return teamFullMenu; } set { teamFullMenu = value; } }
	public bool Flip { get { return flip; } set { flip = value; } }


	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		EventManager.Instance.GameStarted += Init;
	}

	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= Init;
	}

	private void FixedUpdate()
	{
		// Flip player if they're moving in a different direction than what they're currently facing.
		if (flip) playerMesh.transform.rotation = Quaternion.Euler(0, 180, 0);
		else playerMesh.transform.rotation = Quaternion.Euler(0, 0, 0);

		// If the first lil guy is defeated, move to end of list automatically
		if (lilGuyTeam[0].health <= 0)
		{
			// Hide them from player, as to not confuse them with a living one... maybe find a better way to convey this
			lilGuyTeam[0].gameObject.SetActive(false);
			lilGuyTeam[0].GetComponentInChildren<SpriteRenderer>().color = Color.white;


			if (CheckTeamHealth())
			{
				// If this returns true, then there's at least one living lil guy on this player's team, so swap until they're at the front.

				// Grab the first lil guy, save them.
				LilGuyBase currentSelected = lilGuyTeam[0];
				Transform[] initialParents = new Transform[lilGuyTeam.Count];

				for (int i = 0; i < lilGuyTeam.Count; i++)
				{
					// Store initial parents for all items
					initialParents[i] = lilGuyTeamSlots[i].transform;
				}

				for (int i = 0; i < lilGuyTeam.Count - 1; i++)
				{
					// Send everyone else to the left one (lilGuy[1] -> lilGuy[0], lilGuy[2] -> lilGuy[1]);
					lilGuyTeam[i] = lilGuyTeam[i + 1];
					lilGuyTeam[i].transform.SetParent(initialParents[i]);
					lilGuyTeam[i].transform.localPosition = Vector3.zero;

					// Call CheckIfLiving on the slots, to update lock state.
					lilGuyTeamSlots[i].CheckIfLiving();
				}

				// Move the first element to the last position
				lilGuyTeam[lilGuyTeam.Count - 1] = currentSelected;
				currentSelected.transform.SetParent(initialParents[lilGuyTeam.Count - 1]);
				currentSelected.transform.localPosition = Vector3.zero;

				// Call CheckIfLiving on the slots, to update lock state.
				lilGuyTeamSlots[lilGuyTeam.Count - 1].CheckIfLiving();
			}
			else
			{
				// No living lil guys, time for a respawn.
				Respawn();
			}
		}

		// Movement behaviours
		if (!isDashing)
		{
			// If the player is not dashing, then they will have regular movement mechanics

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

	/// <summary>
	/// Method that updates the movement vector based on given input direction.
	/// </summary>
	/// <param name="dir">The input vector.</param>
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
		// Only one lil guy, so you can't swap.
		if (lilGuyTeam.Count <= 1) return;

		List<LilGuyBase> aliveTeamMembers = lilGuyTeam.Where(guy => guy.health > 0).ToList();       // Filter out the dead team members from the live ones.
		List<LilGuySlot> aliveTeamSlots = lilGuyTeamSlots.Where(slot => !slot.LockState).ToList();  // We only care about the lil guy slots of the lil guys that are still alive.

		// Only one lil guy alive, so swapping makes no sense here.
		if (aliveTeamMembers.Count <= 1) return;

		if (shiftDirection < 0)
		{
			// Store the first lil guy and we'll swap them to the end of the living member list.
			LilGuyBase currentSelected = lilGuyTeam[0];
			Transform[] initialParents = new Transform[aliveTeamMembers.Count];	

			
			for (int i = 0; i < aliveTeamMembers.Count; i++)
			{
				// Store initial parents for all items
				initialParents[i] = aliveTeamSlots[i].transform;
			}

			for (int i = 0; i < aliveTeamMembers.Count - 1; i++)
			{
				// Shift all living lil guys to the left
				// Also swap parents with the parent on their left.
				lilGuyTeam[i] = lilGuyTeam[i + 1];
				lilGuyTeam[i].transform.SetParent(initialParents[i]);
				lilGuyTeam[i].transform.localPosition = Vector3.zero;
			}

			// Move the first element to the last position
			lilGuyTeam[aliveTeamMembers.Count - 1] = currentSelected;
			currentSelected.transform.SetParent(initialParents[aliveTeamMembers.Count - 1]);
			currentSelected.transform.localPosition = Vector3.zero;
		}
		else if (shiftDirection > 0)
		{
			// Store the last element to rotate it to the beginning of the living member list.
			LilGuyBase lastInTeam = lilGuyTeam[aliveTeamMembers.Count - 1];
			Transform[] initialParents = new Transform[aliveTeamMembers.Count];

			for (int i = 0; i < aliveTeamMembers.Count; i++)
			{
				// Store initial parents for all items
				initialParents[i] = aliveTeamSlots[i].transform;
			}

			for (int i = aliveTeamMembers.Count - 1; i > 0; i--)
			{
				// Shift all living lil guys to the right
				// Also swap parents with the parent on their right.
				lilGuyTeam[i] = lilGuyTeam[i - 1];
				lilGuyTeam[i].transform.SetParent(initialParents[i]);
				lilGuyTeam[i].transform.localPosition = Vector3.zero;
			}

			// Move the last element to the first position
			lilGuyTeam[0] = lastInTeam;
			lastInTeam.transform.SetParent(initialParents[0]);
			lastInTeam.transform.localPosition = Vector3.zero;
		}
	}

	public void StartDash()
	{
		isDashing = true;
	}

	public void StopDash()
	{
		isDashing = false;
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
			Managers.AudioManager.Instance.PlaySfx("Jump", gameObject.GetComponent<AudioSource>());
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

	/// <summary>
	/// Returns true if there is some object marked as ground beneath the player's feet.
	/// </summary>
	/// <returns>True if there's ground beneath the player's feet, otherwise false.</returns>
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

	/// <summary>
	/// Shows the last hit prompt to the player, as they defeated a lil guy!
	/// </summary>
	/// <param name="lilGuy">The lil guy they defeated, and have a chance to capture.</param>
	public void ShowLastHitPrompt(LilGuyBase lilGuy)
	{
		lastHitPromptUI.GetComponent<LastHitMenu>().Initialize(lilGuy);
		lastHitPromptUI?.SetActive(true); // Activate the UI element if not null
		inMinigame = true;

		EnableUIControl();
	}

	/// <summary>
	/// Enable UI control for the player
	/// </summary>
	public void EnableUIControl()
	{
		playerInput.SwitchCurrentActionMap("UI"); // Switch to UI input map for menu control
	}

	/// <summary>
	/// Called when the player exits the UI
	/// </summary>
	public void DisableUIControl()
	{
		playerInput.SwitchCurrentActionMap("World");	// Switch back to gameplay controls
		lastHitPromptUI?.SetActive(false);				// Deactivate the UI element
	}

	/// <summary>
	/// Checks if there's any living lil guy on the player's team.
	/// </summary>
	/// <returns>True if there's a living guy on the player's team, otherwise false.</returns>
	private bool CheckTeamHealth()
	{
		return lilGuyTeam.Any(guy => guy.health > 0);
	}

	/// <summary>
	/// Method that handles how players respawn, and heals their entire team to full.
	/// </summary>
	private void Respawn()
	{
		transform.position = GameManager.Instance.FountainSpawnPoint.position;
		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			lilGuyTeam[i].health = lilGuyTeam[i].maxHealth;
			lilGuyTeam[i].gameObject.SetActive(true);
		}
	}
}
