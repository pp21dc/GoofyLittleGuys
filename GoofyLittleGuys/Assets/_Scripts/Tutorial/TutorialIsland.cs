using System.Collections.Generic;
using UnityEngine;

public class TutorialIsland : MonoBehaviour
{
    /// <summary>
    /// this is just a ref holder for all GO's on a specific island, reference-able by each state
    /// </summary>
    
    public Transform spawnPoint;
    public GameObject lilGuyPref;
    public Transform enemySpawnPoint;
    public GameObject storm;
    public GameObject berryBush;
    public GameObject fountain;
    public GameObject exitPortal;
    
    public List<GameObject> enemies;
}