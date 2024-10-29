using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: To manage playing Audio (both Music and SFX)
namespace Managers
{
    public class SpawnManager : SingletonBase<SpawnManager>
    {
        //Serialized fields
        [SerializeField] public List<GameObject> forestLilGuys;
        [SerializeField] public List<GameObject> mountainLilGuys;
        [SerializeField] public List<GameObject> beachLilGuys;
        [SerializeField] public List<GameObject> legendaryLilGuys;
        [SerializeField] public List<GameObject> forestSpawners;
        [SerializeField] public List<GameObject> mountainSpawners;
        [SerializeField] public List<GameObject> beachSpawners;
        [SerializeField] public SpawnerObj legendarySpawner;
        [SerializeField] public int maxNumSpawns;
        [SerializeField] public int maxSpawnsPerArea;
        [SerializeField] public int minSpawnsPerArea;
        [SerializeField] public float spawnDelay;

        //public variables
        public int currNumSpawns;
        public int currForestSpawns;
        public int currMountainSpawns;
        private int currBeachSpawns;

        private void Start()
        {
            currNumSpawns = 0;
            currForestSpawns = 0;
            currMountainSpawns = 0;
            currBeachSpawns = 0;
            StartCoroutine(InitialSpawns());
        }


        /// <summary>
        /// This method gets called by the different biome despawn methods, and simply deletes the given Lil Guy,
        /// then it spawns a new one if any of the biomes have less than their minimum spawns.
        /// Note: May want to have it choose the biome with the least Lil Guys, when it spawns a new one
        /// </summary>
        public void DespawnLilGuy(GameObject theLilGuy)
        {
            if (currNumSpawns > 0)
            {
                currNumSpawns--;
            }

            Destroy(theLilGuy);

            if (currForestSpawns < minSpawnsPerArea)
            {
                StartCoroutine(respawnWithDelay(0));
            }
            else if (currMountainSpawns < minSpawnsPerArea)
            {
                StartCoroutine(respawnWithDelay(1));
            }
            else if (currBeachSpawns < minSpawnsPerArea)
            {
                StartCoroutine(respawnWithDelay(2));
            }
        }

        /// <summary>
        /// This method simply spawns a random Forest Lil Guy at a random forest spawner.
        /// </summary>
        public void SpawnForest()
        {
            SpawnerObj pointToSpawn;
            GameObject theLilGuy;

            theLilGuy = RandFromList(forestLilGuys);
            pointToSpawn = RandFromList(forestSpawners).GetComponent<SpawnerObj>();
            if((currForestSpawns + 1) <= maxSpawnsPerArea && (currNumSpawns + 1) <= maxNumSpawns)
            {
                pointToSpawn.SpawnLilGuy(theLilGuy);
                currForestSpawns++;
            }
        }

        /// <summary>
        /// This method simply spawns a random Mountain Lil Guy at a random mountain spawner.
        /// </summary>
        public void SpawnMountain()
        {
            SpawnerObj pointToSpawn;
            GameObject theLilGuy;

            theLilGuy = RandFromList(mountainLilGuys);
            pointToSpawn = RandFromList(mountainSpawners).GetComponent<SpawnerObj>();
            if ((currForestSpawns + 1) <= maxSpawnsPerArea && (currNumSpawns + 1) <= maxNumSpawns)
            {
                pointToSpawn.SpawnLilGuy(theLilGuy);
                currMountainSpawns++;
            }
        }

        /// <summary>
        /// This method simply spawns a random Beach Lil Guy at a random beach spawner.
        /// </summary>
        public void SpawnBeach()
        {
            SpawnerObj pointToSpawn;
            GameObject theLilGuy;

            theLilGuy = RandFromList(beachLilGuys);
            pointToSpawn = RandFromList(beachSpawners).GetComponent<SpawnerObj>();
            if ((currForestSpawns + 1) <= maxSpawnsPerArea && (currNumSpawns + 1) <= maxNumSpawns)
            {
                pointToSpawn.SpawnLilGuy(theLilGuy);
                currBeachSpawns++;
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
            currForestSpawns--;
            DespawnLilGuy(theLilGuy);
        }

        /// <summary>
        /// This coroutine handles spawning a random number of Lil Guys in each biome 
        /// (in the range of how many spawns are allowed for each biome)
        /// </summary>
        private IEnumerator InitialSpawns()
        {
            int numForestSpawns = Random.Range(minSpawnsPerArea,maxSpawnsPerArea + 1);
            int numMountainSpawns = Random.Range(minSpawnsPerArea, maxSpawnsPerArea + 1);
            int numBeachSpawns = Random.Range(minSpawnsPerArea, maxSpawnsPerArea + 1);
            while (currNumSpawns < maxNumSpawns)
            {
                int biomeNum = Random.Range(0, 3);
                if (biomeNum == 0 && currForestSpawns < numForestSpawns)
                {
                    yield return new WaitForSeconds(spawnDelay);
                    SpawnForest();
                }
                else if(biomeNum == 1 && currMountainSpawns < numMountainSpawns)
                {
                    yield return new WaitForSeconds(spawnDelay);
                    SpawnMountain();
                }
                else if (biomeNum == 2 && currBeachSpawns < numBeachSpawns)
                {
                    yield return new WaitForSeconds(spawnDelay);
                    SpawnBeach();
                }
            }
            
        }
        /// <summary>
        /// Coroutine that spawns a NEW Lil Guy at the given biome (given by an int between 0 and 2 inclusive)
        /// It waits for spawnDelay
        /// </summary>
        /// <param name="biomeNum"></param>
        /// <returns></returns>
        private IEnumerator respawnWithDelay(int biomeNum)
        {
            
            switch (biomeNum)
            {
                case 0:
                    yield return new WaitForSeconds(spawnDelay);
                    SpawnForest();
                    break;
                case 1:
                    yield return new WaitForSeconds(spawnDelay);
                    SpawnMountain();
                    break;
                case 2:
                    yield return new WaitForSeconds(spawnDelay);
                    SpawnBeach();
                    break;
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
