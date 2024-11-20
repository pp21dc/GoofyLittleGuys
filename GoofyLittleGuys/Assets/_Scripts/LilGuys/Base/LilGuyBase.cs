using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public abstract class LilGuyBase : MonoBehaviour
{
	//VARIABLES//
	[Header("Lil Guy Information")]
	public string guyName;
	public PrimaryType type;
	public GameObject mesh;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private GameObject hitboxPrefab;
	[SerializeField] private Animator anim;
	public Transform attackPosition;

	[Header("Lil Guy Stats")]
	public float health;
	public float maxHealth;
	public float speed;
	public float defense;
	public float strength;
	public const int MAX_STAT = 100;
	private int average;

	[Header("Special Attack Specific")]
	[Tooltip("The length (in seconds) that the special attack should last for.\n(A value of -1 defaults the length to be the same as the special attack animation).")]
	[SerializeField] protected float specialDuration = -1;
	[SerializeField] protected int currentCharges = 1;
	[SerializeField] protected int maxCharges = 1;
	[SerializeField] protected float cooldownDuration = 1;
	[SerializeField] protected float chargeRefreshRate = 1;

	public GameObject playerOwner = null;
	protected float cooldownTimer = 0;
	protected float chargeTimer = 0;
	private Transform goalPosition;

	private bool isMoving = false;
	private bool isHurt = false;
	private bool isDead = false;
	private GameObject instantiatedHitbox;

	// Variables for attack speed and cooldown
	[SerializeField] private float attackCooldown = 1f; // Cooldown duration in seconds
	[SerializeField] private float attackRange = 2f;   // Max attack range
	[SerializeField] private GameObject attackEffect;  // Optional VFX prefab for hit feedback

	private float lastAttackTime = -999f; // Tracks the last time an attack occurred
	private bool flip = false;

	public bool Flip { get { return flip; } set { flip = value; } }
	public bool IsMoving { get { return isMoving; } set { isMoving = value; } }
	public float CooldownTimer => cooldownTimer;
	public int CurrentCharges => currentCharges;
	public Transform GoalPosition => goalPosition;

	public enum PrimaryType
	{
		Strength,
		Defense,
		Speed
	}

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
		gameObject.layer = layer;
		GetComponentInChildren<AiHealthUi>().gameObject.SetActive(false);
		GetComponent<AiController>().SetState(AiController.AIState.Tamed);
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

		if (anim != null) UpdateAnimations();

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
		GetComponentInChildren<SpriteRenderer>().color = Color.red;

		while (timeElapsed < 1.5f)
		{
			// Calculate progress
			float progress = timeElapsed / 1.5f;

			// Interpolate the green channel from 1 to 0
			Color newColor = new Color(1, Mathf.Lerp(0, 1, progress), Mathf.Lerp(0, 1, progress), 1);
			GetComponentInChildren<SpriteRenderer>().color = newColor;

			// Increase elapsed time and wait until next frame
			timeElapsed += Time.deltaTime;
			yield return null;
		}

		GetComponentInChildren<SpriteRenderer>().color = Color.white;
	}

	/// <summary>
	/// Helper method called when the lil guy is damaged.
	/// </summary>
	public void Damaged()
	{
		StartCoroutine(FlashRed());
		if (anim != null) anim.Play("Hurt");
	}

	public void OnDeath()
	{
		if (anim != null) anim.Play("Death");
	}

	/// <summary>
	/// This is the basic attack across all lil guys\
	/// it uses a hitbox prefab to detect other ai within it and deal damage from that script
	/// NOTE: the second value in destroy (line 29) is the duration that the attack lasts
	/// </summary>
	public void Attack()
	{
		// Check if the target is in range
		if (anim != null) anim.SetTrigger("BasicAttack");
		// Ensure attack respects cooldown
		if (Time.time - lastAttackTime < attackCooldown)
		{
			Debug.Log("Attack on cooldown.");
			return;
		}

		// Update attack time
		lastAttackTime = Time.time;

		// Create the hitbox (snappy, instant feedback)
		if (instantiatedHitbox == null)
		{
			instantiatedHitbox = Instantiate(hitboxPrefab, attackPosition.position, Quaternion.identity);
			instantiatedHitbox.transform.SetParent(null); // Detach for independent lifetime

			// Configure the hitbox
			Hitbox hitbox = instantiatedHitbox.GetComponent<Hitbox>();
			hitbox.layerMask = playerOwner != null ? playerOwner.layer : gameObject.layer;
			hitbox.Init(gameObject); // Pass the target directly to enhance accuracy

			// Destroy the hitbox after its effect time
			Destroy(instantiatedHitbox, 0.2f); // Shorter lifespan for faster feedback
		}
	}

	/// <summary>
	/// override this function in all derivitives of this class with its unique special attack
	/// </summary>
	public virtual void Special()
	{
		if (anim != null) anim.SetTrigger("SpecialAttack");
		StartCoroutine(EndSpecial());
	}

	private IEnumerator EndSpecial()
	{
		if (specialDuration >= 0)
			yield return new WaitForSeconds(specialDuration);
		else if (specialDuration == -1)
		{
			AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
			if (clip != null)
				yield return new WaitForSeconds(clip.length);
		}
		if (anim != null) anim.SetTrigger("SpecialAttackEnded");
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
	public void AddCaptureStats(LilGuyBase defeatedLilGuy)
	{
		float primaryModifier = 1.5f;
		float secondaryModifier = 3.0f;
		Debug.Log("Adding capture stats... Before: Str - " + strength + " Def - " + defense + " Spd - " + speed);
		switch (defeatedLilGuy.type)
		{
			case PrimaryType.Strength:
				strength = Mathf.Min(strength + defeatedLilGuy.strength / primaryModifier, MAX_STAT);
				defense = Mathf.Min(defense + defeatedLilGuy.defense / secondaryModifier, MAX_STAT);
				speed = Mathf.Min(speed + defeatedLilGuy.speed / secondaryModifier, MAX_STAT);
				break;
			case PrimaryType.Defense:
				strength = Mathf.Min(strength + defeatedLilGuy.strength / secondaryModifier, MAX_STAT);
				defense = Mathf.Min(defense + defeatedLilGuy.defense / primaryModifier, MAX_STAT);
				speed = Mathf.Min(speed + defeatedLilGuy.speed / secondaryModifier, MAX_STAT);
				break;
			case PrimaryType.Speed:
				strength = Mathf.Min(strength + defeatedLilGuy.strength / secondaryModifier, MAX_STAT);
				defense = Mathf.Min(defense + defeatedLilGuy.defense / secondaryModifier, MAX_STAT);
				speed = Mathf.Min(speed + defeatedLilGuy.speed / primaryModifier, MAX_STAT);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		Debug.Log("After: Str - " + strength + " Def - " + defense + " Spd - " + speed);

		maxHealth = 100 + (speed + defense + strength) / 10;
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