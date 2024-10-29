using System;
using UnityEngine;

public abstract class LilGuyBase : MonoBehaviour
{
    //VARIABLES//
    public string guyName;
    public int health;
    public int maxHealth;
    public PrimaryType type;
    public int speed;
    public int defense;
    public int strength;
    private int average;
    public const int MAX_STAT = 100;
    private Transform attackPosition;
	protected float cooldownTimer = 0;
	protected float cooldownDuration = 1;
	protected float chargeRefreshRate = 1;
	protected float chargeTimer = 0;

	[SerializeField] private GameObject hitboxPrefab;
	[SerializeField] protected int currentCharges = 1;
	[SerializeField] protected int maxCharges = 1;

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
		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}
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

	/// <summary>
	/// This is the basic attack across all lil guys\
	/// it uses a hitbox prefab to detect other ai within it and deal damage from that script
	/// NOTE: the second value in destroy (line 29) is the duration that the attack lasts
	/// </summary>
	public void Attack()
    {
        GameObject hitbox = Instantiate(hitboxPrefab, attackPosition.position + attackPosition.forward * 0.5f, Quaternion.identity);
        hitbox.transform.SetParent(transform);
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