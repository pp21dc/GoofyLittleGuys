using System;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public abstract class LilGuyBase : MonoBehaviour
{
	//VARIABLES//
	[Header("Lil Guy Information")]
	public string guyName;
	public PrimaryType type;
	public GameObject mesh;
	[SerializeField] private GameObject hitboxPrefab;
	[SerializeField] private AnimatorOverrideController animatorController;
	public Transform attackPosition;

	[Header("Lil Guy Stats")]
	public int health;
	public int maxHealth;
	public int speed;
	public int defense;
	public int strength;
	public const int MAX_STAT = 100;
	private int average;

	[Header("Special Attack Specific")]
	[SerializeField] protected int currentCharges = 1;
	[SerializeField] protected int maxCharges = 1;
	[SerializeField] protected float cooldownDuration = 1;
	[SerializeField] protected float chargeRefreshRate = 1;
	public GameObject playerOwner = null;
	protected float cooldownTimer = 0;
	protected float chargeTimer = 0;

	private bool isHurt = false;
	private bool isDead = false;
	private GameObject instantiatedHitbox;

	public enum PrimaryType
	{
		Strength,
		Defense,
		Speed
	}


	/// <summary>
	/// Method to be called on lil guy instantiation.
	/// </summary>
	/// <param name="layer">The layer that this lil guy should be on.</param>
	public void Init(LayerMask layer)
	{
		gameObject.layer = layer;
	}

	private void Update()
	{
		if (playerOwner != null && GetComponent<AiController>() != null)
		{
			// This is a player owned lil guy, so remove their AI behaviours and reset mesh rotations.
			mesh.transform.localRotation = Quaternion.Euler(Vector3.zero);
			Destroy(GetComponent<AiController>());
		}

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
	}

	/// <summary>
	/// This is the basic attack across all lil guys\
	/// it uses a hitbox prefab to detect other ai within it and deal damage from that script
	/// NOTE: the second value in destroy (line 29) is the duration that the attack lasts
	/// </summary>
	public void Attack()
	{
		if (instantiatedHitbox == null)
		{
			instantiatedHitbox = Instantiate(hitboxPrefab, attackPosition.position - attackPosition.right * 0.5f, Quaternion.identity);
			instantiatedHitbox.transform.SetParent(transform);

			instantiatedHitbox.GetComponent<Hitbox>().layerMask = playerOwner != null ? playerOwner.layer : gameObject.layer;	// Set hitbox to player layer if player-owned, otherwise, set it to this lil guy's layer.
			instantiatedHitbox.GetComponent<Hitbox>().Init(gameObject);
			Destroy(instantiatedHitbox, 1f);
		}
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