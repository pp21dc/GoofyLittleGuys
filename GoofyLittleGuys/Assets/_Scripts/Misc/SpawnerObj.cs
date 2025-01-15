using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

// Purpose: Spawn Lil Guys within a given distance from this object,
// after a randomized interval (with given min and max value)
public class SpawnerObj : MonoBehaviour
{
    
    [ReadOnly]
    private float spawnRadius;

	public bool startedSpawns = false; // whether or not to start spawning
	private bool isSpawning = false; // if we have a coroutine going currently for spawning

    public LayerMask GroundLayer;

	[SerializeField] private List<GameObject> campLilGuys;
	[SerializeField] private GameObject legendaryLilGuy;
	[SerializeField] private int maxSpawnCount;
	[SerializeField] private int spawnDelay;
	[SerializeField] public int currSpawnCount;
	[SerializeField] public bool legendarySpawned = false;

	private bool isRespawning = false;
	public float SpawnRadius => spawnRadius;
	public int SpawnDelay => spawnDelay;

	private void Start()
	{
		spawnRadius = GetComponent<SphereCollider>().radius;
	}

    private void Update()
    {
        if(startedSpawns && currSpawnCount < maxSpawnCount)
        {
			StartSpawning();
        }
    }

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

        GameObject GO = Instantiate(newLilGuy, spawningPos, Quaternion.identity, Managers.SpawnManager.Instance.transform);
        GO.layer = LayerMask.NameToLayer("WildLilGuys");
		GO.GetComponent<LilGuyBase>().DetermineLevel();
		GO.GetComponent<WildBehaviour>().HomeSpawner = this;
        

        Managers.SpawnManager.Instance.currNumSpawns++;
    }

	public void SpawnLegendary()
	{//int failedAttempts = 0;
		Vector3 spawningPos = PickValidSpot();

		GameObject GO = Instantiate(legendaryLilGuy, spawningPos, Quaternion.identity, Managers.SpawnManager.Instance.transform);
		GO.layer = LayerMask.NameToLayer("WildLilGuys");

		GO.GetComponent<WildBehaviour>().HomeSpawner = this;


		Managers.SpawnManager.Instance.currNumSpawns++;
	}

	/// <summary>
	/// Spawns a random Lil Guy from this camp/spawner's list of Lil Guys
	/// </summary>
	public void SpawnRandLilGuy()
    {
		GameObject randLilGuy = RandFromList(campLilGuys);
		if(currSpawnCount < maxSpawnCount)
        {
			SpawnLilGuy(randLilGuy);
			currSpawnCount++;
		}
    }

	/// <summary>
	/// Simply checks if we aren't already trying to spawn a Lil Guy, then spawns one if not.
	/// </summary>
	public void StartSpawning()
    {
        if (!isSpawning && !isRespawning)
        {
			StartCoroutine(DelayedSpawn());
		}
    }

	/// <summary>
	/// Just waits for a few seconds then spawns a random Lil Guy.
	/// </summary>
	/// <returns></returns>
	public IEnumerator DelayedSpawn()
    {
		isSpawning = true;
		//SpawnRandLilGuy();
		yield return new WaitForSeconds(spawnDelay);
		SpawnRandLilGuy();
		isSpawning = false;
        if (isRespawning)
        {
			isRespawning = false;
		}
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
	/// This method picks a valid spawning position
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

	/// <summary>
	/// This method accepts a list of objects and returns a random one from that list.
	/// </summary>
	/// <param name="theList"></param>
	/// <returns> GameObject </returns>
	public GameObject RandFromList(List<GameObject> theList)
	{
		return theList[Random.Range(0, theList.Count)];
	}

	public void RemoveLilGuyFromSpawns()
	{
		currSpawnCount--;
		isRespawning = true;
		StartCoroutine(DelayedSpawn());
	}

}
