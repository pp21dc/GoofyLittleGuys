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

    public LayerMask GroundLayer;

    /// <summary>
    /// This method accepts a Lil Guy (or any object) and spawns it
    /// at a random point within this spawner's radius.
    /// </summary>
    /// <param name="newLilGuy"></param>
    /// <returns></returns>
    public void SpawnLilGuy(GameObject newLilGuy)
    {
        //int failedAttempts = 0;
        Vector3 spawningPos = PickValidSpot();

        Instantiate(newLilGuy, spawningPos, Quaternion.identity);
        

        Managers.SpawnManager.Instance.currNumSpawns++;
    }

    /// <summary>
    /// Simply returns a random value to serve as the position within one axis
    /// (use multiple times to form a random position vector)
    /// </summary>
    ///  /// <param name="offset"></param> -> Should be the spawner's position in desired axis
    /// <returns>float</returns>
    public float RandPos(float offset)
    {
        return (Random.Range(-spawnRadius, spawnRadius)) + offset;
    }

    /// <summary>
    /// This me
    /// </summary>
    /// <returns>Vector3</returns>
    private Vector3 PickValidSpot()
    {
        Vector3 spawnPos = new Vector3(RandPos(transform.position.x), transform.position.y, RandPos(transform.position.z));
        
        Vector3 checkVector = spawnPos - transform.position;
        Vector3 checkDir = checkVector.normalized;
        
        Ray ray = new Ray(transform.position, checkDir);
        if(Physics.Raycast(ray, out RaycastHit hit, spawnRadius, GroundLayer))
        {
            spawnPos = hit.point;
            spawnPos.y += 2;
            return spawnPos;
        }
        else
        {
            return spawnPos;
        }
    }

    
}
