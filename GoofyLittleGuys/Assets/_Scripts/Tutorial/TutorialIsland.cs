using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialIsland : MonoBehaviour
{
    /// <summary>
    /// this is just a ref holder for all GO's on a specific island, reference-able by each state
    /// </summary>
    
    public GameObject lilGuyPref;
    public Transform enemySpawnPoint;
    public GameObject storm;
    public GameObject fountain;
    public GameObject exitPortal;
    
    public List<GameObject> enemies;

    private void Start()
    {
        exitPortal.SetActive(false);
        fountain.GetComponent<TutorialFountain>().interactArea.enabled = false;
    }
}