
using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerBody : MonoBehaviour
{
	[SerializeField] private LilGuyBase activeLilGuy;
	[SerializeField] private List<LilGuyBase> lilGuyTeam;               // The lil guys in the player's team.
	[SerializeField] private List<LilGuySlot> lilGuyTeamSlots;          // The physical positions on the player prefab that the lil guys are children of.
	[SerializeField] private GameObject playerMesh;                     // Reference to the player's mesh gameobject
	[SerializeField] private PlayerInput playerInput;                   // This player's input component.
	[SerializeField] private GameObject lastHitPromptUI;                // The last hit prompt UI.
	[SerializeField] private GameObject teamFullMenu;                   // The menu shown if the player captured a lil guy but their team is full.

	[SerializeField, Range(1, 25f)] private float maxSpeed = 25f;       // To be replaced with lilGuys[0].speed
	[SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed
	[SerializeField] private float decelerationTime = 0.2f;  // Time to stop
	[SerializeField] private float smoothFactor = 0.8f;      // Factor for smooth transitions
	[SerializeField] private int maxBerryCount = 3;

	[SerializeField] private float fallMultiplier = 4f;

	private int berryCount = 0;
	private bool isDashing = false;                         // When the dash action is pressed for speed lil guy. Note this is in here because if the player swaps mid dash, they will get stuck in dash UNLESS this bool is here and is adjusted here.
	private bool flip = false;
	private bool inMinigame = false;
	private bool hasInteracted = false;
	private bool hasSwappedRecently = false; // If the player is in swap cooldown (feel free to delete cmnt)
	private bool hasImmunity = false; // If the player is in swap I-frames (feel free to delete cmnt)
	private bool canMove = true; // Whether or not the player can move, set it to false when you want to halt movement
	private Vector3 currentVelocity; // Internal tracking for velocity smoothing


	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;

	public float MaxSpeed { get { return maxSpeed; } }
	public bool HasInteracted { get { return hasInteracted; } set { hasInteracted = value; } }
	public bool HasSwappedRecently { get { return hasSwappedRecently; } set { hasSwappedRecently = value; } }
	public bool HasImmunity { get { return hasImmunity; } set { hasImmunity = value; } }
	public bool IsDashing { get { return isDashing; } set { isDashing = value; } }
	public bool InMinigame { get { return inMinigame; } set { inMinigame = value; } }
	public Vector3 MovementDirection { get { return movementDirection; } }
	public LilGuyBase ActiveLilGuy { get { return  activeLilGuy; } set {  activeLilGuy = value; } }
	public List<LilGuyBase> LilGuyTeam { get { return lilGuyTeam; } }
	public List<LilGuySlot> LilGuyTeamSlots { get { return lilGuyTeamSlots; } }
	public GameObject TeamFullMenu { get { return teamFullMenu; } set { teamFullMenu = value; } }
	public bool Flip { get { return flip; } set { flip = value; } }
	public int BerryCount { get { return berryCount; } set {  berryCount = value; } }
	public int MaxBerryCount => maxBerryCount;


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
				foreach (var lilGuy in lilGuyTeam)
				{
					lilGuy.GetComponent<Rigidbody>().isKinematic = false;
				}

				// Grab the first lil guy, save them.
				LilGuyBase firstInTeam = lilGuyTeam[0];
				Transform firstParent = lilGuyTeamSlots[0].transform;

				// Shift each element up the list
				for (int i = 0; i < lilGuyTeam.Count - 1; i++)
				{
					lilGuyTeam[i] = lilGuyTeam[i + 1];
					lilGuyTeam[i].SetFollowGoal(lilGuyTeamSlots[i].transform); // Update follow goal
				}

				// Move the first lil guy to the end of the list
				lilGuyTeam[lilGuyTeam.Count - 1] = firstInTeam;
				lilGuyTeam[lilGuyTeam.Count - 1].SetFollowGoal(lilGuyTeamSlots[lilGuyTeam.Count - 1].transform); // Update follow goal

				// Call CheckIfLiving on the slots, to update lock state.
				lilGuyTeamSlots[lilGuyTeam.Count - 1].CheckIfLiving(lilGuyTeam[lilGuyTeam.Count - 1]);


				lilGuyTeam[0].SetFollowGoal(lilGuyTeamSlots[0].transform); // Update follow goal
				lilGuyTeam[0].GetComponent<Rigidbody>().isKinematic = true;
				lilGuyTeam[0].transform.localPosition = Vector3.zero;
				activeLilGuy = lilGuyTeam[0];
			}
			else
			{
				// No living lil guys, time for a respawn.
				StartCoroutine(DelayedRespawn());
				//Respawn();
			}
		}

		// Movement behaviours
		if (!isDashing && canMove)
		{
			// If the player is not dashing, then they will have regular movement mechanics

			if (movementDirection.magnitude < 0.1f)
			{
				// Smooth deceleration when no input
				currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.fixedDeltaTime / decelerationTime);
			}
			else
			{
				// Calculate the target velocity based on input direction
				Vector3 targetVelocity = movementDirection.normalized * maxSpeed;

				// Smoothly accelerate towards the target velocity
				currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime / accelerationTime);
			}

			// Apply the smoothed velocity to the Rigidbody
			rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
		}

		if (rb.velocity.y < 0)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
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

		lilGuyTeam[0].IsMoving = Mathf.Abs(movementDirection.magnitude) > 0;
		lilGuyTeam[0].Flip = flip;
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

		foreach (var lilGuy in lilGuyTeam)
		{
			lilGuy.GetComponent<Rigidbody>().isKinematic = false;
		}

		List<LilGuyBase> aliveTeamMembers = lilGuyTeam.Where(guy => guy.health > 0).ToList();       // Filter out the dead team members from the live ones.
		List<LilGuySlot> aliveTeamSlots = lilGuyTeamSlots.Where(slot => !slot.LockState).ToList();  // We only care about the lil guy slots of the lil guys that are still alive.

		// Only one lil guy alive, so swapping makes no sense here.
		if (aliveTeamMembers.Count <= 1) return;

		if (shiftDirection < 0)
		{
			LilGuyBase firstInTeam = lilGuyTeam[0];
			Transform firstParent = aliveTeamSlots[0].transform;

			// Shift each element up the list
			for (int i = 0; i < aliveTeamMembers.Count - 1; i++)
			{
				lilGuyTeam[i] = lilGuyTeam[i + 1];
				lilGuyTeam[i].SetFollowGoal(aliveTeamSlots[i].transform); // Update follow goal
			}

			// Move the first lil guy to the end of the list
			lilGuyTeam[aliveTeamMembers.Count - 1] = firstInTeam;
			lilGuyTeam[aliveTeamMembers.Count - 1].SetFollowGoal(aliveTeamSlots[aliveTeamMembers.Count - 1].transform); // Update follow goal
		}
		else if (shiftDirection > 0)
		{
			// Store the last element to rotate it to the beginning of the living member list.
			LilGuyBase lastInTeam = lilGuyTeam[aliveTeamMembers.Count - 1];
			Transform lastParent = aliveTeamSlots[aliveTeamMembers.Count - 1].transform;

			// Shift each element down the list
			for (int i = aliveTeamMembers.Count - 1; i > 0; i--)
			{
				lilGuyTeam[i] = lilGuyTeam[i - 1];
				lilGuyTeam[i].SetFollowGoal(aliveTeamSlots[i].transform); // Update follow goal
			}

			// Move the last lil guy to the front of the list
			lilGuyTeam[0] = lastInTeam;
			lilGuyTeam[0].SetFollowGoal(aliveTeamSlots[0].transform); // Update follow goal
		}

		lilGuyTeam[0].GetComponent<Rigidbody>().isKinematic = true;
		lilGuyTeam[0].transform.localPosition = Vector3.zero;
		activeLilGuy = lilGuyTeam[0];
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
		playerInput.SwitchCurrentActionMap("World");    // Switch back to gameplay controls
		lastHitPromptUI?.SetActive(false);              // Deactivate the UI element
	}

	/// <summary>
	/// Checks if there's any living lil guy on the player's team.
	/// </summary>
	/// <returns>True if there's a living guy on the player's team, otherwise false.</returns>
	public bool CheckTeamHealth()
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
		canMove = true;
	}

	/// <summary>
	/// This coroutine simply waits respawnTimer, then respawns the given player
	/// NOTE: always give this the PlayerBody component of the player in question
	/// </summary>
	/// <param name="thePlayer"></param>
	/// <returns></returns>
	private IEnumerator DelayedRespawn()
	{
		canMove = false;
		yield return new WaitForSeconds(Managers.GameManager.Instance.RespawnTimer);
		Respawn();
	}
}
