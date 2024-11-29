using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

	[Header("Lil Guy Stats")]
	protected int level = 1;
	protected int xp = 0;
	protected int max_xp = 5;
	[SerializeField] protected float health;
	[SerializeField] protected float maxHealth;
	[SerializeField] protected float speed;
	[SerializeField] protected float defense;
	[SerializeField] protected float strength;
	protected const int max_stat = 100;

	[Header("Special Attack Specific")]
	[Tooltip("The length (in seconds) that the special attack should last for.\n(A value of -1 defaults the length to be the same as the special attack animation).")]
	[SerializeField] protected float specialDuration = -1;
	[SerializeField] protected int currentCharges = 1;
	[SerializeField] protected int maxCharges = 1;
	[SerializeField] protected float cooldownDuration = 1;
	[SerializeField] protected float chargeRefreshRate = 1;

	protected PlayerBody playerOwner = null;
	protected float cooldownTimer = 0;
	protected float chargeTimer = 0;
	protected Transform goalPosition;

	private bool isMoving = false;
	private bool isHurt = false;
	private bool isDead = false;

	private bool isAttacking = false;

	// ANIMATION RELATED
	private bool isInBasicAttack = false;
	private bool isInSpecialAttack = false;

	private GameObject instantiatedHitbox;

	// Variables for attack speed and cooldown
	[SerializeField] private float attackCooldown = 1f; // Cooldown duration in seconds
	[SerializeField] private float attackRange = 2f;   // Max attack range
	[SerializeField] private GameObject attackEffect;  // Optional VFX prefab for hit feedback

	private float lastAttackTime = -999f; // Tracks the last time an attack occurred
	private bool flip = false;

	#region Getters and Setters
	public int Level { get => level; set => level = value; }
	public float Health { get => health; set => health = value; }
	public float MaxHealth { get => maxHealth; set => maxHealth = value; }
	public float Speed { get => speed; set => speed = value; }
	public float Defense { get => defense; set => defense = value; }
	public float Strength { get => strength; set => strength = value; }
	public string GuyName { get => guyName; set => guyName = value; }
	public PlayerBody PlayerOwner { get => playerOwner; set => playerOwner = value; }
	public PrimaryType Type { get => type; set => type = value; }
	public bool Flip { get { return flip; } set { flip = value; } }
	public bool IsMoving { get { return isMoving; } set { isMoving = value; } }
	public bool IsAttacking { get { return isAttacking; } set { isAttacking = value; } }
	public bool IsInBasicAttack { get { return isInBasicAttack; } set { isInBasicAttack = value; } }
	public bool IsInSpecialAttack { get { return isInSpecialAttack; } set { isInSpecialAttack = value; } }
	public float CooldownTimer => cooldownTimer;
	public int CurrentCharges => currentCharges;
	public Transform GoalPosition => goalPosition;
	public int MaxStat => max_stat;
	public int Xp { get => xp; set => xp = value; }
	#endregion

	public enum PrimaryType
	{
		Strength,
		Defense,
		Speed
	}

	/// <summary>
	/// Method that sets the goal position that this Lil Guy will try and follow to (if owned by a player)
	/// </summary>
	/// <param name="goalPosition">The goal position the Lil Guy wants to get to.</param>
	public void SetFollowGoal(Transform goalPosition)
	{
		this.goalPosition = goalPosition;
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

	public void SetLayer(LayerMask layer)
	{
		gameObject.layer = layer;
	}

	private void Update()
	{
		// Flip character
		if (flip) mesh.transform.localRotation = Quaternion.Euler(0, 180, 0);
		else mesh.transform.localRotation = Quaternion.identity;

		if (health <= 0)
		{
			health = 0;
			isDead = true;
		}
		else isDead = false;

		if (isDead) return;
		// Replenish cooldown over time.
		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}

		// Regenerate charges over time
		if (currentCharges < maxCharges)
		{
			if (chargeTimer > 0)
			{
				chargeTimer -= Time.deltaTime;
			}
			else
			{
				// Add a charge and reset the timer
				currentCharges++;
				chargeTimer = chargeRefreshRate;
			}
		}

		if (Xp >= max_xp)
		{
			Xp -= max_xp;
			LevelUp();
		}

		if (anim != null) UpdateAnimations();
		if (isAttacking) Attack();
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
		if (anim != null && !isDead) anim.Play("Hurt");
	}

	public void OnDeath()
	{
		if (anim != null) anim.Play("Death");
		GetComponent<Hurtbox>().lastHit.GetComponent<PlayerBody>().LilGuyTeam[0].AddXP(Level * 2);
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
			if (!isInBasicAttack && !isInSpecialAttack)
			{
				anim.SetTrigger("BasicAttack");
			}
		}
		else
		{
			SpawnHitbox();
		}


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
			Destroy(instantiatedHitbox, 0.2f);
		}
	}

	public void DestroyHitbox()
	{

		// Destroy the hitbox after its effect time
		Destroy(instantiatedHitbox); // Shorter lifespan for faster feedback
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
	}

	// Lil Guy constructor :3
	public LilGuyBase(string guyName, int health, int maxHealth, PrimaryType type, int speed, int defense, int strength)
	{
		this.guyName = guyName;
		this.health = health;
		this.maxHealth = maxHealth;
		this.type = type;
		this.speed = speed;
		this.defense = defense;
		this.strength = strength;
	}

	/// <summary>
	/// Add the stats of a given lil guy to this lil guy
	/// </summary>

	private void LevelUp()
	{
		if (level % 5 == 0)
		{
			switch (type)
			{
				case PrimaryType.Strength:
					Strength += Random.Range(5, 8);
					Speed += Random.Range(3, 5);
					Defense += Random.Range(3, 5);
					break;
				case PrimaryType.Defense:
					Strength += Random.Range(3, 5);
					Speed += Random.Range(3, 5);
					Defense += Random.Range(5, 8);
					break;
				case PrimaryType.Speed:
					Strength += Random.Range(3, 5);
					Speed += Random.Range(5, 8);
					Defense += Random.Range(3, 5);
					break;
				default: // somehow we got here
					throw new ArgumentOutOfRangeException();
			}
			MaxHealth += Random.Range(8, 11);
		}
		else
		{
			switch (type)
			{
				case PrimaryType.Strength:
					Strength += Random.Range(3, 5);
					Speed += Random.Range(1, 3);
					Defense += Random.Range(1, 3);
					break;
				case PrimaryType.Defense:
					Strength += Random.Range(1, 3);
					Speed += Random.Range(1, 3);
					Defense += Random.Range(3, 5);
					break;
				case PrimaryType.Speed:
					Strength += Random.Range(1, 3);
					Speed += Random.Range(3, 5);
					Defense += Random.Range(1, 3);
					break;
				default: // somehow we got here
					throw new ArgumentOutOfRangeException();
			}
			MaxHealth += Random.Range(5, 7);
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
}