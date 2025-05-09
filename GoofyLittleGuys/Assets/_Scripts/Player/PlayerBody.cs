using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerBody : MonoBehaviour
{
	#region Public Variables & Serialize Fields
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private PlayerInput playerInput;                   // This player's input component.
	[ColoredGroup][SerializeField] private PlayerController controller;
	[ColoredGroup][SerializeField] private StatMetrics gameplayStats;
	[ColoredGroup][SerializeField] private GameObject playerMesh;                     // Reference to the player's mesh gameobject

	[Header("UI")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject directionIndicator;
	[ColoredGroup][SerializeField] private GameObject leaderCrown;
	[ColoredGroup][SerializeField] private PlayerUi playerUi;                         // This player's input component.
	[ColoredGroup] public GameObject miniMapIcon;
	[ColoredGroup][SerializeField] private GameObject teamFullMenu;                   // The menu shown if the player captured a lil guy but their team is full.


	[Header("Effects")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject invincibilityFX;
	[ColoredGroup][SerializeField] private GameObject stormHurtFX;
	[ColoredGroup][SerializeField] private Volume playerVolume;
	[ColoredGroup][SerializeField] private VolumeProfile basePlayerProfile;

	[Header("Lil Guy Team")]
	[HorizontalRule]
	[SerializeField] private List<LilGuyBase> lilGuyTeam = new List<LilGuyBase>();               // The lil guys in the player's team.
	[SerializeField] private List<LilGuySlot> lilGuyTeamSlots;          // The physical positions on the player prefab that the lil guys are children of.
	[ColoredGroup][SerializeField] private LilGuyBase activeLilGuy;

	[Header("Movement Parameters")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float maxSpeed = 25f;           // This turns into the speed of the active lil guy's. Used for the AI follow behaviours so they all keep the same speed in following the player.
	[ColoredGroup][SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed

	[Header("Berry Parameters")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private int maxBerryCount = 3;
	[ColoredGroup][SerializeField, Range(0f, 1f)] private float berryHealPercentage = 0.33f;

	[Header("Combat Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField, DebugOnly] private float debugTeamSpeedBoost;
	[ColoredGroup][SerializeField, DebugOnly] private float debugTeamDamageReduction;
	[ColoredGroup][SerializeField] private float knockbackResistance = 1f;  // Reduce knockback effect
	[ColoredGroup][SerializeField] private float knockbackDecayRate = 5f;   // Speed at which knockback fades

	[Header("Cooldown Parameters")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float berryUsageCooldown = 1f;
	[ColoredGroup][SerializeField] private float interactCooldown = 0.2f;
	[ColoredGroup][SerializeField] private float swapCooldown = 0.25f;
	[ColoredGroup][SerializeField] private float respawnInvincibility = 3f;
	[ColoredGroup][SerializeField] private float swapInvincibility = 0.05f;
	#endregion

	#region Private Variables
	// PLAYER
	private Rigidbody rb;
	private Color playerColour = Color.white;
	private float nextBerryUseTime = 0;
	private float nextSwapTime = -Mathf.Infinity;
	private float deathTime = -Mathf.Infinity;
	private bool flip = false;
	private bool isSwapping = false;
	private bool hasSwappedRecently = false;    // If the player is in swap cooldown (feel free to delete cmnt)
	private bool hasImmunity = false;           // If the player is in swap I-frames (feel free to delete cmnt)
	private bool isDead = false;
	private bool wasDefeated = false;           // Only true if this player has been defeated in phase 2
	private bool isLeader;

	// TUTORIAL
	private GameObject starter = null;

	// MOVEMENT
	private Vector3 movementDirection = Vector3.zero;
	private Vector3 currentVelocity;            // Internal tracking for velocity smoothing
	private Vector3 lastPosition; // Previous position for tracking
	private float smoothFactor = 10;
	private bool canMove = true;                // Whether or not the player can move, set it to false when you want to halt movement
	private bool isDashing = false;             // When the dash action is pressed for speed lil guy. Note this is in here because if the player swaps mid dash, they will get stuck in dash UNLESS this bool is here and is adjusted here.

	// COMBAT
	private Coroutine hitstunCoroutine = null;  // for da hitstun
	private Vector3 knockbackForce = Vector3.zero;
	private float hitStunSlowMult = 1f;
	private bool knockedBack = false;

	// INTERACTABLE & WORLD
	private LilGuyBase closestWildLilGuy = null;
	private InteractableBase closestInteractable = null;
	private float nextInteractTime = -Mathf.Infinity;
	private int berryCount = 0;
	private bool hasInteracted = false;
	private bool isInteracting = false;

	// WORLD
	private Coroutine respawnCoroutine = null;
	private Coroutine invincibilityCoroutine = null;
	private bool inStorm = false;               // used by storm objects to determine if the player is currently in the storm


	// UI
	private bool inMenu = true;

	#endregion

	#region Getters & Setters
	public Color PlayerColour
	{
		get { return playerColour; }
		set
		{
			playerColour = value;
			DecalProjector projector = directionIndicator.GetComponentInChildren<DecalProjector>();
			Material directionIndicatorMat = new Material(projector.material);
			directionIndicatorMat.SetColor("_BaseColor", playerColour);
			projector.material = directionIndicatorMat;
			miniMapIcon.GetComponent<Renderer>().material.color = playerColour;
		}
	}
	public BuffHandler Buffs { get; private set; } = new BuffHandler();
	public float TeamSpeedBoost => Buffs.GetTotalValue(BuffType.TeamSpeedBoost);
	public float TeamDamageReduction => Buffs.GetTotalValue(BuffType.TeamDamageReduction);
	public Vector3 CurrentVelocity => currentVelocity;
	public StatMetrics GameplayStats => gameplayStats;
	public LilGuyBase ClosestWildLilGuy { get { return closestWildLilGuy; } set { closestWildLilGuy = value; } }
	public bool HasInteracted { get { return hasInteracted; } set { hasInteracted = value; } }
	public bool HasSwappedRecently { get { return hasSwappedRecently; } set { hasSwappedRecently = value; } }
	public bool HasImmunity { get { return hasImmunity; } set { hasImmunity = value; } }
	public bool InStorm { get { return inStorm; } set { inStorm = value; } }
	public GameObject StormHurtFx { get { return stormHurtFX; } set { stormHurtFX = value; } }
	public bool IsDashing { get { return isDashing; } set { isDashing = value; } }
	public bool IsInteracting { get { return isInteracting; } set { isInteracting = value; } }
	public bool KnockedBack { get { return knockedBack; } set { knockedBack = value; } }
	public LilGuyBase ActiveLilGuy { get { return activeLilGuy; } set { activeLilGuy = value; } }
	public GameObject TeamFullMenu { get { return teamFullMenu; } set { teamFullMenu = value; } }
	public GameObject Starter { get { return starter; } set { starter = value; } }
	public bool Flip { get { return flip; } set { flip = value; } }
	public bool IsDead => isDead;
	public int BerryCount { get { return berryCount; } set { berryCount = value; } }
	public bool InMenu { get { return inMenu; } set { inMenu = value; } }
	public float NextBerryUseTime => nextBerryUseTime;
	public float BerryCooldown => berryUsageCooldown;
	public InteractableBase ClosestInteractable { get { return closestInteractable; } set { closestInteractable = value; } }
	public List<LilGuyBase> LilGuyTeam => lilGuyTeam;
	public List<LilGuySlot> LilGuyTeamSlots => lilGuyTeamSlots;
	public Vector3 MovementDirection => movementDirection;
	public int MaxBerryCount => maxBerryCount;
	public float MaxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }
	public PlayerUi PlayerUI => playerUi;
	public PlayerController Controller => controller;
	public float DeathTime => deathTime;
	public bool IsLeader => isLeader;
	public Volume PlayerVolume => playerVolume;
	#endregion

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		EventManager.Instance.GameStarted += Init;
		lastPosition = transform.position; // Initialize position

		VolumeProfile newProfile = CloneProfileDeep(basePlayerProfile);
		playerVolume.profile = newProfile;

		var settings = SettingsManager.Instance.GetSettings();
		if (playerVolume.profile.TryGet(out ColorAdjustments colorAdjust))
		{
			colorAdjust.contrast.value = settings.contrast;
			colorAdjust.postExposure.value = settings.brightness;
		}
	}


	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= Init;
		Buffs.OnBuffExpired -= HandleBuffExpired;
	}
	private void Update()
	{
		Buffs.Update();
		debugTeamSpeedBoost = Buffs.GetTotalValue(BuffType.TeamSpeedBoost);
		debugTeamDamageReduction = Buffs.GetTotalValue(BuffType.TeamDamageReduction);
		if (nextBerryUseTime > 0) nextBerryUseTime -= Time.deltaTime;
		hasInteracted = false;
		if (GameManager.Instance.IsPaused)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
			activeLilGuy.IsMoving = false;
		}
		if (lilGuyTeam.Count <= 0) return;
		RaycastHit hit;
		if (Physics.Raycast(directionIndicator.transform.position + Vector3.up, Vector3.down * 10, out hit))
		{
			Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // Calculate the rotation to match the surface normal   

			// Calculate the angle using Atan2 (for XZ plane rotation)
			Quaternion finalRotation = surfaceRotation * lilGuyTeam[0].AttackOrbit.rotation;          // Combine with the pivot's rotation

			directionIndicator.transform.rotation = Quaternion.Slerp(directionIndicator.transform.rotation, finalRotation, smoothFactor);
		}
		invincibilityFX.SetActive(hasImmunity && !wasDefeated);
		invincibilityFX.transform.rotation = lilGuyTeam[0].Mesh.transform.rotation;
		stormHurtFX.SetActive(inStorm && !wasDefeated);
		stormHurtFX.transform.rotation = lilGuyTeam[0].Mesh.transform.rotation;

		if (isDead) return;
		Vector3 currentPosition = transform.position;
		float distanceThisFrame = Vector3.Distance(lastPosition, currentPosition);

		GameplayStats.DistanceTraveled += distanceThisFrame;
		lastPosition = currentPosition; // Update last position
	}

	private void Awake()
	{
		//lilGuyTeam = new List<LilGuyBase>();
		Buffs.OnBuffExpired += HandleBuffExpired;
	}
	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
			if (lilGuyTeam.Count <= 0) return;
			activeLilGuy.IsMoving = false;
		}
	}
	private void FixedUpdate()
	{
		if (inMenu) return;

		// Apply knockback decay
		if (knockbackForce.magnitude > 0.1f)
		{
			knockbackForce = Vector3.Lerp(knockbackForce, Vector3.zero, Time.fixedDeltaTime * knockbackDecayRate);
		}

		// Flip player if moving in a different direction than they are facing
		if (flip) playerMesh.transform.rotation = new Quaternion(0, 1, 0, 0);
		else playerMesh.transform.rotation = new Quaternion(0, 0, 0, 1);

		if (lilGuyTeam.Count > 0 && lilGuyTeam[0] != null)
			maxSpeed = (lilGuyTeam[0].MovementSpeed + TeamSpeedBoost);

		if (!wasDefeated && !IsGrounded())
		{
			Debug.Log("In AIR!");
			rb.velocity += Vector3.up * Physics.gravity.y * (maxSpeed - 1) * Time.fixedDeltaTime;
		}

		// Apply movement
		if (!isDashing && canMove && !lilGuyTeam[0].LockMovement)
		{
			Vector3 velocity = rb.velocity;
			velocity.y = 0;

			Vector3 targetVelocity = movementDirection.magnitude < 0.1f
				? Vector3.zero
				: movementDirection.normalized * (lilGuyTeam[0].MovementSpeed * lilGuyTeam[0].MoveSpeedModifier + TeamSpeedBoost);

			// Smoothly accelerate towards the target velocity
			currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime / accelerationTime);

			// Apply movement scaled by hitstun slow and add knockback force
			rb.velocity = new Vector3((currentVelocity.x / hitStunSlowMult) + knockbackForce.x, !wasDefeated ? rb.velocity.y : (currentVelocity.y / hitStunSlowMult) + knockbackForce.y, (currentVelocity.z / hitStunSlowMult) + knockbackForce.z);
		}

		if (lilGuyTeam[0].LockMovement)
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
	}
	public void SetLeader(bool state)
	{
		isLeader = state;
		if (leaderCrown != null)
			leaderCrown.SetActive(state);
	}

	private void HandleBuffExpired(BuffType type, object source)
	{
		if (type == BuffType.TeamSpeedBoost)
		{
			foreach (LilGuyBase lilGuy in LilGuyTeam)
			{
				if (lilGuy != null && lilGuy.isActiveAndEnabled)
				{
					lilGuy.RemoveSpeedBoost();
				}
			}
		}
	}

	// Method to apply knockback
	public void ApplyKnockback(Vector3 force)
	{
		knockbackForce = force / knockbackResistance; // Reduce knockback effect if resistance is higher
	}

	public void StartHitStun(float stunMult, float stunTime, AnimationCurve stunCurve)
	{
		if (!lilGuyTeam[0].CanStun) return;
		hitstunCoroutine = StartCoroutine(ApplyHitStun(stunMult, stunTime, stunCurve));
	}

	private IEnumerator ApplyHitStun(float stunMult, float stunTime, AnimationCurve stunCurve)
	{
		//set stun mult
		//lerp stun mult back to 1 over stuntime
		//change animator speed

		hitStunSlowMult = stunMult;
		var timer = 0.0f;
		while (timer < stunTime)
		{
			timer += Time.deltaTime;
			var t = Mathf.Clamp01(timer / stunTime);
			var curveVal = stunCurve.Evaluate(t);
			hitStunSlowMult = Mathf.Lerp(stunMult, 1.0f, curveVal);
			lilGuyTeam[0].Animator.speed = 0.0f;
			yield return null;
		}
		lilGuyTeam[0].Animator.speed = 1.0f;
		hitStunSlowMult = 1.0f;
	}

	private void StopHitStun()
	{
		hitStunSlowMult = 1.0f;
		lilGuyTeam[0].Animator.speed = 1.0f;
		if (hitstunCoroutine != null) StopCoroutine(hitstunCoroutine);
	}

	private bool IsGrounded()
	{
		return Physics.Raycast(rb.position + Vector3.up, Vector3.down, 1f, LayerMask.GetMask("Ground"));
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

		activeLilGuy.IsMoving = Mathf.Abs(movementDirection.magnitude) > 0;
		activeLilGuy.MovementDirection = movementDirection.magnitude > 0 ? movementDirection : Vector3.zero;
	}

	public void UpdateUpDown(float dir)
	{
		movementDirection = new Vector3(movementDirection.x, dir, movementDirection.z);
	}

	public void Interact()
	{
		if (closestWildLilGuy != null)
		{
			GameplayStats.LilGuysTamedTotal++;
			var wBehave = closestWildLilGuy.GetComponent<WildBehaviour>();
			if (wBehave.enabled)
				wBehave.HomeSpawner.RemoveLilGuyFromSpawns();
			closestWildLilGuy.PlayerOwner = this;
			closestWildLilGuy.ShouldRestoreKinematic = false;
			closestWildLilGuy.Init(LayerMask.NameToLayer("Player"));
			closestWildLilGuy.SetMaterial(GameManager.Instance.RegularLilGuySpriteMat);
			closestWildLilGuy.Health = closestWildLilGuy.MaxHealth;
			closestWildLilGuy.ResetTimers();
			closestWildLilGuy.GetComponent<AiController>().SetState(AiController.AIState.Tamed);
			closestWildLilGuy.GetComponent<TutorialAiController>()?.SetState(TutorialAiController.AIState.Tamed);
			closestWildLilGuy.GetComponent<Hurtbox>().LastHit = null;
			closestWildLilGuy.CalculateMoveSpeed();
			if (LilGuyTeam.Count < 3)
			{
				lilGuyTeam[0].PlaySound("Tamed_Lil_Guy");
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

		if (Time.time <= nextInteractTime) return;
		if (closestInteractable != null)
		{
			closestInteractable.StartInteraction(this); // Start the interaction process
		}
		nextInteractTime = Time.time + interactCooldown;
		EventManager.Instance.RefreshUi(this.playerUi, 0);
		SetTimer();
	}

	public void StopInteract()
	{
		if (closestInteractable != null)
		{
			closestInteractable.CancelInteraction(this); // Stop the interaction process
		}
	}


	public void UseBerry()
	{
		if (IsDead || LilGuyTeam[0].IsDying) return;

		if (lilGuyTeam[0].Health < lilGuyTeam[0].MaxHealth && nextBerryUseTime <= 0 && berryCount > 0)
		{
			int healthRestored = Mathf.CeilToInt(lilGuyTeam[0].MaxHealth * berryHealPercentage);
			EventManager.Instance.HealLilGuy(lilGuyTeam[0], healthRestored);

			berryCount--;
			GameplayStats.BerriesEaten++;
			lilGuyTeam[0].PlaySound("Eat_Berry");
			nextBerryUseTime = berryUsageCooldown;
			EventManager.Instance.UpdatePlayerHealthUI(this);

			EventManager.Instance.UpdatePlayerHealthUI(this);
			playerUi.SetBerryCount(berryCount);
		}

	}

	public void SetActiveLilGuy(LilGuyBase newLilGuy)
	{
		if (activeLilGuy != null)
		{
			activeLilGuy.OnDeath -= HandleLilGuyDeath; // Unsubscribe old
		}

		activeLilGuy = newLilGuy;
		activeLilGuy.IsAttacking = false;

		// Sanitize the list: remove ALL duplicates of newLilGuy
		lilGuyTeam = lilGuyTeam.Distinct().ToList();
		lilGuyTeam.RemoveAll(lg => lg == newLilGuy);
		lilGuyTeam.Insert(0, newLilGuy);

		// Move dead Lil Guys to the end
		var alive = lilGuyTeam.Where(lg => lg.Health > 0).ToList();
		var dead = lilGuyTeam.Where(lg => lg.Health <= 0 && lg != activeLilGuy).ToList();
		lilGuyTeam = new List<LilGuyBase> { activeLilGuy };
		lilGuyTeam.AddRange(alive.Where(lg => lg != activeLilGuy));
		lilGuyTeam.AddRange(dead);

		// Update follow goals
		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			lilGuyTeam[i].SetFollowGoal(lilGuyTeamSlots[i].transform);
		}

		// Rigidbody and visuals
		var rb = activeLilGuy.GetComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.interpolation = RigidbodyInterpolation.None;
		activeLilGuy.transform.localPosition = Vector3.zero;
		activeLilGuy.gameObject.layer = LayerMask.NameToLayer("PlayerLilGuys");

		activeLilGuy.OnDeath += HandleLilGuyDeath;
		activeLilGuy.SetMaterial(GameManager.Instance.OutlinedLilGuySpriteMat);
	}


	/// <summary>
	/// Only use this in Character Select Menu for now
	/// </summary>
	public void UnsubscribeActiveLilGuy()
	{
		if (activeLilGuy != null)
		{
			activeLilGuy.OnDeath -= HandleLilGuyDeath; // Unsubscribe from old Lil Guy
		}
	}

	public void HandleLilGuyDeath()
	{
		if (activeLilGuy.Health > 0) return;

		EventManager.Instance.UpdatePlayerHealthUI(this);
		activeLilGuy.OnEndSpecial(true);
		StopHitStun();

		// Hide them from player, as to not confuse them with a living one... maybe find a better way to convey this
		if (!activeLilGuy.IsDying) lilGuyTeam[0].PlayDeathAnim();
		activeLilGuy.SetLayer(LayerMask.NameToLayer("Player"));

		if (lilGuyTeam.Any(guy => guy.Health > 0))
		{
			LilGuyBase nextAlive = lilGuyTeam.FirstOrDefault(guy => guy.Health > 0);
			if (nextAlive != null)
			{
				lilGuyTeam.Remove(activeLilGuy);
				lilGuyTeam.Add(activeLilGuy); // Move dead Lil Guy to the back

				SetActiveLilGuy(nextAlive);

				// Update follow goals
				for (int i = 0; i < lilGuyTeam.Count; i++)
				{
					lilGuyTeam[i].SetFollowGoal(lilGuyTeamSlots[i].transform);
				}

				foreach (var lilGuy in lilGuyTeam.Skip(1))
				{
					lilGuy.gameObject.layer = LayerMask.NameToLayer("Player");
					lilGuy.GetComponent<Rigidbody>().isKinematic = false;
					lilGuy.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
					lilGuy.IsAttacking = false;
					lilGuy.SetMaterial(GameManager.Instance.RegularLilGuySpriteMat);
				}


				SetInvincible(swapInvincibility);
				GameplayStats.SwitchCharacter(lilGuyTeam[0].GuyName);
				EventManager.Instance.UpdatePlayerHealthUI(this);
			}
		}
		else
		{
			isDead = true;
			if (!ReferenceEquals(lilGuyTeam[0].GetComponent<Hurtbox>().LastHit, null))
				lilGuyTeam[0].GetComponent<Hurtbox>().LastHit.GameplayStats.TeamWipes++;
			GameplayStats.DeathCount++;
			deathTime = Time.time;
			// No living lil guys, time for a respawn if possible.
			if (GameManager.Instance.CurrentPhase == 1)
			{
				EventManager.Instance.ShowRespawnTimer(this);   // Show the respawn screen.
				respawnCoroutine ??= StartCoroutine(DelayedRespawn(true));
			}
			else if (GameManager.Instance.CurrentPhase == 2 && !wasDefeated)
			{
				GameManager.Instance.PlayerDefeat(this);
				wasDefeated = true;
				canMove = true;
				rb.useGravity = false;
				playerUi.DisablePlayerUI();
			}
		}
		EventManager.Instance.RefreshUi(this.playerUi, 0);
	}

	/// <summary>
	/// Swaps the Lil guy based on a queue. If input is right, then the next one in list moves to position 1 and if left, the previous lil guy moves to position 1
	/// and the rest cascade accordingly.
	/// </summary>
	/// <param name="shiftDirection">The input provided by the D-Pad. Negative means they pressed left, and positive means they pressed right.</param>
	public void SwapLilGuy(float shiftDirection)
	{
		if (isSwapping || lilGuyTeam.Count <= 1 || activeLilGuy.IsInSpecialAttack) return;
		if (Time.time < nextSwapTime) return;
		isSwapping = true;
		StopHitStun();
		GameplayStats.SwapCount++;
		List<LilGuyBase> aliveTeam = lilGuyTeam.Where(guy => guy.Health > 0).ToList();
		if (aliveTeam.Count <= 1)
		{
			isSwapping = false;
			return;
		}
		activeLilGuy.OnEndSpecial(true, activeLilGuy.GuyName.Equals("Turteriam"));
		if (shiftDirection < 0) // Left shift
		{
			LilGuyBase first = aliveTeam[0];
			aliveTeam.RemoveAt(0);
			aliveTeam.Add(first);
		}
		else if (shiftDirection > 0) // Right shift
		{
			LilGuyBase last = aliveTeam[^1];
			aliveTeam.RemoveAt(aliveTeam.Count - 1);
			aliveTeam.Insert(0, last);
		}

		lilGuyTeam = aliveTeam.Concat(lilGuyTeam.Where(guy => guy.Health <= 0)).ToList();
		SetActiveLilGuy(lilGuyTeam[0]);

		// Update follow goals
		for (int i = 0; i < lilGuyTeam.Count; i++)
		{
			lilGuyTeam[i].SetFollowGoal(lilGuyTeamSlots[i].transform);
		}

		foreach (var lilGuy in lilGuyTeam.Skip(1))
		{
			lilGuy.gameObject.layer = LayerMask.NameToLayer("Player");
			lilGuy.GetComponent<Rigidbody>().isKinematic = false;
			lilGuy.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
			lilGuy.IsAttacking = false;
			lilGuy.SetMaterial(GameManager.Instance.RegularLilGuySpriteMat);
		}


		SetInvincible(swapInvincibility);
		GameplayStats.SwitchCharacter(lilGuyTeam[0].GuyName);
		EventManager.Instance.UpdatePlayerHealthUI(this);

		isSwapping = false;
		nextSwapTime = Time.time + swapCooldown;
		EventManager.Instance.RefreshUi(playerUi, 0);
		SetTimer();
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
	private void Init(bool isTutorial = false)
	{
		DisableUIControl();
		if (!isTutorial)
		{
			controller.GetComponent<PlayerInput>().DeactivateInput();
			CoroutineRunner.Instance.StartCoroutine(ReactivateInput());
		}
		rb.isKinematic = false;

		playerVolume.gameObject.SetActive(true);
		
		controller.PlayerCam.gameObject.SetActive(true);
		playerInput.camera.clearFlags = CameraClearFlags.Skybox;
		controller.PlayerEventSystem.firstSelectedGameObject = null;
		controller.PlayerEventSystem.gameObject.SetActive(false);
		playerUi.SetBerryCount(berryCount);
		playerUi.SetColour();
		playerUi.MirrorUI(controller.PlayerCam.rect.x >= 0.5);
		playerMesh.SetActive(true);
		SetIcon();
		GameplayStats.CurrentCharacter = lilGuyTeam[0].GuyName;



		//EventManager.Instance.RefreshUi(playerUi, 0);
	}

	public VolumeProfile CloneProfileDeep(VolumeProfile original)
	{
		// Create a new profile instance
		var newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

		// Clone each override manually
		foreach (var setting in original.components)
		{
			var settingCopy = Instantiate(setting);
			newProfile.components.Add(settingCopy);
		}

		return newProfile;
	}

	private IEnumerator ReactivateInput()
	{
		Time.timeScale = 0;
		playerUi.StartGameScreen.gameObject.SetActive(true);
		float timeRemaining = GameManager.Instance.GameplayStartTime;
		while (timeRemaining > 0)
		{
			timeRemaining -= Time.unscaledDeltaTime;
			playerUi.StartGameScreen.TimerImage.fillAmount = timeRemaining / GameManager.Instance.GameplayStartTime;
			playerUi.StartGameScreen.TimerText.text = timeRemaining.ToString("0.0");
			yield return null;
		}
		Time.timeScale = 1;
		playerUi.StartGameScreen.gameObject.SetActive(false);
		if (GameManager.Instance.TimerCanvas != null) GameManager.Instance.TimerCanvas.SetActive(true);   // Show the timer canvas if one exists.
		controller.GetComponent<PlayerInput>().ActivateInput();
	}
	private void SetIcon()
	{
		miniMapIcon.GetComponent<SpriteRenderer>().sprite = UiManager.Instance.shapes[(controller.PlayerNumber) - 1];
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
		EventManager.Instance.RefreshUi(playerUi, 0);
		SetInvincible(respawnInvincibility);
	}

	/// <summary>
	/// This coroutine simply waits respawnTimer, then respawns the given player
	/// </summary>
	/// <returns></returns>
	private IEnumerator DelayedRespawn(bool isPhase1)
	{
		canMove = false;
		rb.velocity = Vector3.zero;
		if (!isPhase1)
		{
			rb.useGravity = false;
		}
		yield return new WaitForSeconds(Managers.GameManager.Instance.RespawnTimer);
		if (isPhase1)
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
		if (GameManager.Instance.IsPaused) return;
		Hurtbox h = lilGuyTeam[0].gameObject.GetComponent<Hurtbox>();
		if (h != null)
		{
			inStorm = true;
			lilGuyTeam[0].PlaySound("Storm_Hurt");
			lilGuyTeam[0].DefaultHurt = false;
			h.TakeDamage(dmg);
			lilGuyTeam[0].DefaultHurt = true;
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

	private void SetTimer()
	{
		playerUi.ResetCDTimer();
		EventManager.Instance.StartAbilityCooldown(PlayerUI, activeLilGuy.CooldownTimer);
	}
}
