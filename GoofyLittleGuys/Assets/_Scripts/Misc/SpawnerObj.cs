using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

/// <summary>
/// Handles spawning and managing Lil Guys in a specific area.
/// Ensures limits per spawner and per clearing while allowing legendary spawns unrestricted.
/// </summary>
public class SpawnerObj : MonoBehaviour
{
	[SerializeField] private List<GameObject> campLilGuys; // List of possible Lil Guys to spawn
	[SerializeField] private GameObject legendaryLilGuy; // Legendary Lil Guy prefab
	[SerializeField] private int maxSpawnCount = 3; // Max Lil Guys per spawner (updated from 2 to 3)
	[SerializeField] private int spawnDelay = 5; // Delay between spawns
	[SerializeField] public bool legendarySpawned = false; // If legendary Lil Guy has spawned

	private int currSpawnCount = 0; // Current count of spawned Lil Guys
	private bool isSpawning = false; // If a spawn is currently in progress
	private bool initialSpawnsSpawned = false; // If a spawn is currently in progress
	private SpawnManager spawnManager; // Reference to the SpawnManager
	private SphereCollider spawnArea; // Collider representing the spawn area
	private string clearingID; // ID representing the clearing this spawner belongs to
	private ShuffleBag<GameObject> lilGuyShuffleBag; // Shuffle Bag to random select lil Guys, but ensure each in the list appears once before repeats occur.


	public LayerMask GroundLayer; // Layer mask for valid ground placement

	public int CurrentSpawnCount => currSpawnCount;

	public SphereCollider SpawnArea => spawnArea;

	private void Start()
	{
		initialSpawnsSpawned = false;
		lilGuyShuffleBag = new ShuffleBag<GameObject>(campLilGuys);
		spawnArea = GetComponent<SphereCollider>();
		spawnManager = SpawnManager.Instance;
		clearingID = transform.parent.name; // Assuming parent represents the clearing
		spawnManager.RegisterSpawner(this, clearingID);
	}

	public void RequestSpawn()
	{
		if (currSpawnCount >= maxSpawnCount) return; // Still respects per-spawner cap

		GameObject randLilGuy = lilGuyShuffleBag.Next();
		SpawnLilGuy(randLilGuy);
	}


	/// <summary>
	/// Spawns a random Lil Guy from the available list.
	/// </summary>
	public void SpawnRandLilGuy()
	{
		if (currSpawnCount >= maxSpawnCount || !spawnManager.CanSpawnMore(clearingID) || !initialSpawnsSpawned)
		{
			return;
		}
		GameObject randLilGuy = lilGuyShuffleBag.Next();
		SpawnLilGuy(randLilGuy);
	}


	/// <summary>
	/// Instantiates a specific Lil Guy at a valid spawn position.
	/// </summary>
	private void SpawnLilGuy(GameObject lilGuyPrefab)
	{
		Vector3 spawnPos = PickValidSpot();
		GameObject lilGuy = Instantiate(lilGuyPrefab, spawnPos, Quaternion.identity, Managers.SpawnManager.Instance.transform);
		lilGuy.layer = LayerMask.NameToLayer("WildLilGuys");
		lilGuy.GetComponent<WildBehaviour>().HomeSpawner = this;
		currSpawnCount++;
		spawnManager.RegisterSpawn(clearingID);
		lilGuy.GetComponent<LilGuyBase>().DetermineLevel();
	}

	/// <summary>
	/// Spawns the legendary Lil Guy, ignoring normal spawn restrictions.
	/// </summary>
	public void SpawnLegendary(float maxScale, int level)
	{
		Vector3 spawnPos = PickValidSpot();
		GameObject legendary = Instantiate(legendaryLilGuy, spawnPos, Quaternion.identity, Managers.SpawnManager.Instance.transform);
		legendary.layer = LayerMask.NameToLayer("WildLilGuys");
		legendary.GetComponent<WildBehaviour>().HomeSpawner = this;

		LilGuyBase leg = legendary.GetComponent<LilGuyBase>();
		leg.CreateLegendary(maxScale, level);
	}

	/// <summary>
	/// Removes a Lil Guy from this spawner's count and triggers a respawn attempt.
	/// </summary>
	public void RemoveLilGuyFromSpawns()
	{
		currSpawnCount--;
		spawnManager.DeregisterSpawn(clearingID);
	}

	/// <summary>
	/// Finds a valid ground position within the spawner's radius for spawning.
	/// </summary>
	private Vector3 PickValidSpot()
	{
		Vector3 origin = transform.position + spawnArea.center;
		const float startHeight = 1000f;

		for (int i = 0; i < 10; i++)
		{
			Vector3 spawnPos = new Vector3(
				Random.Range(origin.x - spawnArea.radius, origin.x + spawnArea.radius),
				origin.y + startHeight,
				Random.Range(origin.z - spawnArea.radius, origin.z + spawnArea.radius)
			);

			if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, startHeight * 2, GroundLayer))
			{
				spawnPos = hit.point + Vector3.up * 2f;
				return spawnPos;
			}
		}
		return origin + Vector3.up * 2;
	}
}
