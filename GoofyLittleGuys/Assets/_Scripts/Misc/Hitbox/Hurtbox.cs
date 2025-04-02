using Managers;
using UnityEngine;
using UnityEngine.InputSystem;


//Auth: Thomas Berner
// - add as a component to detect any damage and keep track of health, keeps all combat collisions on a single layer
public class Hurtbox : MonoBehaviour
{
	[SerializeField] private GameObject owner;
	[SerializeField] private float health;

	private float timeSinceLastHit = 0;

	public float Health { get { return health; } }
	private PlayerBody lastHit;                      // Player who last hit this hurtbox.
	public PlayerBody LastHit { get { return lastHit; } set { lastHit = value; } }
	public GameObject Owner { get { return owner; } }


	private LilGuyBase lilGuy;
	private void Start()
	{
		EventManager.Instance.GameStarted += Init;
		lilGuy = GetComponent<LilGuyBase>();
	}
	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= Init;
	}

	private void Update()
	{
		if (lilGuy.PlayerOwner == null || (lilGuy.PlayerOwner != null && lilGuy.PlayerOwner.GameplayStats.SurvivedWithLowHP)) return;
		if (lilGuy.PlayerOwner != null && lilGuy.PlayerOwner.ActiveLilGuy == lilGuy)
		{
			if (lilGuy.Health < 0)
			{
				timeSinceLastHit = 0;
				return;
			}
			timeSinceLastHit += Time.deltaTime;
			if (timeSinceLastHit > 5 && lilGuy.Health < 10)
			{
				lilGuy.PlayerOwner.GameplayStats.SurvivedWithLowHP = true;
			}
		}
		else
		{
			timeSinceLastHit = 0;
		}

	}
	/// <summary>
	/// Method called when the game is started.
	/// </summary>
	private void Init(bool isTutorial = false)
	{
		PlayerBody body = owner.GetComponent<PlayerBody>();
		AiController controller = owner.GetComponent<AiController>();
		if (body != null)
		{
			health = body.LilGuyTeam[0].Health;
		}
		else if (controller != null)
		{
			health = controller.LilGuy.Health;
		}
	}

	/// <summary>
	/// Gets the health value of given object. To be called when a lil guy is damaged.
	/// </summary>
	/// <param name="dmg">The amount of damage dealt.</param>
	public void TakeDamage(float dmg, bool giveHapticFeedback = true)
	{
		if (gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
			timeSinceLastHit = 0;
			// Player lil guy was hit
			LilGuyBase playerLilGuy = owner.GetComponent<LilGuyBase>();
			if (playerLilGuy.PlayerOwner.HasImmunity) return;
			dmg *= Mathf.Max((1 - playerLilGuy.PlayerOwner.TeamDamageReduction), 1);
			dmg = Mathf.CeilToInt((float)(dmg * (1 - (playerLilGuy.Defense * 0.006))));
			playerLilGuy.Health -= dmg;
			health = playerLilGuy.Health;
			playerLilGuy.Damaged();
			playerLilGuy.PlayerOwner.GameplayStats.DamageTaken += dmg;

			if (giveHapticFeedback)
			{
				HapticEvent hapticEvent = GameManager.Instance.GetHapticEvent("Hurt");
				if (hapticEvent != null) HapticFeedback.PlayHapticFeedback(playerLilGuy.PlayerOwner.Controller.GetComponent<PlayerInput>(), hapticEvent.lowFrequency, hapticEvent.highFrequency, hapticEvent.duration);
			}

			//Passes the new health info to the player UI
			//Definitely needs to be rewritten for efficency
			EventManager.Instance.UpdatePlayerHealthUI(playerLilGuy.PlayerOwner);
		}
		else if (gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			AiController controller = owner.GetComponent<AiController>();
			if (controller.enabled)
			{
				float oldHealth = controller.LilGuy.Health; // Wild lil guy was hit
				dmg = Mathf.CeilToInt((float)(dmg * (1 - (controller.LilGuy.Defense * 0.006))));
				controller.LilGuy.Health =
					oldHealth - dmg >= 0
						? oldHealth - dmg
						: 0; // Set health to health - dmg if it's greater than or equal to 0, otherwise set it to 0 so it's non-negative.
				health = controller.LilGuy.Health;
				controller.LilGuy.Damaged();
				AiHealthUi enemyHealthUI = owner.GetComponentInChildren<AiHealthUi>();
				if (enemyHealthUI != null) enemyHealthUI.SetHealth(health, oldHealth);
			}
			else
			{
				TutorialAiController aiController = owner.GetComponent<TutorialAiController>();
				float oldHealth = aiController.LilGuy.Health; // Wild lil guy was hit
				dmg = Mathf.CeilToInt((float)(dmg * (1 - (aiController.LilGuy.Defense * 0.006))));
				aiController.LilGuy.Health =
					oldHealth - dmg >= 0
						? oldHealth - dmg
						: 0; // Set health to health - dmg if it's greater than or equal to 0, otherwise set it to 0 so it's non-negative.
				health = aiController.LilGuy.Health;
				aiController.LilGuy.Damaged();
				AiHealthUi enemyHealthUI = owner.GetComponentInChildren<AiHealthUi>();
				if (enemyHealthUI != null) enemyHealthUI.SetHealth(health, oldHealth);
			}
		}
		else
		{
			health -= dmg;
		}
	}
}
