using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AiController : MonoBehaviour
{
	public enum AIState { Wild, Tamed }
	private AIState state = AIState.Wild;               // The current state of the AI (either wild or one caught by a player)
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private GameObject interactCanvas;

	private WildBehaviour wildBehaviour;                // Defines Wild AI behaviour (Idle, Chase, Attack, Death)
	private TamedBehaviour tamedBehaviour;              // Defines Tamed AI behaviour (Follow Player)
	private Rigidbody rb;



	private Vector3 originalSpawnPosition = Vector3.zero;
	private Transform player;                           // The transform of the closest player to this AI
	private LilGuyBase lilGuy;                          // Reference to this AI's stats
	public Transform Player => player;
	public LilGuyBase LilGuy => lilGuy;
	public Rigidbody RB => rb;
	public Vector3 OriginalSpawnPosition => originalSpawnPosition;

	private void Awake()
	{
		wildBehaviour = GetComponent<WildBehaviour>();
		tamedBehaviour = GetComponent<TamedBehaviour>();
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		lilGuy = GetComponent<LilGuyBase>();

		SetSpawnPosition(transform.position);
		UpdateState();
	}

	private void Update()
	{
		if (state == AIState.Wild)
		{
			if (PlayerInput.all.Count > 0)
				player = FindClosestPlayer();
		}
		else
		{
			if (PlayerInput.all.Count > 0)
				player = lilGuy.GoalPosition;
		}
	}

	/// <summary>
	/// Method that toggles the interact canvas when a player is nearby.
	/// </summary>
	public void ToggleInteractCanvas(bool visible)
	{
		interactCanvas.SetActive(visible);
		if (visible) player.GetComponent<PlayerBody>().ClosestWildLilGuy = lilGuy;
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
		if (PlayerInput.all.Count > 0)
			return Vector3.Distance(transform.position, player.position);
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
		if (PlayerInput.all.Count <= 0) return null;
		Transform currClosest = PlayerInput.all[0].GetComponent<PlayerController>().Body.transform;
		foreach (PlayerInput input in PlayerInput.all)
		{
			if (Vector3.Distance(input.transform.position, transform.position) < Vector3.Distance(currClosest.transform.position, transform.position))
			{
				currClosest = input.transform;
			}
		}
		return currClosest;
	}
}
