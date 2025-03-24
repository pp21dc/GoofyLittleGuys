using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;



namespace Managers
{
	/// <summary>
	/// Manages all spawners and enforces spawn limits across different clearings.
	/// Ensures that each clearing has a maximum of 3 Lil Guys per spawner, while also ensuring
	/// that each spawner in the clearing has spawned at least one Lil Guy.
	/// Legendary spawners are exempt from these limits.
	/// </summary>
	public class SpawnManager : SingletonBase<SpawnManager>
	{
		[SerializeField] private float gracePeriod = 5f;
		[SerializeField] private int maxLilGuysPerClearing = 3; // Default limit for wild Lil Guys per clearing

		private Dictionary<string, List<SpawnerObj>> clearingSpawners = new();  // Tracks spawners per clearing
		private Dictionary<string, int> clearingSpawnCounts = new();            // Tracks active Lil Guys per clearing

		private ShuffleBag<SpawnerObj> legendarySpawnersSB = new();
		private List<SpawnerObj> legendarySpawners = new();                     // Tracks all legendary spawners

		private Coroutine spawnLoop;
		private float spawnInterval = 3f; // Time between spawn attempts

		public float GracePeriod { get => gracePeriod; set => gracePeriod = value; }
		public int MaxLilGuysPerClearing { get => maxLilGuysPerClearing; set => maxLilGuysPerClearing = value; }

		private void Start()
		{
			StartCoroutine(InitializeSpawners());
		}

		private IEnumerator InitializeSpawners()
		{
			yield return new WaitForSecondsRealtime(gracePeriod); // Wait for grace period

			// Ensure all spawners get at least one spawn
			foreach (var clearing in clearingSpawners.Keys)
			{
				foreach (var spawner in clearingSpawners[clearing])
				{
					spawner.RequestSpawn();
				}
			}

			// Start normal spawn loop after initial spawns
			spawnLoop = StartCoroutine(SpawnLoop());
		}

		private IEnumerator SpawnLoop()
		{
			while (true)
			{
				yield return new WaitForSeconds(spawnInterval);
				AttemptSpawnInAllClearings();
			}
		}

		private void AttemptSpawnInAllClearings()
		{
			foreach (var clearingID in clearingSpawners.Keys)
			{
				if (!CanSpawnMore(clearingID)) continue; // Respect global clearing limit

				List<SpawnerObj> spawners = clearingSpawners[clearingID];

				SpawnerObj selectedSpawner = spawners[Random.Range(0, spawners.Count)];
				selectedSpawner.RequestSpawn(); // Only the chosen spawner spawns
			}
		}


		/// <summary>
		/// Registers a spawner to the appropriate clearing, or as a legendary spawner if applicable.
		/// Ensures at least one Lil Guy is spawned per spawner in a clearing.
		/// </summary>
		public void RegisterSpawner(SpawnerObj spawner, string clearingID)
		{
			if (spawner.legendarySpawned)
			{
				legendarySpawners.Add(spawner);
				legendarySpawnersSB.Add(spawner);
			}

			if (!clearingSpawners.ContainsKey(clearingID))
			{
				clearingSpawners[clearingID] = new List<SpawnerObj>();
				clearingSpawnCounts[clearingID] = 0;
			}

			clearingSpawners[clearingID].Add(spawner);
		}

		/// <summary>
		/// Checks if a clearing can spawn more Lil Guys, ensuring it does not exceed the cap of 3 per spawner.
		/// </summary>
		public bool CanSpawnMore(string clearingID)
		{
			if (!clearingSpawnCounts.ContainsKey(clearingID)) return false;

			int currentCount = clearingSpawnCounts[clearingID];
			int maxAllowed = Mathf.Min(maxLilGuysPerClearing, clearingSpawners[clearingID].Count * 3); // Ensure it doesn't exceed per-spawner limits.

			return currentCount < maxAllowed;
		}


		/// <summary>
		/// Registers a new Lil Guy spawn within a clearing, increasing the count.
		/// </summary>
		public void RegisterSpawn(string clearingID)
		{
			if (clearingSpawnCounts.ContainsKey(clearingID))
			{
				clearingSpawnCounts[clearingID]++;
			}
		}

		/// <summary>
		/// Deregisters a Lil Guy spawn when one is removed, reducing the count.
		/// </summary>
		public void DeregisterSpawn(string clearingID)
		{
			if (clearingSpawnCounts.ContainsKey(clearingID) && clearingSpawnCounts[clearingID] > 0)
			{
				clearingSpawnCounts[clearingID]--;
			}
		}

		/// <summary>
		/// Randomly selects a legendary spawner and triggers its SpawnLegendary() method.
		/// Ensures that legendary Lil Guys are spawned independently of clearing spawn limits.
		/// </summary>
		public void SpawnLegendaryLilGuy(float maxScale, int level)
		{

			SpawnerObj selectedSpawner = legendarySpawnersSB.Next();
			selectedSpawner.SpawnLegendary(maxScale, level);

		}
	}
}
