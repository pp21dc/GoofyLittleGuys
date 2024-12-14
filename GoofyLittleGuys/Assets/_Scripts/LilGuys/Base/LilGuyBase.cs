using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class LilGuyBase : MonoBehaviour
{
	//VARIABLES//
	[Header("Lil Guy Information")]
	[SerializeField] protected string guyName;
	[SerializeField] protected PrimaryType type;
	[SerializeField] protected GameObject mesh;
	[SerializeField] protected GameObject hitboxPrefab;
	[SerializeField] protected Animator anim;
	[SerializeField] protected Transform attackPosition;
	[SerializeField] protected Transform attackOrbit;


	[Header("Lil Guy Stats")]
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
	private Vector3 currentVelocity = Vector3.zero;
	protected Vector3 movementDirection = Vector3.zero;

	// AI Specific
	protected Transform goalPosition;
	protected PlayerBody playerOwner = null;
	private bool isMoving = false;
	private bool lockAttackRotation = false;


	// ANIMATION RELATED
	private bool isAttacking = false;
	private bool isInBasicAttack = false;
	private bool isInSpecialAttack = false;


	// Variables for attack speed and cooldown
	[SerializeField] private float attackCooldown = 1f; // Cooldown duration in seconds
	[SerializeField] private GameObject attackEffect;  // Optional VFX prefab for hit feedback
	private GameObject instantiatedHitbox;
	private float lastAttackTime = -999f; // Tracks the last time an attack occurred

	private bool isDead = false;
	private bool isDying = false;

	#region Getters and Setters
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
	public Vector3 CurrentVelocity => currentVelocity;
	public PrimaryType Type { get => type; set => type = value; }
	public bool IsMoving { get { return isMoving; } set { isMoving = value; } }
	public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
	public bool IsInBasicAttack { get { return isInBasicAttack; } set { isInBasicAttack = value; } }
	public bool IsInSpecialAttack { get { return isInSpecialAttack; } set { isInSpecialAttack = value; } }
	public float CooldownTimer => cooldownTimer;
	public int CurrentCharges => currentCharges;
	public Transform GoalPosition => goalPosition;
	public int MaxStat => max_stat;
	public int Xp { get => xp; set => xp = value; }
	public bool IsDying { get { return isDying; } set { isDying = value; } }
	public bool LockAttackRotation { get { return lockAttackRotation; } set { lockAttackRotation = value; } }
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
	}

	/// <summary>
	/// Method to be called on lil guy instantiation.
	/// </summary>
	/// <param name="layer">The layer that this lil guy should be on.</param>
	public void Init(LayerMask layer)
	{
		SetLayer(layer);
		GetComponentInChildren<AiHealthUi>().gameObject.SetActive(false);
		GetComponent<AiController>().SetState(AiController.AIState.Tamed);
	}
	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
		}
	}
	protected virtual void Update()
	{
		// Flip character
		

		if (GameManager.Instance.IsPaused)
		{
			movementDirection = Vector3.zero;
			rb.velocity = Vector3.zero;
		}

		if (!lockAttackRotation)
		{
			// Flipping Sprite
			if (movementDirection.x > 0) mesh.transform.localRotation = new Quaternion(0, 1, 0, 0);
			else if (movementDirection.x < 0) mesh.transform.localRotation = new Quaternion(0, 0, 0, 1);


			if (movementDirection.sqrMagnitude > 0.01)
			{
				// Calculate the angle using Atan2 (for XZ plane rotation)
				float targetAngle = Mathf.Atan2(movementDirection.z, -movementDirection.x) * Mathf.Rad2Deg;
				// Apply the rotation
				attackOrbit.rotation = Quaternion.Euler(0, targetAngle, 0);
			}
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
		if (Xp >= max_xp && level < 21)
		{
			Xp -= max_xp;
			LevelUp();
		}

		if (anim != null) UpdateAnimations();
		if (isAttacking) Attack();
	}

	private void FixedUpdate()
	{
		// Applying modified gravity
		if (GetComponent<Rigidbody>().velocity.y < 0)
		{
			GetComponent<Rigidbody>().velocity += Vector3.up * Physics.gravity.y * (4 - 1) * Time.fixedDeltaTime;
		}
	}
	private void UpdateAnimations()
	{
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
		StartCoroutine(FlashRed());
		if (anim != null && !isDead && !isInBasicAttack && !isInSpecialAttack) anim.SetTrigger("Hurt");
	}

	public virtual void PlayDeathAnim(bool isWild = false)
	{
		if (isDying) return;
		if (anim != null) anim.Play("Death");
		isDying = true;
		isInBasicAttack = false;
		isAttacking = false;
		isInSpecialAttack = false;
		lockAttackRotation = false;

		GameObject[] hitboxes = GetAllChildren(attackPosition);
		for (int i = hitboxes.Length - 1; i >= 0; i--)
		{
			Destroy(hitboxes[i]);
		}

		if (!isWild)
		{
			if (GetComponent<Hurtbox>().LastHit != null) GetComponent<Hurtbox>().LastHit.LilGuyTeam[0].AddXP(Mathf.FloorToInt(((Level + 4) ^ 2) / 2)); // problem here
			StartCoroutine(Disappear());
		}
		else GetComponent<Hurtbox>().LastHit.LilGuyTeam[0].AddXP(Mathf.FloorToInt(((Level + 4) ^ 2) / 3));
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
		if (anim != null) yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Death") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
		gameObject.SetActive(false);
	}

	/// <summary>
	/// This is the basic attack across all lil guys
	/// it uses a hitbox prefab to detect other ai within it and deal damage from that script
	/// </summary>
	public void Attack()
	{
		// Check if the target is in range
		// Ensure attack respects cooldown
		if (Time.time - lastAttackTime < attackCooldown)
		{
			Debug.Log("Attack on cooldown.");
			return;
		}
		if (anim != null)
		{
			// There are animations made for this lil guy, so set the trigger
			if (!isInBasicAttack && !isInSpecialAttack) anim.SetTrigger("BasicAttack");
		}
		else SpawnHitbox(); // Animations for this lil guy not done yet, so just spawn hitbox.

		// Update attack time
		lastAttackTime = Time.time;
	}

	public void SpawnHitbox()
	{
		// Create the hitbox (snappy, instant feedback)
		if (instantiatedHitbox == null)
		{
			instantiatedHitbox = Instantiate(hitboxPrefab, attackPosition.position, Quaternion.identity, attackPosition);

			// Configure the hitbox
			Hitbox hitbox = instantiatedHitbox.GetComponent<Hitbox>();
			hitbox.Init(gameObject); // Pass the target directly to enhance accuracy
		}

		if (anim == null)
		{
			// There's no animation event to tell this hitbox to destroy itself so we'll do it after 0.2s
			Destroy(instantiatedHitbox, 0.2f);
		}
	}

	public void DestroyHitbox()
	{
		// Destroy the hitbox after its effect time
		Destroy(instantiatedHitbox); // Shorter lifespan for faster feedback
	}

	public virtual void StartChargingSpecial()
	{
		if (playerOwner == null) StopChargingSpecial();
	}

	public virtual void StopChargingSpecial()
	{
		// Decrement charges and reset cooldowns
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
		currentCharges--;
		Special();
	}

	/// <summary>
	/// override this function in all derivitives of this class with its unique special attack
	/// </summary>
	public virtual void Special()
	{
		if (anim != null)
		{
			anim.ResetTrigger("SpecialAttackEnded");
			if (!isInBasicAttack && !isInSpecialAttack)
			{
				anim.SetTrigger("SpecialAttack");
			}
		}
		OnEndSpecial();
	}

	protected virtual void OnEndSpecial()
	{
		StartCoroutine(EndSpecial());
	}

	private IEnumerator EndSpecial()
	{
		if (specialDuration >= 0) yield return new WaitForSeconds(specialDuration);
		else if (specialDuration == -1)
		{
			AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
			if (clip != null) yield return new WaitForSeconds(clip.length);
		}
		if (anim != null)
		{
			anim.SetTrigger("SpecialAttackEnded");
		}
		lockAttackRotation = false;
	}


	/// <summary>
	/// Add the stats of a given lil guy to this lil guy
	/// </summary>
	private void LevelUp()
	{
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
					Speed += secondaryPoints;
					Defense += secondaryPoints;
					break;
				case PrimaryType.Defense:
					Strength += secondaryPoints;
					Speed += secondaryPoints;
					Defense += primaryPoints;
					break;
				case PrimaryType.Speed:
					Strength += secondaryPoints;
					Speed += primaryPoints;
					Defense += secondaryPoints;
					break;
				default: // somehow we got here
					throw new ArgumentOutOfRangeException();
			}
			MaxHealth += primaryPoints;
		}
		Level++;
		max_xp = (Level) * 5;
	}

	public void AddXP(int xpToAdd)
	{
		Xp += xpToAdd;
	}

	public void LeaveDeathAnim()
	{
		if (anim != null) anim.SetTrigger("Revive");
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

	public virtual void MoveLilGuy()
	{
		// Move the creature towards the player with smoothing
		Vector3 targetVelocity = movementDirection.normalized * ((((playerOwner != null) ? speed : speed * 0.75f) + 10) / 3f);
		// Smoothly accelerate towards the target velocity
		currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime / accelerationTime);
		// Apply the smoothed velocity to the Rigidbody
		rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
	}
}