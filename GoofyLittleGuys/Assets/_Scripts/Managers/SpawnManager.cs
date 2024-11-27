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

		private void OnDestroy()
		{
		}

		/// <summary>
		/// This method gets called by the forest despawn method, and simply deletes the given Lil Guy,
		/// then it spawns a new one at a random spawner in the world.
		/// Note: May want to have it choose the biome with the least Lil Guys, when it spawns a new one
		/// </summary>
		public void DespawnLilGuy(GameObject theLilGuy)
        {
            if (currNumSpawns > 0)
            {
                currNumSpawns--;
            }

            if (currNumSpawns < minNumSpawns)
            {
                StartCoroutine(respawnWithDelay());
            }
            
        }

		/// <summary>
		/// This method simply spawns a random Forest Lil Guy at a random forest spawner.
		/// </summary>
		/// 
		public void SpawnForest()
		{
            if(GameManager.Instance.CurrentPhase == 1)
            {
                SpawnerObj pointToSpawn;
                GameObject theLilGuy;

                theLilGuy = RandFromList(forestLilGuys);
                pointToSpawn = RandFromList(forestSpawners).GetComponent<SpawnerObj>();
                if ((currNumSpawns + 1) <= maxNumSpawns)
                {
                    pointToSpawn.SpawnLilGuy(theLilGuy);
                    currNumSpawns++;
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
        /// This method should ALWAYS get called when a FOREST Lil Guy gets despawned.
        /// (I.E. theLilGuy is defeated/tamed etc)
        /// </summary>
        public void DespawnForest(GameObject theLilGuy)
        {
            DespawnLilGuy(theLilGuy);
        }

        public void StartInitialSpawns()
        {
            Debug.Log("Yes");
            StartCoroutine(InitialSpawns());
        }

		/// <summary>
		/// This coroutine handles spawning a random number of Lil Guys in each biome 
		/// (in the range of how many spawns are allowed for each biome)
		/// </summary>
		private IEnumerator InitialSpawns()
		{
			int numForestSpawns = Random.Range(minNumSpawns, maxNumSpawns + 1);

			// Track the number of spawn attempts to avoid an infinite loop
			int spawnAttempts = 0;
			int maxSpawnAttempts = maxNumSpawns * 2;  // Allow some retries

			while (currNumSpawns < maxNumSpawns && spawnAttempts < maxSpawnAttempts)
			{

				if (currNumSpawns < numForestSpawns)
				{
					yield return new WaitForSeconds(spawnDelay);
					SpawnForest();
				}
				

				spawnAttempts++;
			}

			if (spawnAttempts >= maxSpawnAttempts)
			{
				Debug.LogWarning("Max spawn attempts reached in InitialSpawns.");
			}
		}
		/// <summary>
		/// Coroutine that spawns a NEW Lil Guy into the forest.
		/// It waits for spawnDelay
		/// </summary>
		/// <param name="biomeNum"></param>
		/// <returns></returns>
		private IEnumerator respawnWithDelay()
        {

            yield return new WaitForSeconds(spawnDelay);
            SpawnForest();
            
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
