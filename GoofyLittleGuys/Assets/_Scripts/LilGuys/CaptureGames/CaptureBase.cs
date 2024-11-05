using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureBase : MonoBehaviour
{
	// Put in base class

	// -- Variables --
	[SerializeField] protected PlayerInput player;
	[SerializeField] protected LilGuyBase lilGuyBeingCaught;
    [SerializeField] private GameObject barrier;

	private int maxTime = 10;
    private float time = 0.0f;
    private bool complete = false;
	protected bool gameActive = false;
	public virtual void Initialize(LilGuyBase creature)
	{
		lilGuyBeingCaught = creature;
	}
	protected virtual void EndMinigame(bool playerWon)
	{
		if (playerWon)
		{
			Debug.Log("Player Won!");
			// Add this lil guy to their team (if there's space)
			// Probably call some method on the player just to handle team management
			// In case they need to choose to remove a lil guy or something.
		}
		else
		{
			Debug.Log("Player lost!");
			// Lost... idk what happens :3
		}

		gameObject.SetActive(false);
	}
	// Update is called once per frame
	void Update()
    {
        if (complete)
        {
            //need ref for player passed along from capture menu
            CaptureLilGuy();
            //play anim
            //remove minigame
        }
        else
        {
            //need ui for timer on screen
            time += Time.deltaTime;
            if (time >= maxTime)
            {
                LostMinigame();
            }
        }
    }

    private void CaptureLilGuy()
    {
        //add lil guy to team of player
    }

    private void LostMinigame()
    {
        //send fail message on Ui?
        //play escape animation
        Destroy(this.gameObject);
    }
    
}