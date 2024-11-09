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
    public GameObject lastHit;

	private void Start()
	{
        EventManager.Instance.GameStarted += Init;
	}
	private void OnDestroy()
	{
        EventManager.Instance.GameStarted -= Init;
	}

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
	public void TakeDamage(int dmg)
    {
        if (gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
        {
			if (owner.GetComponent<LilGuyBase>().playerOwner.GetComponent<PlayerBody>().InMinigame)
			{
				owner.GetComponent<LilGuyBase>().health -= dmg;
				health = owner.GetComponent<LilGuyBase>().health -= dmg;
				owner.GetComponent<LilGuyBase>().Damaged();
			}
        }
        else if (gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
        {
			owner.GetComponent<AiController>().LilGuy.health -= dmg;
            health = owner.GetComponent<AiController>().LilGuy.health;
            owner.GetComponent<LilGuyBase>().Damaged();
        }
        else
        {
            health -= dmg;
        }
    }

    //
}
