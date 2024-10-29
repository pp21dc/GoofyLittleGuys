using UnityEngine;


//Auth: Thomas Berner
// - add as a component to detect any damage and keep track of health, keeps all combat collisions on a single layer
public class Hurtbox : MonoBehaviour
{
    [SerializeField] private int health;
    private bool player;
    private bool Ai;

    public int Health { get { return health; } }
    public GameObject lastHit;

    private void Awake()
    {
        if (gameObject.GetComponentInParent<PlayerBody>() != null)
        {
            health = gameObject.GetComponentInParent<PlayerBody>().LilGuyTeam[0].health;
            player = true;
            Ai = false;
        }
        else if (gameObject.GetComponentInParent<AiController>() != null)
        {
            health = gameObject.GetComponentInParent<AiController>().LilGuy.health;
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
        if (player)
        {
            gameObject.GetComponentInParent<PlayerBody>().LilGuyTeam[0].health -= dmg;
            health = gameObject.GetComponentInParent<PlayerBody>().LilGuyTeam[0].health;
        }
        else if (Ai)
        {
            gameObject.GetComponent<AiController>().LilGuy.health -= dmg;
            health = gameObject.GetComponent<AiController>().LilGuy.health;
        }
        else
        {
            health -= dmg;
        }
    }

    //
}
