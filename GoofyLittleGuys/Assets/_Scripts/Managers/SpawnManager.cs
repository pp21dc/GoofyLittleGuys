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
        

        /// <summary>
        /// This method should ALWAYS get called when a wild Lil Guy despawns
        /// (I.E. is defeated/captured etc)
        /// </summary>
        public void LilGuyDespawned()
        {
            currNumSpawns--;
            if (currNumSpawns < maxNumSpawns)
            {
                if (currForestSpawns < minSpawnsPerArea)
                {
                    SpawnForest();
                }
            }
        }

        public void SpawnForest()
        {
            SpawnerObj pointToSpawn;
            GameObject theLilGuy;

            theLilGuy = RandFromList(forestLilGuys);
            pointToSpawn = RandFromList(forestSpawners).GetComponent<SpawnerObj>();
            pointToSpawn.SpawnLilGuy(theLilGuy);
            currForestSpawns++;
        }

        /// <summary>
        /// This method accepts a list of objects and returns a random one.
        /// </summary>
        /// <param name="theList"></param>
        /// <returns> GameObject </returns>
        public GameObject RandFromList(List<GameObject> theList)
        {
            //int randomIndex = Random.Range(0,(theList.Count-1));
            return theList[Random.Range(0, (theList.Count - 1))];

        }
    }
    
}
