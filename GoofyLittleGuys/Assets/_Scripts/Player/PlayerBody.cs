
using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerBody : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private LilGuyBase activeLilGuy;
	[SerializeField] private List<LilGuyBase> lilGuyTeam = new List<LilGuyBase>();               // The lil guys in the player's team.
	[SerializeField] private List<LilGuySlot> lilGuyTeamSlots;          // The physical positions on the player prefab that the lil guys are children of.
	[SerializeField] private GameObject playerMesh;                     // Reference to the player's mesh gameobject
	[SerializeField] private PlayerInput playerInput;                   // This player's input component.
	[SerializeField] private PlayerUi playerUi;                         // This player's input component.
	[SerializeField] private GameObject teamFullMenu;                   // The menu shown if the player captured a lil guy but their team is full.
	[SerializeField] private PlayerController controller;
	[SerializeField] private GameObject invincibilityFX;
	[SerializeField] private GameObject stormHurtFX;
	[SerializeField] private GameObject directionIndicator;

	[Header("Movement Parameters")]
	[SerializeField] private float maxSpeed = 25f;           // This turns into the speed of the active lil guy's. Used for the AI follow behaviours so they all keep the same speed in following the player.
	[SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed
	[SerializeField] private float decelerationTime = 0.2f;  // Time to stop
	[SerializeField] private float fallMultiplier = 4f;

	private float teamSpeedBoost = 0f;                       // If a lil guy gives a team boost to speed, this variable will store that speed boost.
	private float teamDamageReduction = 0f;                       // If a lil guy gives a team boost to speed, this variable will store that speed boost.

	[Header("Berry Inventory Parameters")]
	[SerializeField] private int maxBerryCount = 3;
	[SerializeField, Range(0f, 1f)] private float berryHealPercentage = 0.33f;

	[Header("Cooldown Parameters")]
	[SerializeField] private float berryUsageCooldown = 1f;
	[SerializeField] private float interactCooldown = 0.2f;
	[SerializeField] private float swapCooldown = 0.25f;
	[SerializeField] private float respawnInvincibility = 3f;
	[SerializeField] private float swapInvincibility = 0.05f;

	private float nextBerryUseTime = -Mathf.Infinity;
	private float nextInteractTime = -Mathf.Infinity;
	private float nextSwapTime = -Mathf.Infinity;
	private float deathTime = -Mathf.Infinity; 
	private float smoothFactor = 10;

	private Color playerColour = Color.white;

	private InteractableBase closestnteractable = null;
	private int berryCount = 0;
	private bool isDead = false;
	private bool inStorm = false;               // used by storm objects to determine if the player is currently in the storm
	private bool stormDmg = false;              // used by storm objects to determine if the player should take storm damage this frame.
	private bool stormCoroutine = false;        // used by storm objects to determine if this player has already started a damage coroutine.
	private bool isDashing = false;             // When the dash action is pressed for speed lil guy. Note this is in here because if the player swaps mid dash, they will get stuck in dash UNLESS this bool is here and is adjusted here.
	private bool flip = false;
	private bool hasInteracted = false;
	private bool hasSwappedRecently = false;    // If the player is in swap cooldown (feel free to delete cmnt)
	private bool hasImmunity = false;           // If the player is in swap I-frames (feel free to delete cmnt)
	private bool canMove = true;                // Whether or not the player can move, set it to false when you want to halt movement
	private bool knockedBack = false;
	private bool wasDefeated = false;           // Only true if this player has been defeated in phase 2
	private Vector3 currentVelocity;            // Internal tracking for velocity smoothing
	private bool inMenu = true;

	private bool isSwapping = false;            // NECESSARY FOR AUTOSWAP/PLAYER SWAP. We need a mutex-type mechanism so that that the resources in the lil guy list doesn't get mismanaged from the two swapping mechanisms accessing it at the same time.
	private Coroutine respawnCoroutine = null;
	private Coroutine invincibilityCoroutine = null;

	private Vector3 movementDirection = Vector3.zero;
	private Rigidbody rb;
	private LilGuyBase closestWildLilGuy = null;

	public GameObject MinimapIcon;

	public Vector3 CurrentVelocity => currentVelocity;
	public Color PlayerColour { get { return playerColour; } 
		set
		{ 
			playerColour = value;
			DecalProjector projector = directionIndicator.GetComponentInChildren<DecalProjector>();
			Material directionIndicatorMat = new Material(projector.material);
			directionIndicatorMat.SetColor("_BaseColor", playerColour);
			projector.material = directionIndicatorMat;
			MinimapIcon.GetComponent<Renderer>().material.color = playerColour;
		}
	} 
	public LilGuyBase ClosestWildLilGuy { get { return closestWildLilGuy; } set { closestWildLilGuy = value; } }
	public bool HasInteracted { get { return hasInteracted; } set { hasInteracted = value; } }
	public bool HasSwappedRecently { get { return hasSwappedRecently; } set { hasSwappedRecently = value; } }
	public bool HasImmunity { get { return hasImmunity; } set { hasImmunity = value; } }
	public bool InStorm { get { return inStorm; } set { inStorm = value; } }
	public bool StormDmg { get { return stormDmg; } set { stormDmg = value; } }
	public bool StormCoroutine { get { return stormCoroutine; } set { stormCoroutine = value; } }
	public bool IsDashing { get { return isDashing; } set { isDashing = value; } }
	public bool KnockedBack { get { return knockedBack; } set { knockedBack = value; } }
	public LilGuyBase ActiveLilGuy { get { return activeLilGuy; } set { activeLilGuy = value; } }
	public GameObject TeamFullMenu { get { return teamFullMenu; } set { teamFullMenu = value; } }
	public bool Flip { get { return flip; } set { flip = value; } }
	public bool IsDead => isDead;
	public int BerryCount { get { return berryCount; } set { berryCount = value; } }
	public bool InMenu { get { return inMenu; } set { inMenu = value; } }

	public InteractableBase ClosestInteractable { get { return closestnteractable; } set { closestnteractable = value; } }

	public List<LilGuyBase> LilGuyTeam => lilGuyTeam;
	public List<LilGuySlot> LilGuyTeamSlots => lilGuyTeamSlots;
	public Vector3 MovementDirection => movementDirection;
	public int MaxBerryCount => maxBerryCount;
	public float MaxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }
	public float TeamSpeedBoost { get { return teamSpeedBoost; } set { teamSpeedBoost = value; } }
	public float TeamDamageReduction { get { return teamDamageReduction; } set { teamDamageReduction = value; } }
	public PlayerUi PlayerUI => playerUi;
	public PlayerController Controller => controller;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		EventManager.Instance.GameStarted += Init;
	}

	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= Init;
	}
	private void Update()
	{
		hasInteracted = false;
		if (GameManager.Instance.IsPaused)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
			lilGuyTeam[0].IsMoving = false;
		}
		if (lilGuyTeam.Count <= 0) return; 
		RaycastHit hit;
		if (Physics.Raycast(directionIndicator.transform.position + Vector3.up, Vector3.down*10, out hit))
		{
			Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // Calculate the rotation to match the surface normal   
			
			// Calculate the angle using Atan2 (for XZ plane rotation)
			Quaternion finalRotation = surfaceRotation * lilGuyTeam[0].AttackOrbit.rotation;          // Combine with the pivot's rotation

			directionIndicator.transform.rotation = Quaternion.Slerp(directionIndicator.transform.rotation, finalRotation, smoothFactor); ;
		}
		invincibilityFX.SetActive(hasImmunity);
		invincibilityFX.transform.rotation = lilGuyTeam[0].Mesh.transform.rotation;
		stormHurtFX.SetActive(inStorm);
		stormHurtFX.transform.rotation = lilGuyTeam[0].Mesh.transform.rotation;
	}

	private void Awake()
	{
		//lilGuyTeam = new List<LilGuyBase>();
	}
	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
			if (lilGuyTeam.Count <= 0) return;
			lilGuyTeam[0].IsMoving = false;
		}
	}
	private void FixedUpdate()
	{
		if (inMenu) { return; }

		// Flip player if they're moving in a different direction than what they're currently facing.
		if (flip) playerMesh.transform.rotation = new Quaternion(0, 1, 0, 0);
		else playerMesh.transform.rotation = new Quaternion(0, 0, 0, 1);

		if (lilGuyTeam.Count > 0 && lilGuyTeam != null && lilGuyTeam[0] != null) maxSpeed = (lilGuyTeam[0].BaseSpeed + (lilGuyTeam[0].Speed * 0.3f) + teamSpeedBoost);

		if (!IsGrounded())
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (maxSpeed - 1) * Time.fixedDeltaTime;
		}


		// If the first lil guy is defeated, move to end of list automatically
		if (lilGuyTeam[0].Health <= 0)
		{
			if (isSwapping) return;
			isSwapping = true;

			activeLilGuy.CancelSpecial();
			// Hide them from player, as to not confuse them with a living one... maybe find a better way to convey this
			if (!lilGuyTeam[0].IsDying) lilGuyTeam[0].PlayDeathAnim();
			lilGuyTeam[0].SetLayer(LayerMask.NameToLayer("Player"));
			lilGuyTeam[0].GetComponentInChildren<SpriteRenderer>().color = Color.white;


			if (CheckTeamHealth())
			{
				// If this returns true, then there's at least one living lil guy on this player's team, so swap until they're at the front.
				foreach (LilGuyBase lilGuy in lilGuyTeam)
				{
					lilGuy.RB.isKinematic = false;
					lilGuy.RB.interpolation = RigidbodyInterpolation.Interpolate;
				}
				ReorganizeTeam();
			}
			else
			{
				isDead = true;
				// No living lil guys, time for a respawn if possible.
				if (deathTime < GameManager.Instance.PhaseOneDurationSeconds())
				{
					respawnCoroutine ??= StartCoroutine(DelayedRespawn());
				}
				else if (GameManager.Instance.CurrentPhase == 2 && !wasDefeated)
				{
					GameManager.Instance.PlayerDefeat(this);
					wasDefeated = true;
				}
				isSwapping = false;
			}

		}
		// Movement behaviours
		if (!isDashing && canMove && !lilGuyTeam[0].LockMovement && !knockedBack)
		{
			Vector3 velocity = rb.velocity;
			velocity.y = 0;
			Vector3 targetVelocity;
			// If the player is not dashing, then they will have regular movement mechanics

			if (movementDirection.magnitude < 0.1f)
			{
				targetVelocity = Vector3.zero;
				// Smooth deceleration when no input
				currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.fixedDeltaTime / decelerationTime);
			}
			else
			{
				// Calculate the target velocity based on input direction
				 targetVelocity = movementDirection.normalized * (lilGuyTeam[0].BaseSpeed + (lilGuyTeam[0].Speed * 0.3f) + teamSpeedBoost);
				// Smoothly accelerate towards the target velocity
				currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime / accelerationTime);
			}

			// Apply the smoothed velocity to the Rigidbody
			rb.velocity = new Vector3(currentVelocity.x, GameManager.Instance.CurrentPhase == 2 && IsDead ? currentVelocity.y : rb.velocity.y, currentVelocity.z);

		}
		if (lilGuyTeam[0].LockMovement) rb.velocity = new Vector3(0, rb.velocity.y, 0);
	}
	

	private bool IsGrounded()
	{
		return Physics.Raycast(rb.position + Vector3.up, Vector3.down, 3f, LayerMask.GetMask("Ground"));
	}

	/// <summary>
	/// Method that updates the movement vector based on given input direction.
	/// </summary>
	/// <param name="dir">The input vector.</param>
	public void UpdateMovementVector(Vector2 dir)
	{
		movementDirection = new Vector3(dir.x, movementDirection.y, dir.y);
		if (dir.x > 0) flip = true;
		if (dir.x < 0) flip = false;

		lilGuyTeam[0].IsMoving = Mathf.Abs(movementDirection.magnitude) > 0;
		lilGuyTeam[0].MovementDirection = movementDirection;
	}

	public void UpdateUpDown(float dir)
	{
		movementDirection = new Vector3(movementDirection.x, dir, movementDirection.z);
	}

	public void Interact()
	{
		if (Time.time <= nextInteractTime) return;
		if (closestnteractable != null)
		{
			closestnteractable.OnInteracted(this);
		}
		nextInteractTime = Time.time + interactCooldown;
	}

	public void UseBerry()
	{
		if (berryCount <= 0) return;
		if (IsDead) return;
		if (closestWildLilGuy != null)
		{
			berryCount--;

			SpawnManager.Instance.RemoveLilGuyFromSpawns();
			closestWildLilGuy.PlayerOwner = this;
			closestWildLilGuy.Init(LayerMask.NameToLayer("Player"));
			closestWildLilGuy.Health = closestWildLilGuy.MaxHealth;
			closestWildLilGuy.GetComponent<AiController>().SetState(AiController.AIState.Tamed);
			closestWildLilGuy.LeaveDeathAnim();
			if (LilGuyTeam.Count < 3)
			{

				// There is room on the player's team for this lil guy.
				// Set player owner to this player, and reset the lil guy's health to full, before adding to the player's party.
				LilGuyTeam.Add(closestWildLilGuy);

				// Setting layer to Player Lil Guys, and putting the lil guy into the first empty slot available.
				closestWildLilGuy.transform.SetParent(transform, false);
				closestWildLilGuy.SetFollowGoal(LilGuyTeamSlots[LilGuyTeam.Count - 1].transform);
				closestWildLilGuy.GetComponent<Rigidbody>().isKinematic = false;
				closestWildLilGuy.transform.localPosition = Vector3.zero;
				closestWildLilGuy.transform.localRotation = Quaternion.identity;
			}
			else
			{
				//Handle choosing which lil guy on the player's team will be replaced with this lil guy
				TeamFullMenu.SetActive(true);
				TeamFullMenu.GetComponent<TeamFullMenu>().Init(closestWildLilGuy);
			}
			GameObject catchEffect = Instantiate(FXManager.Instance.GetEffect("Catch"), transform.position, Quaternion.identity);
			catchEffect.GetComponent<SpriteRenderer>().sortingOrder = (int)-transform.position.z - 1;
		}
		else if (lilGuyTeam[0].Health < lilGuyTeam[0].MaxHealth && Time.time > nextBerryUseTime)
		{
			int healthRestored = Mathf.CeilToInt(lilGuyTeam[0].MaxHealth * berryHealPercentage);
			EventManager.Instance.HealLilGuy(lilGuyTeam[0], healthRestored);

			berryCount--;
			nextBerryUseTime = Time.time + berryUsageCooldown;
			EventManager.Instance.UpdatePlayerHealthUI(this);
		}
		EventManager.Instance.UpdatePlayerHealthUI(this);
		playerUi.SetBerryCount(berryCount);
	}

	public void ReorganizeTeam()
	{
		lilGuyTeam = lilGuyTeam
	.Where(guy => guy.Health > 0) // Filter alive lil guys
	.Concat(lilGuyTeam.Where(guy => guy.Health <= 0)) // Append dead lil guys
	.ToList();

		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			lilGuyTeam[i].SetFollowGoal(lilGuyTeamSlots[i].transform); // Update follow goal
			lilGuyTeamSlots[i].LilGuyInSlot = lilGuyTeam[i];
			lilGuyTeam[i].IsAttacking = false;
			lilGuyTeam[i].SetMaterial(GameManager.Instance.RegularLilGuySpriteMat);
		}

		lilGuyTeam[0].GetComponent<Rigidbody>().isKinematic = true;
		lilGuyTeam[0].SetLayer(LayerMask.NameToLayer("PlayerLilGuys"));
		lilGuyTeam[0].RB.interpolation = RigidbodyInterpolation.None;
		lilGuyTeam[0].transform.localPosition = Vector3.zero;
		activeLilGuy = lilGuyTeam[0];

		activeLilGuy.SetMaterial(GameManager.Instance.OutlinedLilGuySpriteMat);

		isSwapping = false;
		SetInvincible(swapInvincibility);
	}
	/// <summary>
	/// Swaps the Lil guy based on a queue. If input is right, then the next one in list moves to position 1 and if left, the previous lil guy moves to position 1
	/// and the rest cascade accordingly.
	/// </summary>
	/// <param name="shiftDirection">The input provided by the D-Pad. Negative means they pressed left, and positive means they pressed right.</param>
	public void SwapLilGuy(float shiftDirection)
	{
		// Only one lil guy, so you can't swap.
		if (isSwapping || lilGuyTeam.Count <= 1 || Time.time <= nextSwapTime) return;
		isSwapping = true;

		List<LilGuyBase> aliveTeamMembers = lilGuyTeam.Where(guy => guy.Health > 0).ToList();       // Filter out the dead team members from the live ones.
		List<LilGuySlot> aliveTeamSlots = lilGuyTeamSlots.Where(slot => !slot.LockState).ToList();  // We only care about the lil guy slots of the lil guys that are still alive.
		activeLilGuy.CancelSpecial();

		// Only one lil guy alive, so swapping makes no sense here.
		if (aliveTeamMembers.Count <= 1)
		{
			isSwapping = false;
			return;
		}

		foreach (LilGuyBase lilGuy in lilGuyTeam)
		{
			lilGuy.GetComponent<Rigidbody>().isKinematic = false;
			lilGuy.SetLayer(LayerMask.NameToLayer("Player"));
			lilGuy.IsAttacking = false;
			lilGuy.RB.interpolation = RigidbodyInterpolation.Interpolate;
			lilGuy.SetMaterial(GameManager.Instance.RegularLilGuySpriteMat);
		}

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
		lilGuyTeam[0].SetLayer(LayerMask.NameToLayer("PlayerLilGuys"));
		lilGuyTeam[0].transform.localPosition = Vector3.zero;
		lilGuyTeam[0].RB.interpolation = RigidbodyInterpolation.None;
		activeLilGuy = lilGuyTeam[0];
		activeLilGuy.SetMaterial(GameManager.Instance.OutlinedLilGuySpriteMat);
		EventManager.Instance.UpdatePlayerHealthUI(this);
		nextSwapTime = Time.time + swapCooldown;

		isSwapping = false;
		SetInvincible(swapInvincibility);
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
		DisableUIControl();
		rb.isKinematic = false;
		controller.PlayerCam.gameObject.SetActive(true);
		playerInput.camera.clearFlags = CameraClearFlags.Skybox;
		controller.PlayerEventSystem.firstSelectedGameObject = null;
		controller.PlayerEventSystem.gameObject.SetActive(false);
		playerMesh.SetActive(true);
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
	}

	/// <summary>
	/// Checks if there's any living lil guy on the player's team.
	/// </summary>
	/// <returns>True if there's a living guy on the player's team, otherwise false.</returns>
	public bool CheckTeamHealth()
	{
		return lilGuyTeam.Any(guy => guy.Health > 0);
	}

	/// <summary>
	/// Method that handles how players respawn, and heals their entire team to full.
	/// </summary>
	private void Respawn()
	{
		knockedBack = false;
		isDead = false;
		rb.MovePosition(GameManager.Instance.FountainSpawnPoint.position);
		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			lilGuyTeam[i].IsDying = false;
			lilGuyTeam[i].Health = lilGuyTeam[i].MaxHealth;
			lilGuyTeam[i].gameObject.SetActive(true);
		}
		canMove = true;
		lilGuyTeam[0].SetLayer(LayerMask.NameToLayer("PlayerLilGuys"));
		EventManager.Instance.UpdatePlayerHealthUI(this);
		SetInvincible(respawnInvincibility);
	}

	/// <summary>
	/// This coroutine simply waits respawnTimer, then respawns the given player
	/// </summary>
	/// <param name="thePlayer"></param>
	/// <returns></returns>
	private IEnumerator DelayedRespawn()
	{
		canMove = false;
		rb.velocity = Vector3.zero;
		deathTime = GameManager.Instance.CurrentGameTime;
		if (deathTime > GameManager.Instance.PhaseOneDurationSeconds())
		{
			rb.useGravity = false;
		}
		yield return new WaitForSeconds(Managers.GameManager.Instance.RespawnTimer);
		if (deathTime <= GameManager.Instance.PhaseOneDurationSeconds())
		{
			rb.useGravity = true;
			Respawn();
		}
		else
		{
			canMove = true;
			rb.useGravity = false;
		}
		respawnCoroutine = null;
	}

	/// <summary>
	/// This method deals damage to the currently active Lil Guy if the player is 
	/// currently meant to be taking storm damage.
	/// </summary>
	public void StormDamage(float dmg)
	{
		Hurtbox h = lilGuyTeam[0].gameObject.GetComponent<Hurtbox>();
		if (h != null)
		{
			inStorm = true;
			h.TakeDamage(dmg);
		}
	}


	/// <summary>
	/// Method that makes this lil guy invincible for x duration in seconds. 
	/// Duration less than or equal to -1: Sets invincibility to true indefinitely (until you turn it off elsewhere).
	/// Duration equal to 0: Sets invincibility to false.
	/// </summary>
	/// <param name="duration">Duration in seconds.</param>
	public void SetInvincible(float duration)
	{
		if (invincibilityCoroutine != null)
		{
			StopCoroutine(invincibilityCoroutine);
			invincibilityCoroutine = null;
			hasImmunity = false;
		}

		if (duration <= -1)
		{
			hasImmunity = true;
			return;
		}
		else if (duration == 0)
		{
			hasImmunity = false;
			return;
		}
		else invincibilityCoroutine = StartCoroutine(InvincibilityTimer(duration));


	}

	private IEnumerator InvincibilityTimer(float invincibilityDuration)
	{
		hasImmunity = true;
		yield return new WaitForSeconds(invincibilityDuration);
		hasImmunity = false;
	}
}
