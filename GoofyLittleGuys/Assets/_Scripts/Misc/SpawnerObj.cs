using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: Spawn Lil Guys within a given distance from this object, after a randomized interval (with given min and max value)
public class SpawnerObj : MonoBehaviour
{
    
    [SerializeField]
    private float spawnRadius;


    /// <summary>
    /// This method accepts a Lil Guy (or any object) and spawns it
    /// at a random point within this spawner's radius.
    /// </summary>
    /// <param name="newLilGuy"></param>
    /// <returns></returns>
    public void SpawnLilGuy(GameObject newLilGuy)
    {
        Instantiate(newLilGuy, new Vector3(RandPos(this.transform.position.x), this.transform.position.y, RandPos(this.transform.position.z)), Quaternion.identity);

        Managers.SpawnManager.Instance.currNumSpawns++;
    }

    /// <summary>
    /// This method gets a random position within spawnRadius, with an added offset
    /// </summary>
    ///  /// <param name="offset"></param>
    /// <returns>float</returns>
    public float RandPos(float offset)
    {
        return (Random.Range(-spawnRadius, spawnRadius)) + offset;
    }
}
