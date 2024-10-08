using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: Spawn Lil Guys within a given distance from this object, after a randomized interval (with given min and max value)
public class SpawnerObj : MonoBehaviour
{
    [SerializeField]
    private float minTime;
    [SerializeField]
    private float maxTime;
    [SerializeField]
    private List<GameObject> spawnList;
    [SerializeField]
    private float spawnRadius;
    [SerializeField]
    private int maxNumSpawns;

    private float spawnInterval;
    private int spawnIndex;
    private int currNumSpawns;

    // Getters and Setters:
    public List<GameObject> GetSpawnList()
    {
        return spawnList;
    }
    public void SetSpawnList(List<GameObject> givenList)
    {
        spawnList = givenList;
    }
    public float GetMinTime()
    {
        return minTime;
    }
    public void SetMinTime(float givenTime)
    {
        minTime = givenTime;
    }
    public float GetMaxTime()
    {
        return maxTime;
    }
    public void SetMaxTime(float givenTime)
    {
        maxTime = givenTime;
    }
    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnLilGuys());
        currNumSpawns = 0;
    }

    // Coroutine to handle spawning Lil Guys
    private IEnumerator SpawnLilGuys()
    {
        // get a random interval
        RandInterval(minTime, maxTime);
        // wait that interval
        yield return new WaitForSeconds(spawnInterval);

        // get a random index in the spawnList
        RandIndex();

        if(currNumSpawns < maxNumSpawns)
        {
            // instantiate the Lil Guy at that index in the list, at a random location within spawnRadius of this object
            GameObject newLilGuy = Instantiate(spawnList[spawnIndex], new Vector3(Random.Range(-spawnRadius, spawnRadius), this.transform.position.y, Random.Range(-spawnRadius, spawnRadius)), Quaternion.identity);
            currNumSpawns++;
        }
       
        // repeat the process
        StartCoroutine(SpawnLilGuys());
    }
    
    

    // Method that picks a random spawn interval value, between minTime and maxTime
    public void RandInterval(float minTime, float maxTime)
    {
        spawnInterval = Random.Range(minTime, maxTime);
    }

    // Method that picks a random index within the spawn list
    public void RandIndex()
    {
        spawnIndex = Random.Range(0, (spawnList.Count - 1));
    }
}
