using UnityEngine;


//Auth: Thomas Berner
// - add as a component to detect any damage and keep track of health, keeps all combat collisions on a single layer
public class Hurtbox : MonoBehaviour
{
    [SerializeField] private int health;
    private bool lilGuy;

    private void Awake()
    {
        if (gameObject.GetComponentInParent<PlayerBody>() != null)
        {
            health = gameObject.GetComponentInParent<PlayerBody>().;
            lilGuy = true;
        }
        else
        {
            lilGuy = false;
        }
    }

    /// <summary>
    /// Gets the health value of given object. To be called when a lil guy is damaged.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        if (lilGuy)
        {
            gameObject.GetComponent<LilGuyBase>().health -= dmg;
            health = gameObject.GetComponent<LilGuyBase>().health;
        }
        else
        {
            gameObject.GetComponent<Hurtbox>().health -= dmg;
        }
    }

    //
}
