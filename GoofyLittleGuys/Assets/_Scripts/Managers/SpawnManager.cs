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
		private Dictionary<string, List<SpawnerObj>> clearingSpawners = new();  // Tracks spawners per clearing
		private Dictionary<string, int> clearingSpawnCounts = new();            // Tracks active Lil Guys per clearing
		private List<SpawnerObj> legendarySpawners = new();                     // Tracks all legendary spawners

		[SerializeField] private float gracePeriod = 5f;

		private ShuffleBag<SpawnerObj> legendarySpawnersSB = new();
		public float GracePeriod { get => gracePeriod; set => gracePeriod = value; }

		private void Start()
		{

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
				return;
			}

			if (!clearingSpawners.ContainsKey(clearingID))
			{
				clearingSpawners[clearingID] = new List<SpawnerObj>();
				clearingSpawnCounts[clearingID] = 0;
			}

			clearingSpawners[clearingID].Add(spawner);
			EnsureMinimumSpawns(clearingID);
		}

		/// <summary>
		/// Ensures that each spawner in a clearing has spawned at least one Lil Guy.
		/// </summary>
		private void EnsureMinimumSpawns(string clearingID)
		{
			if (clearingSpawners.ContainsKey(clearingID))
			{
				foreach (SpawnerObj spawner in clearingSpawners[clearingID])
				{
					if (spawner.CurrentSpawnCount == 0)
					{
						spawner.SpawnRandLilGuy();
					}
				}
			}
		}

		/// <summary>
		/// Checks if a clearing can spawn more Lil Guys, ensuring it does not exceed the cap of 3 per spawner.
		/// </summary>
		public bool CanSpawnMore(string clearingID)
		{
			return clearingSpawnCounts.ContainsKey(clearingID) && clearingSpawnCounts[clearingID] < clearingSpawners[clearingID].Count * 1;
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
