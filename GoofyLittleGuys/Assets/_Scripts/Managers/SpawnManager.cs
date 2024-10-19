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
        [SerializeField] public List<GameObject> forestLilGuys;
        [SerializeField] public List<GameObject> forestSpawners;
        [SerializeField] public int maxNumSpawns;
        [SerializeField] public int maxSpawnsPerArea;
        [SerializeField] public int minSpawnsPerArea;
        [SerializeField] public int currForestSpawns;
        [SerializeField] public int currNumSpawns;
        [SerializeField] public float spawnDelay;

        private void Start()
        {
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
                SpawnForest();
            }
        }

        /// <summary>
        /// This method/coroutine simply spawns a random Forest Lil Guy at a random forest spawner.
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
        public IEnumerator InitialSpawns()
        {
            int numForestSpawns = Random.Range(minSpawnsPerArea,maxSpawnsPerArea);
            while (currForestSpawns < numForestSpawns)
            {
                yield return new WaitForSeconds(spawnDelay);
                SpawnForest();
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
