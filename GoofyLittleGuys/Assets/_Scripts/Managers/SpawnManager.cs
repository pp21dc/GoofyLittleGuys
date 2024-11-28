using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: To manage spawning Lil Guys at the correct Spawners
namespace Managers
{
	public class SpawnManager : SingletonBase<SpawnManager>
	{
		//Serialized fields
		[SerializeField] private List<GameObject> forestLilGuys;
		[SerializeField] private List<GameObject> legendaryLilGuys;
		[SerializeField] private List<GameObject> forestSpawners;
		[SerializeField] private SpawnerObj legendarySpawner;
		[SerializeField] private int maxNumSpawns;
		[SerializeField] private int minNumSpawns;
		[SerializeField] private float spawnDelay;

		//public variables
		public int currNumSpawns;



		private void Start()
		{
			currNumSpawns = 0;
			StartCoroutine(InitialSpawns());
		}

		public void RemoveLilGuyFromSpawns()
		{
			currNumSpawns--;
			if (currNumSpawns < maxNumSpawns)
			{
				StartCoroutine(RespawnWithDelay());
			}
		}

		/// <summary>
		/// This method simply spawns a random Forest Lil Guy at a random forest spawner.
		/// </summary>
		/// 
		public void SpawnForest()
		{
			if (GameManager.Instance.CurrentPhase == 1)
			{
				SpawnerObj pointToSpawn;
				GameObject theLilGuy;

				theLilGuy = RandFromList(forestLilGuys);
				pointToSpawn = RandFromList(forestSpawners).GetComponent<SpawnerObj>();
				if (currNumSpawns < maxNumSpawns)
				{
					pointToSpawn.SpawnLilGuy(theLilGuy);
				}
			}

		}

		/// <summary>
		/// Method that spawns a random Legendary at the provided legendarySpawner
		/// </summary>
		public void SpawnLegendaryGuy()
		{
			GameObject theLegendary = RandFromList(legendaryLilGuys);
			legendarySpawner.SpawnLilGuy(theLegendary);

		}

		/// <summary>
		/// This coroutine handles spawning a random number of Lil Guys in each biome 
		/// (in the range of how many spawns are allowed for each biome)
		/// </summary>
		private IEnumerator InitialSpawns()
		{
			int numForestSpawns = Random.Range(minNumSpawns, maxNumSpawns + 1);

			// Track the number of spawn attempts to avoid an infinite loop
			while (currNumSpawns < maxNumSpawns)
			{
				yield return new WaitForSeconds(spawnDelay);
				SpawnForest();
				currNumSpawns++;
			}
		}

		/// <summary>
		/// Coroutine that spawns a NEW Lil Guy into the forest.
		/// It waits for spawnDelay
		/// </summary>
		/// <param name="biomeNum"></param>
		/// <returns></returns>
		private IEnumerator RespawnWithDelay()
		{

			// Track the number of spawn attempts to avoid an infinite loop
			while (currNumSpawns < maxNumSpawns)
			{
				yield return new WaitForSeconds(spawnDelay);
				SpawnForest();
				currNumSpawns++;
			}

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

	}

}
