using UnityEngine;


//Auth: Thomas Berner
// - add as a component to detect any damage and keep track of health, keeps all combat collisions on a single layer
public class Hurtbox : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private GameObject owner;
    private bool player;
    private bool Ai;

    public int Health { get { return health; } }
    public GameObject lastHit;						// Player who last hit this hurtbox.

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
		if (owner.GetComponent<PlayerBody>() != null)
		{
			health = owner.GetComponent<PlayerBody>().LilGuyTeam[0].health;
			player = true;
			Ai = false;
		}
		else if (owner.GetComponent<AiController>() != null)
		{
			health = owner.GetComponent<AiController>().LilGuy.health;
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
	public void TakeDamage(int dmg)
	{
		if (gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
			// Player lil guy was hit
			if (!owner.GetComponent<LilGuyBase>().playerOwner.GetComponent<PlayerBody>().InMinigame)
			{
				// If the player lil guy that was hit is currently not in a minigame, apply damage to their health.
				owner.GetComponent<LilGuyBase>().health -= dmg;
				health = owner.GetComponent<LilGuyBase>().health -= dmg;
				owner.GetComponent<LilGuyBase>().Damaged();

				//Passes the new health info to the player UI
				//Definitely needs to be rewritten for efficency
				owner.GetComponentInParent<Searchlight>().playerUi.SetPersistentHealthBarValue(health, owner.GetComponent<LilGuyBase>().maxHealth);
            }
		}
		else if (gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			int oldHealth = owner.GetComponent<AiController>().LilGuy.health;			// Wild lil guy was hit
			
			owner.GetComponent<AiController>().LilGuy.health = oldHealth - dmg >= 0 ? oldHealth - dmg : 0;	// Set health to health - dmg if it's greater than or equal to 0, otherwise set it to 0 so it's non-negative.
			health = owner.GetComponent<AiController>().LilGuy.health;
			owner.GetComponent<LilGuyBase>().Damaged();
			owner.GetComponentInChildren<AiHealthUi>().SetHealth(health, oldHealth);
		}
		else
		{
			health -= dmg;
		}
	}
}
