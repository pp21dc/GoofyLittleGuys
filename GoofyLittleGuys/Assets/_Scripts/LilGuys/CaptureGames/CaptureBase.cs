using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureBase : MonoBehaviour
{
	// Put in base class

	// -- Variables --
	[SerializeField] protected PlayerInput player;
	[SerializeField] protected LilGuyBase lilGuyBeingCaught;
    [SerializeField] protected GameObject barrier;

    protected Vector2 areaBounds;
    protected GameObject instantiatedBarrier;

	private int maxTime = 10;
    private float time = 0.0f;
    private bool complete = false;
	protected bool gameActive = false;
	public virtual void Initialize(LilGuyBase creature)
	{
		lilGuyBeingCaught = creature;
        instantiatedBarrier = Instantiate(barrier, player.transform.position, Quaternion.identity);
		float radius = instantiatedBarrier.transform.lossyScale.x * 0.5f;
		areaBounds = new Vector2(radius, radius);
	}
	protected virtual void EndMinigame(bool playerWon)
	{
		if (playerWon)
		{
			PlayerBody body = player.GetComponent<PlayerBody>();
			Debug.Log("Player Won!");
            if (body.LilGuyTeam.Count < 3)
            {
                lilGuyBeingCaught.playerOwner = body.gameObject;
                lilGuyBeingCaught.health = lilGuyBeingCaught.maxHealth;
                body.LilGuyTeam.Add(lilGuyBeingCaught);
                lilGuyBeingCaught.Init(LayerMask.NameToLayer("PlayerLilGuys"));
                lilGuyBeingCaught.gameObject.transform.SetParent(body.LilGuyTeamSlots[body.LilGuyTeam.Count - 1].transform, false);
                lilGuyBeingCaught.gameObject.transform.localPosition = Vector3.zero;
                lilGuyBeingCaught.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }
            else
            {
                //Handle choosing which lil guy on the player's team will be replaced with this lil guy
            }
			// Add this lil guy to their team (if there's space)
			// Probably call some method on the player just to handle team management
			// In case they need to choose to remove a lil guy or something.
		}
		else
		{
			Debug.Log("Player lost!");
            // Lost... idk what happens :3
            Destroy(lilGuyBeingCaught.gameObject);
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