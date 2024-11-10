using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureBase : MonoBehaviour
{
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

    /// <summary>
    /// Method called on capture game started.
    /// </summary>
    /// <param name="creature">The lil guy the player is trying to catch.</param>
	public virtual void Initialize(LilGuyBase creature)
	{
		lilGuyBeingCaught = creature;
        instantiatedBarrier = Instantiate(barrier, player.transform.position, Quaternion.identity);

        // Defining play area for the minigame.
		float radius = instantiatedBarrier.transform.localScale.x * 0.5f;
		areaBounds = new Vector2(radius, radius);
	}

    /// <summary>
    /// Method called when a minigame is completed.
    /// </summary>
    /// <param name="playerWon">Did the player win or lose the minigame?</param>
	protected virtual void EndMinigame(bool playerWon)
	{
		PlayerBody body = player.GetComponent<PlayerBody>();

		if (playerWon)
		{
            // Player won
            if (body.LilGuyTeam.Count < 3)
			{

				// There is room on the player's team for this lil guy.
				// Set player owner to this player, and reset the lil guy's health to full, before adding to the player's party.

				player.SwitchCurrentActionMap("World");             // Switch back to world action map
				lilGuyBeingCaught.playerOwner = body.gameObject;
                lilGuyBeingCaught.health = lilGuyBeingCaught.maxHealth;
                body.LilGuyTeam.Add(lilGuyBeingCaught);

                // Setting layer to Player Lil Guys, and putting the lil guy into the first empty slot available.
                lilGuyBeingCaught.Init(LayerMask.NameToLayer("PlayerLilGuys"));
                lilGuyBeingCaught.gameObject.transform.SetParent(body.LilGuyTeamSlots[body.LilGuyTeam.Count - 1].transform, false);
                lilGuyBeingCaught.gameObject.transform.localPosition = Vector3.zero;
                lilGuyBeingCaught.gameObject.GetComponent<Rigidbody>().isKinematic = true;

				body.InMinigame = false;

			}
            else
            {
				//Handle choosing which lil guy on the player's team will be replaced with this lil guy

				player.GetComponent<PlayerBody>().TeamFullMenu.SetActive(true);
				player.GetComponent<PlayerBody>().TeamFullMenu.GetComponent<TeamFullMenu>().Init(lilGuyBeingCaught);
			}
		}
		else
		{
			// Player lost
            Destroy(lilGuyBeingCaught.gameObject);
			player.SwitchCurrentActionMap("World");             // Switch back to world action map
			body.InMinigame = false;
		}

		LeaveMinigame(body);
	}    

	/// <summary>
	/// Method that handles configuration to officially end the minigame.
	/// </summary>
	public void LeaveMinigame(PlayerBody body)
	{
		gameObject.SetActive(false);
		Destroy(instantiatedBarrier);
	}
}