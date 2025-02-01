using UnityEngine;


//Auth: Thomas Berner
// - add as a component to detect any damage and keep track of health, keeps all combat collisions on a single layer
public class Hurtbox : MonoBehaviour
{
	[SerializeField] private float health;
	[SerializeField] private GameObject owner;
	private bool player;
	private bool Ai;

	public float Health { get { return health; } }
	private PlayerBody lastHit;                      // Player who last hit this hurtbox.
	public PlayerBody LastHit { get { return lastHit; } set { lastHit = value; } }
	public GameObject Owner { get { return owner; }}

	private void Start()
	{
		EventManager.Instance.GameStarted += Init;
	}
	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= Init;
	}

	/// <summary>
	/// Method called when the game is started.
	/// </summary>
	private void Init()
	{
		PlayerBody body = owner.GetComponent<PlayerBody>();
		AiController controller = owner.GetComponent<AiController>();
		if (body != null)
		{
			health = body.LilGuyTeam[0].Health;
			player = true;
			Ai = false;
		}
		else if (controller != null)
		{
			health = controller.LilGuy.Health;
			Ai = true;
			player = false;
		}
		else
		{
			player = false;
			Ai = false;
		}
	}

	/// <summary>
	/// Gets the health value of given object. To be called when a lil guy is damaged.
	/// </summary>
	/// <param name="dmg">The amount of damage dealt.</param>
	public void TakeDamage(float dmg)
	{
		if (gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
			// Player lil guy was hit
			LilGuyBase playerLilGuy = owner.GetComponent<LilGuyBase>();
			if (playerLilGuy.PlayerOwner.HasImmunity) return;
			dmg *= (1 - playerLilGuy.PlayerOwner.TeamDamageReduction);
			dmg = Mathf.CeilToInt((float)(dmg * (1 - (playerLilGuy.Defense * 0.006))));
			playerLilGuy.Health -= dmg;
			health = playerLilGuy.Health;
			playerLilGuy.Damaged();

			//Passes the new health info to the player UI
			//Definitely needs to be rewritten for efficency
			EventManager.Instance.UpdatePlayerHealthUI(playerLilGuy.PlayerOwner);
		}
		else if (gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			AiController controller = owner.GetComponent<AiController>();
			float oldHealth = controller.LilGuy.Health;         // Wild lil guy was hit
            dmg = Mathf.CeilToInt((float)(dmg * (1 - (controller.LilGuy.Defense * 0.006))));
            controller.LilGuy.Health = oldHealth - dmg >= 0 ? oldHealth - dmg : 0;  // Set health to health - dmg if it's greater than or equal to 0, otherwise set it to 0 so it's non-negative.
			health = controller.LilGuy.Health;
			controller.LilGuy.Damaged();
			AiHealthUi enemyHealthUI = owner.GetComponentInChildren<AiHealthUi>();
			if (enemyHealthUI != null)enemyHealthUI.SetHealth(health, oldHealth);
		}
		else
		{
			health -= dmg;
		}
	}
}
