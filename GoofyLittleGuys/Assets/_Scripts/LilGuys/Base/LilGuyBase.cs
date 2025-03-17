using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class LilGuyBase : MonoBehaviour
{
	public int maxCombo = 3; // Max number of attacks in a chain
	public float comboBufferTime = 0.5f; // Time before combo resets
	private int currentComboCount = 0;
	private bool canChainAttack = false;
	public bool CanChainAttack { get => canChainAttack; }
	public int CurrentComboCount { get => currentComboCount; set => currentComboCount = value; }

	private bool attackQueued = false; // For buffering attack inputs


	//VARIABLES//
	[Header("Lil Guy Information")]
	[SerializeField] protected string guyName;
	[SerializeField] protected PrimaryType type;
	[SerializeField] protected SpriteRenderer mesh;
	[SerializeField] protected GameObject hitboxPrefab;
	[SerializeField] protected GameObject basicFxPrefab;
	[SerializeField] protected Animator anim;
	[SerializeField] protected Transform attackPosition;
	[SerializeField] protected Transform attackOrbit;
	[SerializeField] private GameObject healFXPrefab;
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private Sprite uiIcon;
	[SerializeField] private Sprite abilityIcon;

	[Header("Lil Guy Stats")]
	[SerializeField] private int baseSpeed = 13;
	[SerializeField] protected int level = 1;
	[SerializeField] protected int xp = 0;
	[SerializeField] protected int max_xp = 5;
	[SerializeField] protected float health = 50;
	[SerializeField] protected float maxHealth;
	[SerializeField] protected float speed;
	[SerializeField] protected float defense;
	[SerializeField] protected float strength;
	protected const int max_stat = 50;
	private int milestonePoints = 4;
	private int primaryPoints = 2;
	private int secondaryPoints = 1;


	[Header("Special Attack Specific")]
	[Tooltip("The length (in seconds) that the special attack should last for.\n(A value of -1 defaults the length to be the same as the special attack animation).")]
	[SerializeField] protected float specialDuration = -1;
	[SerializeField] protected int currentCharges = 1;
	[SerializeField] protected int maxCharges = 1;
	[SerializeField] protected float cooldownDuration = 1;
	[SerializeField] protected float chargeRefreshRate = 1;

	protected float cooldownTimer = 0;
	protected float chargeTimer = 0;


	[Header("Movement Specific")]
	[SerializeField] private float accelerationTime = 0.1f;  // Time to reach target speed
	private Rigidbody rb;
	private float movementSpeed;
	protected float moveSpeedModifier = 1;
	[SerializeField] private float knockbackResistance = 1f;
	[SerializeField] private float knockbackDecayRate = 5f;
	private Vector3 knockbackForce = Vector3.zero;
	private Coroutine hitstunCoroutine = null;	// for da hitstun
	private float hitStunSlowMult = 1f;

	private Vector3 currentVelocity = Vector3.zero;
	protected Vector3 movementDirection = Vector3.zero;

	[Header("FX")]
	[SerializeField] private float dustSpawnInterval = 1f;
	private Coroutine dustSpawnCoroutine;

	// AI Specific
	protected Transform goalPosition;
	protected PlayerBody playerOwner = null;
	private bool isMoving = false;
	private bool lockMovement = false;
	private bool lockAttackRotation = false;


	// ANIMATION RELATED
	private bool isAttacking = false;
	private bool isInBasicAttack = false;
	private bool isInSpecialAttack = false;


	// Variables for attack speed and cooldown
	[SerializeField] private float attackCooldown = 1f; // Cooldown duration in seconds
	[SerializeField] private GameObject attackEffect;  // Optional VFX prefab for hit feedback
	GameObject levelUpEffect;
	public GameObject LevelUpEffect => levelUpEffect;
	private GameObject instantiatedHitbox;
	private GameObject instantiatedBasicFx;
	private float lastAttackTime = -999f; // Tracks the last time an attack occurred

	private bool isDead = false;
	private bool isInvincible = false;
	private bool isDying = false;
	private bool spawnedDustParticle = false;

	public event Action OnDeath;

	#region Getters and Setters
	public Transform AttackOrbit => attackOrbit;
	public SpriteRenderer Mesh => mesh;
	public int BaseSpeed { get { return baseSpeed; } set { baseSpeed = value; } }
	public int Level { get => level; set => level = value; }
	public float Health { get => health; set => health = value; }
	public float MaxHealth { get => maxHealth; set => maxHealth = value; }
	public float Speed { get => speed; set => speed = value; }
	public float Defense { get => defense; set => defense = value; }
	public float Strength { get => strength; set => strength = value; }
	public string GuyName { get => guyName; set => guyName = value; }
	public PlayerBody PlayerOwner { get => playerOwner; set => playerOwner = value; }
	public Rigidbody RB => rb;
	public Vector3 MovementDirection { get { return movementDirection; } set { movementDirection = value; } }
	public float MovementSpeed { get { return movementSpeed; } set => movementSpeed = value; }
	public float MoveSpeedModifier { get { return moveSpeedModifier; } set => moveSpeedModifier = value; }
	public Vector3 CurrentVelocity => currentVelocity;
	public PrimaryType Type { get => type; set => type = value; }
	public bool IsMoving { get { return isMoving; } set { isMoving = value; } }
	public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
	public bool IsInBasicAttack { get { return isInBasicAttack; } set { isInBasicAttack = value; } }
	public bool IsInSpecialAttack { get { return isInSpecialAttack; } set { isInSpecialAttack = value; } }
	public float CooldownTimer => cooldownTimer;
	public float CooldownDuration => CooldownDuration;
	public int CurrentCharges => currentCharges;
	public Transform GoalPosition => goalPosition;
	public int MaxStat => max_stat;
	public int Xp { get => xp; set => xp = value; }
	public bool IsDying { get { return isDying; } set { isDying = value; } }
	public bool IsInvincible { get { return isInvincible; } set { isInvincible = value; } }
	public bool LockAttackRotation { get { return lockAttackRotation; } set { lockAttackRotation = value; } }
	public bool LockMovement { get { return lockMovement; } set { lockMovement = value; } }
	public Animator Animator { get { return anim; } }

	public int MaxXp { get { return max_xp; } }

	public Sprite Icon { get { return uiIcon; } }
	public Sprite AbilityIcon { get { return abilityIcon; } }
	#endregion

	public enum PrimaryType
	{
		Strength,
		Defense,
		Speed
	}
	protected virtual void Start()
	{
		rb = GetComponent<Rigidbody>();
		GameObject instantiantedFX = Instantiate(FXManager.Instance.GetEffect("Spawn"), transform.position, Quaternion.identity);
		instantiantedFX.GetComponent<SpriteRenderer>().sortingOrder = (int)-transform.position.z - 1;
		Vector3 initialScale = gameObject.transform.localScale;
		StartCoroutine(ScaleUp(initialScale));
		CalculateMoveSpeed();
	}

	public void ResetTimers()
	{
		cooldownTimer = 0;
		currentCharges = maxCharges;
		chargeTimer = 0;
	}
	private IEnumerator ScaleUp(Vector3 scaleTo)
	{
		float elapsedTime = 0;
		while (elapsedTime < 0.25f)
		{
			transform.localScale = Vector3.Lerp(Vector3.zero, scaleTo, elapsedTime / 0.25f);
			elapsedTime += Time.unscaledDeltaTime;
			yield return null;
		}
		transform.localScale = scaleTo; // Ensure the final scale is applied
	}
	public void DetermineLevel()
	{
		int count = 0;
		int totalLevels = 0;
		foreach (PlayerBody body in GameManager.Instance.Players)
		{
			foreach (LilGuyBase lilGuy in body.LilGuyTeam)
			{
				count++;
				totalLevels += lilGuy.level;
			}
		}

		int averageLevel = Mathf.FloorToInt(totalLevels / count);
		SetWildLilGuyLevel(averageLevel);
	}

	public void CreateLegendary(float maxScale, int level)
	{
		transform.localScale = Vector3.one * maxScale;
		SetWildLilGuyLevel(level, false);
		WildBehaviour behaviour = GetComponent<WildBehaviour>();
		behaviour.IsCatchable = false;
		behaviour.AttackRange *= maxScale;
		behaviour.ChaseRange *= maxScale;
	}

	private void SetWildLilGuyLevel(int level, bool randomRange = true)
	{
		if (randomRange) level = Mathf.Clamp(Mathf.FloorToInt(UnityEngine.Random.Range(level - 1f, level + 2f)), 1, 24); // Add a variance of ~1 level greater/less
		this.level = level;

		float healthRatio = health / maxHealth;
		int numOfMilestonesMet = Mathf.FloorToInt(level / 5);
		Strength += (milestonePoints * numOfMilestonesMet);
		Speed += (milestonePoints * numOfMilestonesMet);
		Defense += (milestonePoints * numOfMilestonesMet);
		MaxHealth += (milestonePoints * numOfMilestonesMet);

		switch (type)
		{
			case PrimaryType.Strength:
				Strength += primaryPoints * (level - 1);
				Speed += secondaryPoints * (level - 1);
				Defense += secondaryPoints * (level - 1);
				break;
			case PrimaryType.Defense:
				Strength += secondaryPoints * (level - 1);
				Speed += secondaryPoints * (level - 1);
				Defense += primaryPoints * (level - 1);
				break;
			case PrimaryType.Speed:
				Strength += secondaryPoints * (level - 1);
				Speed += primaryPoints * (level - 1);
				Defense += secondaryPoints * (level - 1);
				break;
			default: // somehow we got here
				throw new ArgumentOutOfRangeException();
		}
		MaxHealth += primaryPoints * (level - 1);   // Exclude first level.
		health = Mathf.CeilToInt(healthRatio * maxHealth);
	}
	/// <summary>
	/// Method to be called on lil guy instantiation.
	/// </summary>
	/// <param name="layer">The layer that this lil guy should be on.</param>
	public void Init(LayerMask layer)
	{
		SetLayer(layer);
		AiHealthUi enemyHealthUI = GetComponentInChildren<AiHealthUi>();
		if (enemyHealthUI != null) enemyHealthUI.gameObject.SetActive(false);

		GetComponent<AiController>().SetState(AiController.AIState.Tamed);
	}

	public void SetMaterial(Material material)
	{
		mesh.material = material;
		mesh.transform.localScale = (material == GameManager.Instance.OutlinedLilGuySpriteMat) ? Vector3.one * GameManager.Instance.ActiveLilGuyScaleFactor : Vector3.one * GameManager.Instance.NonActiveLilGuyScaleFactor;
	}
	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
			isAttacking = false;
		}
	}
	protected virtual void Update()
	{

		// Flip character
		mesh.sortingOrder = (int)-transform.position.z;
		if (GameManager.Instance.IsPaused)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
		}

		if (!lockAttackRotation)
		{
			// Flipping Sprite
			if (movementDirection.x > 0) mesh.flipX = true;
			else if (movementDirection.x < 0) mesh.flipX = false;

			// Rotating sprite slightly when moving forward/backward. I'm so sorry how disgusting this looks, but I needed the 0 case... if there's a way to check for 0 without this mess of if statements, please do
			if (movementDirection.z > 0 && Mathf.Abs(movementDirection.x) <= Mathf.Abs(movementDirection.z))
			{
				if (movementDirection.x < 0) mesh.transform.rotation = new Quaternion(0, 0.173648164f, 0, 0.984807789f);
				else if (movementDirection.x > 0) mesh.transform.rotation = new Quaternion(0, -0.173648253f, 0, 0.984807789f);       // 20 degrees on y axis : -20 degrees on y axis
				else mesh.transform.rotation = new Quaternion(0, 0, 0, 1);
			}
			else if (movementDirection.z < 0 && Mathf.Abs(movementDirection.x) <= Mathf.Abs(movementDirection.z))
			{
				if (movementDirection.x > 0) mesh.transform.rotation = new Quaternion(0, 0.173648164f, 0, 0.984807789f);
				else if (movementDirection.x < 0) mesh.transform.rotation = new Quaternion(0, -0.173648253f, 0, 0.984807789f);       // 20 degrees on y axis : -20 degrees on y axis
				else mesh.transform.rotation = new Quaternion(0, 0, 0, 1);
			}
			else mesh.transform.rotation = new Quaternion(0, 0, 0, 1);

			if (movementDirection.sqrMagnitude > 0.01)
			{
				if (!spawnedDustParticle)
				{
					GameObject spawnedParticle = Instantiate(FXManager.Instance.GetEffect("Dust"), transform.position, Quaternion.identity);
					spawnedParticle.GetComponent<SpriteRenderer>().flipX = mesh.flipX;
					spawnedDustParticle = true;
				}
				// Calculate the angle using Atan2 (for XZ plane rotation)
				float targetAngle = Mathf.Atan2(movementDirection.z, -movementDirection.x) * Mathf.Rad2Deg;
				// Apply the rotation
				attackOrbit.rotation = Quaternion.Euler(0, targetAngle, 0);
			}
			else spawnedDustParticle = false;
		}

		if (health <= 0)
		{
			health = 0;
			isDead = true;
		}
		else isDead = false;

		if (isDead) return;

		// Replenish cooldown over time.
		if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

		// Regenerate charges over time
		if (currentCharges < maxCharges)
		{
			if (chargeTimer > 0) chargeTimer -= Time.deltaTime;
			else
			{
				// Add a charge and reset the timer
				currentCharges++;
				chargeTimer = chargeRefreshRate;
			}
		}

		// Level Up
		if (Xp >= max_xp && level < 20)
		{
			Xp -= max_xp;
			LevelUp();
		}

		if (anim != null) UpdateAnimations();

		if (isAttacking) AttemptAttack();
		// Reset combo if buffer time is exceeded
		if (Time.time - lastAttackTime > comboBufferTime && currentComboCount > 0)
		{
			ResetCombo();
		}
	}

	public void StartAttacking()
	{
		isAttacking = true;
		AttemptAttack(); // Start attack immediately
	}

	public void StopAttacking()
	{
		isAttacking = false; // Stop attacks when button is released
	}

	/// <summary>
	/// Called by PlayerController.cs when the player presses attack
	/// </summary>
	public void AttemptAttack()
	{
		if (isDead || ((playerOwner != null) && !isAttacking)) return;

		// If first attack in the combo, trigger immediately
		if (currentComboCount == 0 && Time.time - lastAttackTime > comboBufferTime)
		{
			PerformAttack();
		}
		else if (currentComboCount > 0) 
		{
			// Otherwise, queue attack to execute when animation allows it
			attackQueued = true;
		}
	}

	/// <summary>
	/// Animation Event: Called at the right frame to allow chaining.
	/// </summary>
	public void EnableChainAttack()
	{
		canChainAttack = true;

		// If attack is queued (player pressed attack within the window), continue the combo
		if (attackQueued && currentComboCount < maxCombo && isAttacking)
		{
			attackQueued = false; // Clear the queue since we're attacking now
			PerformAttack();
		}
	}

	public float GetTotalComboTime()
	{
		if (anim == null) return 0.5f; // Default safety buffer

		// Get the length of the attack animation
		float attackAnimTime = anim.GetCurrentAnimatorStateInfo(0).length;

		// Total time = animation length * combo count, with slight padding for reaction time
		return (attackAnimTime * maxCombo) + 0.1f;
	}


	/// <summary>
	/// Executes the attack only if allowed by animation timing.
	/// </summary>
	private void PerformAttack()
	{
		currentComboCount++;
		lastAttackTime = Time.time; // Update last attack time

		if (anim != null)
		{
			if (!isInSpecialAttack)
			{
				anim.Play("BasicAttack", 0, 0f); // Restart the attack animation
			}
		}
		else
		{
			SpawnHitbox(); // Animations not done yet, just spawn hitbox.
		}
	}

	/// <summary>
	/// Animation Event: Called near the end of animation to check for a combo.
	/// </summary>
	public void CheckCombo()
	{
		if (canChainAttack && attackQueued && currentComboCount < maxCombo && isAttacking)
		{
			attackQueued = false; // Reset queue so it doesn't trigger twice
			PerformAttack();
		}
		else if (currentComboCount >= maxCombo)
		{
			// Only now, after the full combo is complete, apply the buffer
			lastAttackTime = Time.time; // Mark when the combo ended
			ResetCombo();
		}
		else
		{
			ResetCombo(); // If combo is over, reset
		}
	}

	private void ResetCombo()
	{
		currentComboCount = 0;
		canChainAttack = false;
		attackQueued = false; // Make sure attack queue is cleared
	}
	private void FixedUpdate()
	{
		// Applying modified gravity
		if (!IsGrounded() && !rb.isKinematic)
		{
			rb.velocity += Vector3.up * Physics.gravity.y * (speed - 1) * Time.fixedDeltaTime;
		}

		// Always apply knockback, even when not moving
		if (!isMoving && knockbackForce.magnitude > 0.1f)
		{
			// Apply knockback to Rigidbody
			rb.velocity = new Vector3(knockbackForce.x, rb.velocity.y, knockbackForce.z);

			// Knockback decays over time
			knockbackForce = Vector3.Lerp(knockbackForce, Vector3.zero, Time.fixedDeltaTime * knockbackDecayRate);
		}
	}


	private void UpdateAnimations()
	{
		anim.SetBool("IsDead", isDead);
		if (!isDead)
		{
			anim.SetBool("IsMoving", isMoving);
		}
	}

	/// <summary>
	/// Coroutine that flashes the lil guy red when damaged.
	/// </summary>
	/// <returns></returns>
	private IEnumerator FlashRed()
	{
		float timeElapsed = 0f;
		SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
		renderer.color = Color.red;

		while (timeElapsed < 1.5f)
		{
			// Calculate progress
			float progress = timeElapsed / 1.5f;

			// Interpolate the green channel from 1 to 0
			Color newColor = new Color(1, Mathf.Lerp(0, 1, progress), Mathf.Lerp(0, 1, progress), 1);
			renderer.color = newColor;

			// Increase elapsed time and wait until next frame
			timeElapsed += Time.deltaTime;
			yield return null;
		}

		renderer.color = Color.white;
	}

	/// <summary>
	/// Helper method called when the lil guy is damaged.
	/// </summary>
	public void Damaged()
	{
		if (health <= 0)
		{
			OnDeath?.Invoke();
			StopHitStun();
			mesh.color = Color.white;
			return;
		}
		if (isDead) return;
		StartCoroutine(FlashRed());
		if (anim != null && !isInBasicAttack && !isInSpecialAttack) anim.SetTrigger("Hurt");
	}

	public virtual void PlayDeathAnim(bool isWild = false)
	{
		if (isDying) return;
		RemoveSpeedBoost();
		isDying = true;
		anim.SetBool("IsDead", true);
		isInBasicAttack = false;
		isAttacking = false;
		isInSpecialAttack = false;
		lockAttackRotation = false;

		GameObject[] hitboxes = GetAllChildren(attackPosition);
		for (int i = hitboxes.Length - 1; i >= 0; i--)
		{
			Destroy(hitboxes[i]);
		}

		Hurtbox h = GetComponent<Hurtbox>();
		if (!isWild)
		{
			// Player owned lil guy died.
			if (h.LastHit != null)
			{
				DebugManager.Log($"{name} was a player-owned Lil Guy, and was defeated by player {h.LastHit}. Awarding bonus xp.", DebugManager.DebugCategory.COMBAT);
				for (int i = 0; i < h.LastHit.LilGuyTeam.Count; i++)
				{
					h.LastHit.LilGuyTeam[i].AddXP((i == 0) ? Mathf.FloorToInt((Mathf.Pow((Level + 4), 2)) / 2) : Mathf.FloorToInt((Mathf.Pow((Level + 4), 2)) / 5));
				}
			}
			StartCoroutine(Disappear());
		}
		else
		{
			DebugManager.Log($"{name} was a wild Lil Guy, and was defeated by player {GetComponent<Hurtbox>().LastHit}. Awarding XP.", DebugManager.DebugCategory.COMBAT);
			for (int i = 0; i < h.LastHit.LilGuyTeam.Count; i++)
			{
				h.LastHit.LilGuyTeam[i].AddXP((i == 0) ? Mathf.FloorToInt((Mathf.Pow((Level + 2), 2) / 3)) : Mathf.FloorToInt((Mathf.Pow((Level + 2), 2) / 6)));
			}
			h.LastHit.GameplayStats.WildLilGuysDefeated++;
			isDying = false;
		}
	}

	GameObject[] GetAllChildren(Transform parent)
	{
		List<GameObject> children = new List<GameObject>();

		foreach (Transform child in parent)
		{
			children.Add(child.gameObject);
		}

		return children.ToArray();
	}

	/// <summary>
	/// Sets the Lil Guy to inactive after their death animation has played fully.
	/// </summary>
	/// <returns></returns>
	private IEnumerator Disappear()
	{
		if (anim != null)
		{
			yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Death") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
		}
		SpawnDeathParticle();
		gameObject.SetActive(false);
	}

	public void SpawnDeathParticle()
	{
		GameObject instantiatedFX = Instantiate(FXManager.Instance.GetEffect("DeathCloud"), transform.position + Vector3.up + Vector3.back, Quaternion.identity);
	}
	/// <summary>
	/// This is the basic attack across all lil guys
	/// it uses a hitbox prefab to detect other ai within it and deal damage from that script
	/// </summary>
	public void Attack()
	{
		if (isDead) return;
		// Check if the target is in range
		// Ensure attack respects cooldown
		if (Time.time - lastAttackTime < attackCooldown && playerOwner == null)
		{
			Managers.DebugManager.Log("Attack on cooldown.", Managers.DebugManager.DebugCategory.COMBAT);
			return;
		}
		if (anim != null)
		{
			// There are animations made for this lil guy, so set the trigger
			if (!isInBasicAttack && !isInSpecialAttack)
			{
				anim.SetTrigger("BasicAttack");
			}
		}
		else SpawnHitbox(); // Animations for this lil guy not done yet, so just spawn hitbox.

		// Update attack time
		lastAttackTime = Time.time;
	}

	public void SpawnHitbox()
	{
		// Create the hitbox (snappy, instant feedback)

		instantiatedHitbox = Instantiate(hitboxPrefab, attackPosition.position, Quaternion.identity, attackPosition);

		// Configure the hitbox
		Hitbox hitbox = instantiatedHitbox.GetComponent<Hitbox>();
		hitbox.Init(gameObject); // Pass the target directly to enhance accuracy

		KnockbackHitbox k = instantiatedHitbox.GetComponent<KnockbackHitbox>();
		k.KnockbackDir = (playerOwner == null) ? MovementDirection.normalized : playerOwner.MovementDirection.normalized;

		// Spawn in Fx prefab along-side hitbox
		instantiatedBasicFx = Instantiate(basicFxPrefab, attackPosition.position, Quaternion.identity, gameObject.transform);
		var fx = instantiatedBasicFx.GetComponent<BasicAttackFx>();
		fx.Init(this);


		if (anim == null)
		{
			// There's no animation event to tell this hitbox to destroy itself so we'll do it after 0.2s
			Destroy(instantiatedHitbox, 0.2f);
			Destroy(instantiatedBasicFx, 0.2f);
		}
	}

	public void DestroyHitbox()
	{
		// Destroy the hitbox after its effect time
		Destroy(instantiatedHitbox); // Shorter lifespan for faster feedback
	}

	public void PlaySound(string key)
	{
		Managers.AudioManager.Instance.PlaySfx(key, audioSource);
	}

	//Changes this source to loop and plays a sound until StopLoopingSound is called
	public void PlayLoopingSound(string key)
	{
		if (audioSource.loop == false)
		{
			audioSource.loop = true;
		}
		Managers.AudioManager.Instance.PlaySfx(key, audioSource);
	}

	//Changes this source to not loop and stops whatever it is currently playing
	public void StopLoopingSound()
	{
		if (audioSource.loop == true && audioSource.isPlaying)
		{
			audioSource.loop = false;
			audioSource.Stop();
		}
	}

	/// <summary>
	/// Specifically for playing a sound on a spawned object or effect
	/// </summary>
	/// <param name="key"></param>
	public void PlayEffectSound(GameObject effect, string key)
	{
		AudioSource soundSource = effect.GetComponent<AudioSource>();
		if (soundSource != null)
		{
			Managers.AudioManager.Instance.PlaySfx(key, soundSource);
		}
	}

	public virtual void StartChargingSpecial()
	{
		if (isDead) return;
		if (playerOwner == null) StopChargingSpecial();
	}

	public virtual void StopChargingSpecial()
	{
		if (isDead) return;
		// Decrement charges and reset cooldowns
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
		Special();
	}

	public virtual void CancelSpecial()
	{
	}
	/// <summary>
	/// override this function in all derivitives of this class with its unique special attack
	/// </summary>
	protected virtual void Special()
	{
		if (isDead) return;
		if (anim != null)
		{
			anim.ResetTrigger("SpecialAttackEnded");
			if (!isInSpecialAttack)
			{
				anim.SetTrigger("SpecialAttack");
			}
		}

		if (playerOwner != null)
		{
			playerOwner.GameplayStats.SpecialsUsed++; 
			EventManager.Instance.StartAbilityCooldown(playerOwner.PlayerUI, cooldownDuration);
		}
		OnEndSpecial();
	}

	public virtual void OnEndSpecial(bool stopImmediate = false)
	{
		StartCoroutine(EndSpecial(stopImmediate));
	}

	protected virtual IEnumerator EndSpecial(bool stopImmediate = false)
	{
		if (!stopImmediate)
		{
			if (specialDuration >= 0) yield return new WaitForSeconds(specialDuration);
			else if (specialDuration == -1)
			{
				AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
				if (clip != null) yield return new WaitForSeconds(clip.length);
			}
		}
		if (anim != null)
		{
			anim.SetTrigger("SpecialAttackEnded");
		}
		lockAttackRotation = false;
		lockMovement = false;
	}


	/// <summary>
	/// Add the stats of a given lil guy to this lil guy
	/// </summary>
	private void LevelUp()
	{
		levelUpEffect = Instantiate(FXManager.Instance.GetEffect("LevelUp"), transform.position + Vector3.forward + Vector3.up * 0.25f, Quaternion.identity, transform);
		PlayEffectSound(levelUpEffect, "Level_Up");
		if (level % 5 == 0)
		{
			Strength += milestonePoints;
			Speed += milestonePoints;
			Defense += milestonePoints;
			MaxHealth += milestonePoints;
		}
		else
		{
			switch (type)
			{
				case PrimaryType.Strength:
					Strength += primaryPoints;
					Speed += primaryPoints;
					Defense += primaryPoints;
					break;
				case PrimaryType.Defense:
					Strength += primaryPoints;
					Speed += primaryPoints;
					Defense += primaryPoints;
					break;
				case PrimaryType.Speed:
					Strength += primaryPoints;
					Speed += primaryPoints;
					Defense += primaryPoints;
					break;
				default: // somehow we got here
					throw new ArgumentOutOfRangeException();
			}
			MaxHealth += primaryPoints;
		}
		Level++;
		max_xp = (Level) * 5;

		CalculateMoveSpeed();
	}

	public void AddXP(int xpToAdd)
	{
		Xp += xpToAdd;
	}

	public void LeaveDeathAnim()
	{
	}

	public GameObject GetHitboxPrefab()
	{
		return hitboxPrefab;
	}

	public Transform GetAttackPosition()
	{
		return attackPosition;
	}

	/// <summary>
	/// Method that sets the goal position that this Lil Guy will try and follow to (if owned by a player)
	/// </summary>
	/// <param name="goalPosition">The goal position the Lil Guy wants to get to.</param>
	public void SetFollowGoal(Transform goalPosition)
	{
		this.goalPosition = goalPosition;
	}


	public void SetLayer(LayerMask layer)
	{
		gameObject.layer = layer;
	}

	private bool IsGrounded()
	{
		return Physics.Raycast(rb.position + Vector3.up, Vector3.down, 2f, LayerMask.GetMask("Ground"));
	}

	public void CalculateMoveSpeed()
	{
		movementSpeed = (baseSpeed + ((playerOwner != null) ? speed : speed * 0.75f) * 0.3f);
	}

	public virtual void MoveLilGuy(float speedAdjustment = 1f)
	{
		if (knockbackForce.magnitude > 0.1f)
		{
			// Knockback decays over time
			knockbackForce = Vector3.Lerp(knockbackForce, Vector3.zero, Time.fixedDeltaTime * knockbackDecayRate);
		}

		if (!lockMovement)
		{
			Vector3 velocity = rb.velocity;
			velocity.y = 0;

			// Move towards the player with smoothing

			Vector3 targetVelocity = movementDirection.magnitude < 0.1f
				? Vector3.zero
				: movementDirection.normalized * movementSpeed * speedAdjustment * moveSpeedModifier;
			// Smoothly accelerate towards the target velocity
			currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime / accelerationTime);

			// Apply movement and knockback force
			rb.velocity = new Vector3((currentVelocity.x/hitStunSlowMult) + knockbackForce.x, rb.velocity.y, (currentVelocity.z/hitStunSlowMult) + knockbackForce.z);
		}
		else
		{
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
			currentVelocity = Vector3.zero;
		}
	}

	// Method to apply knockback
	public void ApplyKnockback(Vector3 force)
	{
		knockbackForce = force / knockbackResistance;
	}

	public void StartHitStun(float stunMult, float stunTime, AnimationCurve stunCurve)
	{
		hitstunCoroutine = StartCoroutine(ApplyHitStun(stunMult, stunTime, stunCurve));
	}

	private IEnumerator ApplyHitStun(float stunMult, float stunTime, AnimationCurve stunCurve)
	{
		//set stun mult
		//lerp stun mult back to 1 over stuntime
		//change animator speed
		
		hitStunSlowMult = stunMult;
		var timer = 0.0f;
		while (timer <  stunTime)
		{
			timer += Time.deltaTime;
			var t = Mathf.Clamp01(timer / stunTime);
			var curveVal = stunCurve.Evaluate(t);
			hitStunSlowMult = Mathf.Lerp(stunMult, 1.0f, curveVal);
			Animator.speed = 0.0f;
			yield return null;
		}
		Animator.speed = 1.0f;
		hitStunSlowMult = 1.0f;
	}

	private void StopHitStun()
	{
		hitStunSlowMult = 1.0f;
		Animator.speed = 1.0f;
		StopCoroutine(hitstunCoroutine);
	}

	public void ApplySpeedBoost(float spawnInterval, int maxAfterImages, float fadeSpeed, Color emissionColour)
	{
		AfterimageEffect afterimageController = GetComponent<AfterimageEffect>();
		if (afterimageController == null)
		{
			afterimageController = gameObject.AddComponent<AfterimageEffect>();
			afterimageController.CharacterSprite = mesh.GetComponent<SpriteRenderer>();
		}
		afterimageController.SpawnInterval = spawnInterval;
		afterimageController.FadeSpeed = fadeSpeed;
		afterimageController.MaxAfterimages = maxAfterImages;
		afterimageController.EmissionColour = emissionColour;
		afterimageController.StartAfterimages();
	}

	public void RemoveSpeedBoost()
	{
		AfterimageEffect afterimageController = GetComponent<AfterimageEffect>();
		if (afterimageController != null)
		{
			afterimageController.StopAfterimages();
		}
	}

	public void PlayHealEffect()
	{
		GameObject instantiatedHeal = Instantiate(FXManager.Instance.GetEffect("Heal"), transform.position, Quaternion.identity, transform);
		instantiatedHeal.GetComponent<SpriteRenderer>().sortingOrder = mesh.sortingOrder + 1;
		PlayEffectSound(instantiatedHeal, "Heal");
	}

	protected virtual void OnDisable()
	{
		RemoveSpeedBoost();

		Poison poison = GetComponent<Poison>();
		if (poison != null) Destroy(poison);

		Slow slow = GetComponent<Slow>();
		if (slow != null) Destroy(slow);

		IsAttacking = false;
	}
}

public enum DebuffType
{
	Slow,
	Poison
}