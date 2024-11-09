using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: Spawn Lil Guys within a given distance from this object,
// after a randomized interval (with given min and max value)
public class SpawnerObj : MonoBehaviour
{
    
    [SerializeField]
    private float spawnRadius;

    public LayerMask GroundLayer;

    /// <summary>
    /// This method accepts a Lil Guy (or any object) and spawns it
    /// at a random point within this spawner's radius.
    /// </summary>
    /// <param name="newLilGuy"></param>
    /// <returns></returns>
    public void SpawnLilGuy(GameObject newLilGuy)
    {
        //int failedAttempts = 0;
        Vector3 spawningPos = PickValidSpot();

        GameObject GO = Instantiate(newLilGuy, spawningPos, Quaternion.identity);
        GO.layer = LayerMask.NameToLayer("WildLilGuys");
        

        Managers.SpawnManager.Instance.currNumSpawns++;
    }

    /// <summary>
    /// Simply returns a random value to serve as the position within one axis
    /// (use multiple times to form a random position vector)
    /// </summary>
    ///  /// <param name="offset"></param> -> Should be the spawner's position in desired axis
    /// <returns>float</returns>
    public float RandPos(float offset)
    {
        return (Random.Range(-spawnRadius, spawnRadius)) + offset;
    }

	/// <summary>
	/// This me
	/// </summary>
	/// <returns>Vector3</returns>
	private Vector3 PickValidSpot()
	{
		const int maxAttempts = 10;
		const float startHeight = 1000f;  // Height to cast down from
		Vector3 origin = transform.position;

		for (int i = 0; i < maxAttempts; i++)
		{
			// Randomize a position within the spawn radius around the spawner
			Vector3 spawnPos = new Vector3(
				RandPos(origin.x),
				origin.y + startHeight,
				RandPos(origin.z)
			);

			// Raycast down from this high position
			Ray ray = new Ray(spawnPos, Vector3.down);
			if (Physics.Raycast(ray, out RaycastHit hit, startHeight * 2, GroundLayer))
			{
				// If it hits, adjust spawn position slightly above ground level
				spawnPos = hit.point;
				spawnPos.y += 2f;  // Offset to prevent clipping into ground
				return spawnPos;
			}
		}

		// If no valid spawn point found, log a warning and return a default position
		Debug.LogWarning("Failed to find a valid spawn position after max attempts.");
		return origin + Vector3.up * 2;  // Default to a position slightly above the spawner
	}



}
