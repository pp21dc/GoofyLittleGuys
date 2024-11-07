using System;
using System.Collections;
using UnityEngine;

public abstract class LilGuyBase : MonoBehaviour
{
    //VARIABLES//
    [Header("Lil Guy Information")]
	public string guyName;
	public PrimaryType type;
	[SerializeField] private GameObject hitboxPrefab;
	[SerializeField] private AnimatorOverrideController animatorController;

	[Header("Lil Guy Stats")]
    public int health;
    public int maxHealth;
    public int speed;
    public int defense;
    public int strength;
	public const int MAX_STAT = 100;
	private int average;
	private Transform attackPosition;

    [Header("Special Attack Specific")]
	[SerializeField] protected int currentCharges = 1;
	[SerializeField] protected int maxCharges = 1;
	[SerializeField] protected float cooldownDuration = 1;
    public GameObject playerOwner = null;
	protected float chargeRefreshRate = 1;
	protected float cooldownTimer = 0;
	protected float chargeTimer = 0;

    private bool isHurt = false;
    private bool isDead = false;

	public enum PrimaryType
    {
        Strength,
        Defense,
        Speed
    }

    private void Awake()
    {
        attackPosition = gameObject.transform;
    }

	private void Update()
	{
        if (playerOwner != null && GetComponent<AiController>().enabled)
        {
            GetComponent<AiController>().enabled = false;
            GetComponent<Hurtbox>().enabled = false;
        }
		if (isDead) return;
        // Special attack cooldown 
		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}

        // charges regeneration
		if (chargeTimer > 0)
		{
			chargeTimer -= Time.deltaTime;
		}
		if (currentCharges < maxCharges && chargeTimer <= 0)
		{
			currentCharges++;
			chargeTimer = chargeRefreshRate;
		}

	}
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

    public void Damaged()
    {
        StartCoroutine(FlashRed());
    }
	/// <summary>
	/// This is the basic attack across all lil guys\
	/// it uses a hitbox prefab to detect other ai within it and deal damage from that script
	/// NOTE: the second value in destroy (line 29) is the duration that the attack lasts
	/// </summary>
	public void Attack()
    {
        GameObject hitbox = Instantiate(hitboxPrefab, attackPosition.position + attackPosition.forward * 0.5f, Quaternion.identity);
        hitbox.transform.SetParent(transform);
        hitbox.GetComponent<Hitbox>().layerMask = playerOwner != null ? playerOwner.layer : gameObject.layer;
        hitbox.GetComponent<Hitbox>().Init(gameObject);
        Destroy(hitbox, 1f);
    }

    /// <summary>
    /// override this function in all derivitives of this class with its unique special attack
    /// </summary>
    public virtual void Special() { throw new NotImplementedException(); }

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

    public GameObject GetHitboxPrefab()
    {
        return hitboxPrefab;
    }

    public Transform GetAttackPosition()
    {
        return attackPosition;
    }
}