using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureBase : MonoBehaviour
{
	// -- Variables --
	[SerializeField] protected PlayerInput player;
	[SerializeField] protected LilGuyBase lilGuyBeingCaught;
    [SerializeField] protected GameObject barrier;
	[SerializeField] protected float spawnRadius = 5f;			// Radius of the barrier area to check

	[SerializeField] private float groundCheckHeight = 10f;		// Height from which to start raycasting downward
	[SerializeField] private int maxIterations = 10;			// Limit iterations to prevent endless loops
	[SerializeField] private float maxHeightDifference = 1f;	// Maximum allowed height difference from spawn position
	[SerializeField] private LayerMask groundLayer;

	protected GameObject instantiatedBarrier;



	private int maxTime = 10;
    private float time = 0.0f;
    private bool complete = false;
	protected bool gameActive = false;


	/// <summary>
	/// Function to find a valid spawn position for the barrier
	/// </summary>
	/// <param name="initialPosition">The starting position that the barrier was going to spawn at.</param>
	/// <returns>The modified position, which doesn't intersect walls or hang off ledges.</returns>
	public Vector3 GetValidSpawnPosition(Vector3 initialPosition)
	{
		Vector3 spawnPosition = initialPosition;

		for (int i = 0; i < maxIterations; i++)
		{
			List<Vector3> offMapPoints = GetOffMapPoints(spawnPosition, spawnRadius);

			if (offMapPoints.Count == 0)
			{
				// All points are grounded and meet height criteria; position is valid
				return spawnPosition;
			}

			// Calculate the inland adjustment based on the average of off-map points
			Vector3 adjustment = Vector3.zero;
			foreach (var offMapPoint in offMapPoints)
			{
				adjustment += spawnPosition - offMapPoint; // Pulling inward
			}
			adjustment /= offMapPoints.Count; // Average adjustment
			spawnPosition += adjustment.normalized * 5f; // Move inward by a small step
		}

		// If we reach max iterations without finding a valid spot, return the last attempted position
		return spawnPosition;
	}

	/// <summary>
	/// Get points around the perimeter that are off-ground.
	/// </summary>
	/// <param name="position">The position we're checking if any point in the radius is offmap</param>
	/// <param name="radius">The barrier's radius</param>
	/// <returns>All points in the barrier radius that's off the map</returns>
	private List<Vector3> GetOffMapPoints(Vector3 position, float radius)
	{
		int numCheckPoints = 12; // Number of raycast points around the circle
		float angleStep = 360f / numCheckPoints;
		List<Vector3> offMapPoints = new List<Vector3>();

		for (int i = 0; i < numCheckPoints; i++)
		{
			// Calculate position around the perimeter of the circle
			float angle = i * angleStep * Mathf.Deg2Rad;
			Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
			Vector3 checkPoint = position + offset;

			// Raycast downwards to check if ground is present and assess height
			if (Physics.Raycast(checkPoint + Vector3.up * groundCheckHeight, Vector3.down, out RaycastHit hit, groundCheckHeight * 2, groundLayer))
			{
				// Check if the height difference is within acceptable bounds
				float heightDifference = Mathf.Abs(position.y - hit.point.y);
				if (heightDifference > maxHeightDifference)
				{
					Debug.Log(checkPoint + "near cliff");
					// Add to off-map points if the height difference is too large
					offMapPoints.Add(checkPoint);
				}
			}
			else
			{
				Debug.Log(checkPoint + "Off map");
				// Add to off-map points if there's no ground detected
				offMapPoints.Add(checkPoint);
			}
		}

		return offMapPoints;
	}

	/// <summary>
	/// Method called on capture game started.
	/// </summary>
	/// <param name="creature">The lil guy the player is trying to catch.</param>
	public virtual void Initialize(LilGuyBase creature)
	{
		lilGuyBeingCaught = creature;
		Vector3 validBarrierSpawnPos = GetValidSpawnPosition(player.transform.position);
        instantiatedBarrier = Instantiate(barrier, validBarrierSpawnPos, Quaternion.identity);
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
		SpawnManager.Instance.DespawnForest(lilGuyBeingCaught.gameObject);
		//SpawnManager.Instance.currNumSpawns--;
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