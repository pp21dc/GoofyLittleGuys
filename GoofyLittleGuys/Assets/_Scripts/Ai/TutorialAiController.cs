using Managers;
using UnityEngine;

public class TutorialAiController : MonoBehaviour
{
	//purpose: pretty much same thing as AiController, just fitted to use tutorialBehaviour instead of wildBehaviour

	public enum AIState { Wild, Tamed }

	#region Public Variables & Serialize Fields
	[SerializeField] private LayerMask groundLayer;
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private GameObject interactCanvas;
	[ColoredGroup][SerializeField] private AiHealthUi healthBars;
	#endregion

	#region Private Variables
	private LilGuyBase lilGuy;                          // Reference to this AI's stats
	private AIState state = AIState.Wild;               // The current state of the AI (either wild or one caught by a player)
	private TutorialBehaviour wildBehaviour;                // Defines Wild AI behaviour (Idle, Chase, Attack, Death)
	private TamedBehaviour tamedBehaviour;              // Defines Tamed AI behaviour (Follow Player)

	private CanvasGroup healthUi;

	private Vector3 originalSpawnPosition = Vector3.zero;
	private Transform followPosition;                           // The transform of the closest player to this AI
	private Transform currClosestPlayer;
	#endregion

	#region Getters & Setters
	public AIState State => state;
	public AiHealthUi HealthBars => healthBars;
	public Transform FollowPosition => followPosition;
	public LilGuyBase LilGuy => lilGuy;
	public CanvasGroup HealthUi => healthUi;
	public Vector3 OriginalSpawnPosition => originalSpawnPosition;
	#endregion


	private void Awake()
    {
        wildBehaviour = GetComponent<TutorialBehaviour>();
        tamedBehaviour = GetComponent<TamedBehaviour>();
    }

    private void Start()
    {
        lilGuy = GetComponent<LilGuyBase>();
        healthUi = GetComponentInChildren<CanvasGroup>();

        SetSpawnPosition(transform.position);
        UpdateState();
    }

    private void Update()
    {
        if (state == AIState.Wild)
        {
            if (GameManager.Instance.Players.Count > 0)
            {
                followPosition = FindClosestPlayer();
                if (healthUi != null && healthUi.isActiveAndEnabled)
                {
                    if (DistanceToPlayer() > 20)
                    {
                        healthUi.alpha = 0;
                        healthBars.YellowBar.gameObject.SetActive(false);
                    }
                    else if (DistanceToPlayer() <= 10)
                    {
                        healthUi.alpha = 1;
                        healthBars.YellowBar.gameObject.SetActive(true);
                    }
                    else
                    {
                        healthUi.alpha = (20 - DistanceToPlayer()) / 10; // Smoothly transition alpha from 1 to 0
                        healthBars.YellowBar.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (GameManager.Instance.Players.Count > 0)
                followPosition = lilGuy.GoalPosition;
        }
    }

    /// <summary>
    /// Method that toggles the interact canvas when a player is nearby.
    /// </summary>
    public void ToggleInteractCanvas(bool visible)
    {
        interactCanvas.SetActive(visible);
    }

    /// <summary>
    /// Method that changes the state of this AI behaviour to the provided state.
    /// </summary>
    /// <param name="state">The state this AI should be put into.</param>
    public void SetState(AIState state)
    {
        this.state = state;
        UpdateState();
    }

    /// <summary>
    /// Helper method that returns the distance this AI is from the player.
    /// </summary>
    /// <returns>The distance this AI is to the player.</returns>
    public float DistanceToPlayer()
    {
        if (GameManager.Instance.Players.Count > 0 && followPosition != null)
            return Vector3.Distance(transform.position, followPosition.position);
        else return 0;
    }

    /// <summary>
    /// Method that sets the spawn point of this AI to the given position, grounded to the closest ground layer.
    /// </summary>
    /// <param name="spawnPos">The original spawn position of the lil guy.</param>
    private void SetSpawnPosition(Vector3 spawnPos)
    {
        Ray ray = new Ray(spawnPos, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2000, groundLayer))
        {
            // If it hits, adjust spawn position slightly above ground level
            originalSpawnPosition = hit.point + Vector3.up;
        }
    }

    /// <summary>
    /// Method that updates the behaviours based on the current state of the AI.
    /// </summary>
    private void UpdateState()
    {
        wildBehaviour.enabled = (state == AIState.Wild);
        tamedBehaviour.enabled = (state == AIState.Tamed);
    }

    /// <summary>
    /// Method that find the closest player.
    /// </summary>
    /// <returns></returns>
    private Transform FindClosestPlayer()
    {
        // Return null if there are no players
        if (GameManager.Instance.Players.Count <= 0) return null;

        // Initialize variables
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue; // Start with the maximum possible distance

        // Iterate through all players to find the closest living one
        foreach (PlayerBody body in GameManager.Instance.Players)
        {
            if (!body.IsDead) // Skip dead players
            {
                float distance = Vector3.Distance(body.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = body.transform;
                }
            }
        }

        return closestPlayer; // Return the closest living player (or null if none found)
    }
}